using PhishingGuard.Core.DTOs;
using PhishingGuard.Core.Enums;
using PhishingGuard.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PhishingGuard.Engine.Services
{
    public class RuleBasedEngine : IPhishingAnalyzer
    {
        // 1. Whitelist (Hızlı arama için HashSet)
        private readonly HashSet<string> _whiteListDomains;

        // 2. Hedef Markalar (List)
        private readonly List<string> _targetBrands;

        // 3. Tehlikeli Kelimeler (List)
        private readonly List<string> _suspiciousKeywords;

        public RuleBasedEngine()
        {
            _whiteListDomains = new HashSet<string>();
            _targetBrands = new List<string>();
            _suspiciousKeywords = new List<string>();

            LoadAllData();
        }

        private void LoadAllData()
        {
            var whitelist = LoadListFromJson("whitelist.json");
            foreach (var item in whitelist) _whiteListDomains.Add(item);

            var brands = LoadListFromJson("target_brands.json");
            _targetBrands.AddRange(brands);

            var keywords = LoadListFromJson("suspicious_keywords.json");
            _suspiciousKeywords.AddRange(keywords);
        }

        // YARDIMCI METOT: Verilen dosya adını okur ve liste döner
        private List<string> LoadListFromJson(string fileName)
        {
            var list = new List<string>();
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
                if (File.Exists(path))
                {
                    string jsonContent = File.ReadAllText(path);
                    var data = JsonSerializer.Deserialize<List<string>>(jsonContent);
                    if (data != null)
                    {
                        list.AddRange(data.Select(x => x.Trim().ToLower()));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dosya yüklenirken hata ({fileName}): {ex.Message}");
            }
            return list;
        }

        public ScanResult Analyze(string url)
        {
            var result = new ScanResult { Url = url, DetectionSource = "RuleEngine" };
            string lowerUrl = url.ToLower();
            string domain = GetDomainFromUrl(url);

            // --- AŞAMA 1: GÜVENLİ LİSTE KONTROLÜ ---
            bool isWhitelisted = _whiteListDomains.Contains(domain);

            if (!isWhitelisted)
                isWhitelisted = _whiteListDomains.Any(w => domain.EndsWith("." + w));

            // Evrensel Resmi Uzantı ve HTTPS Kontrolü
            if (!isWhitelisted)
            {
                // Regex ile uzantı kontrolü (.gov, .edu, .mil, .k12)
                string trustedPattern = @"\.(gov|edu|mil|k12)(\.[a-z]{2})?$";

                if (Regex.IsMatch(domain, trustedPattern))
                {
                    //  Uzantı doğru olsa bile protokol HTTPS Konturolü
                    if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        // Hem resmi uzantı hem HTTPS -> Güvenli
                        isWhitelisted = true;
                    }
                    else
                    {
                        // Uzantı resmi ama bağlantı GÜVENSİZ (HTTP)
                        result.RiskScore += 40;
                        result.DetectionDetails.Add("Resmi kurum uzantılı site (gov/edu) şifrelenmemiş (HTTP) bağlantı kullanıyor.");
                    }
                }
            }

            if (isWhitelisted)
            {
                result.IsPhishing = false;
                result.RiskLevel = RiskLevel.Safe;
                result.RiskScore = 0;
                result.DetectionDetails.Add("Alan adı güvenli listede veya resmi kurum uzantısına sahip.");
                result.DetectionSource = "Beyaz Liste (Whitelist)";
                return result;
            }

            // --- AŞAMA 2: TEHDİT ANALİZİ ---

            // IP Kontrolü
            if (Regex.IsMatch(url, @"http(s)?://\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
            {
                result.IsPhishing = true;
                result.RiskLevel = RiskLevel.Malicious;
                result.RiskScore = 100;
                result.DetectionDetails.Add("Alan adı yerine doğrudan IP adresi kullanılmış.");
                return result;
            }

            // Marka Taklidi 
            foreach (var brand in _targetBrands)
            {
                if (domain.Contains(brand) && !domain.Equals(brand + ".com") && !domain.Equals("www." + brand + ".com"))
                {
                    result.RiskScore += 30;
                    result.DetectionDetails.Add($"'{brand}' markası resmi olmayan bir domain içinde geçiyor.");
                }

                string pureDomain = domain.Replace(".com", "").Replace("www.", "").Replace(".net", "").Replace(".org", "");
                int distance = ComputeLevenshteinDistance(pureDomain, brand);

                if (distance > 0 && distance <= 2)
                {
                    result.RiskScore += 50;
                    result.DetectionDetails.Add($"Bu adres '{brand}' sitesini taklit ediyor olabilir! (Benzerlik saptandı).");
                    result.RiskLevel = RiskLevel.HighRisk;
                }
            }

            // Tehlikeli Kelimeler 
            int keywordCount = 0;
            foreach (var keyword in _suspiciousKeywords)
            {
                if (lowerUrl.Contains(keyword))
                {
                    keywordCount++;
                    result.DetectionDetails.Add($"URL içinde şüpheli işlem kelimesi bulundu: '{keyword}'");
                }
            }
            if (keywordCount > 0) result.RiskScore += (keywordCount * 15);

            // Uzunluk
            if (url.Length > 75)
            {
                result.RiskScore += 10;
                result.DetectionDetails.Add("URL şüpheli derecede uzun.");
            }
            if (url.Count(c => c == '-') > 3)
            {
                result.RiskScore += 10;
                result.DetectionDetails.Add("URL içinde çok fazla tire (-) işareti var.");
            }

            // --- AŞAMA 3: SONUÇ ---
            if (result.RiskScore > 100) result.RiskScore = 100;

            if (result.RiskScore >= 80) result.RiskLevel = RiskLevel.Malicious;
            else if (result.RiskScore >= 50) result.RiskLevel = RiskLevel.Suspicious;
            else if (result.RiskScore == 0) result.RiskLevel = RiskLevel.Safe;

            return result;
        }

        private string GetDomainFromUrl(string url)
        {
            try
            {
                if (!url.StartsWith("http")) url = "http://" + url;
                var uri = new Uri(url);
                return uri.Host.ToLower();
            }
            catch { return url; }
        }

        private int ComputeLevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
            if (string.IsNullOrEmpty(t)) return s.Length;
            int n = s.Length; int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;
            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            return d[n, m];
        }
    }
}
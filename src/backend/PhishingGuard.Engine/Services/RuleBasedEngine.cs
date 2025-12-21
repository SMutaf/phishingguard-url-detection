using PhishingGuard.Core.DTOs;
using PhishingGuard.Core.Enums;
using PhishingGuard.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhishingGuard.Engine.Services
{
    public class RuleBasedEngine : IPhishingAnalyzer
    {
        // korunan markalar
        private readonly List<string> _targetBrands = new List<string>
        {
            "google", "facebook", "instagram", "twitter", "trendyol", "netflix",
            "microsoft", "apple", "amazon", "linkedin", "ziraat", "garanti", "isbank"
        };

        // tehlikeli kelimeler
        private readonly List<string> _suspiciousKeywords = new List<string>
        {
            "login", "signin", "verify", "secure", "account", "update",
            "confirm", "banking", "service", "bonus", "free", "admin"
        };

        public ScanResult Analyze(string url)
        {
            var result = new ScanResult { Url = url, DetectionSource = "RuleEngine" };
            string lowerUrl = url.ToLower();
            string domain = GetDomainFromUrl(url);

            // güvenilir site uzantılıra ai gönderip yanlış alarm almayı engelleriz
            if (domain.EndsWith(".edu.tr") || domain.EndsWith(".gov.tr") || domain.EndsWith(".k12.tr") || domain.EndsWith("beun.edu.tr"))
            {
                result.IsPhishing = false;
                result.RiskLevel = RiskLevel.Safe;
                result.RiskScore = 0;
                result.DetectionDetails.Add("Alan adı güvenilir kurum uzantısı (.edu.tr / .gov.tr) taşıyor.");
                result.DetectionSource = "Beyaz Liste (Whitelist)";
                return result; // Direkt dön, AI'a gitmeye gerek yok!
            }

            // ip adersi kontolü
            if (System.Text.RegularExpressions.Regex.IsMatch(url, @"http(s)?://\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
            {
                result.IsPhishing = true;
                result.RiskLevel = RiskLevel.Malicious;
                result.RiskScore = 100;
                result.DetectionDetails.Add("Alan adı yerine doğrudan IP adresi kullanılmış.");
                return result; 
            }

            // marka taklidi

            foreach (var brand in _targetBrands)
            {

                if (domain.Contains(brand) && !domain.Equals(brand + ".com") && !domain.Equals("www." + brand + ".com"))
                {
                    // marka adı geçmesine rağmen site orjinal değil rik puanını artırır.
                    result.RiskScore += 30;
                    result.DetectionDetails.Add($"'{brand}' markası resmi olmayan bir domain içinde geçiyor.");
                }

                // benzerlik hesabı
                int distance = ComputeLevenshteinDistance(domain.Replace(".com", "").Replace("www.", ""), brand);

                // benzerlik kontrolü
                if (distance > 0 && distance <= 2)
                {
                    result.RiskScore += 50;
                    result.DetectionDetails.Add($"Bu adres '{brand}' sitesini taklit ediyor olabilir! (Benzerlik saptandı).");
                    result.RiskLevel = RiskLevel.HighRisk;
                }
            }

            // tehlikeli kelimeler
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

            // uzunluk ve karmaşıklık kontrolü
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

            // sonuç hesapla
            if (result.RiskScore > 100) result.RiskScore = 100;

            // Risk Seviyesini Belirle, esas karar servicede
            if (result.RiskScore >= 80) result.RiskLevel = RiskLevel.Malicious;
            else if (result.RiskScore >= 50) result.RiskLevel = RiskLevel.Suspicious;
            else if (result.RiskScore == 0) result.RiskLevel = RiskLevel.Safe;

            return result;
        }

        // urlden sadece domain kısmını açıklar
        private string GetDomainFromUrl(string url)
        {
            try
            {
                if (!url.StartsWith("http")) url = "http://" + url;
                var uri = new Uri(url);
                return uri.Host.ToLower();
            }
            catch
            {
                return url;
            }
        }

        // iki kelime arasındaki farkı matematiksel olarak hesaplar 
        private int ComputeLevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}
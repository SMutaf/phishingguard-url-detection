using PhishingGuard.Core.DTOs;
using PhishingGuard.Core.Enums;
using PhishingGuard.Core.Interfaces;
using System.Collections.Generic;

namespace PhishingGuard.Engine.Services
{
    public class RuleBasedEngine : IPhishingAnalyzer
    {
        public ScanResult Analyze(string url)
        {
            var result = new ScanResult { Url = url, DetectionSource = "RuleEngine" };

            // 1. Kural: IP Adresi Kontrolü (Basit örnek)
            // Gerçek IP regex'ini sonra ekleriz, şimdilik mantık:
            if (url.Contains("http://192.") || url.Contains("http://10."))
            {
                result.IsPhishing = true;
                result.RiskLevel = RiskLevel.Malicious;
                result.RiskScore = 100;
                result.DetectionDetails.Add("Alan adı yerine doğrudan IP adresi kullanılmış.");
                return result; // Kritik hata varsa direkt dön
            }

            // 2. Kural: Uzunluk
            if (url.Length > 75)
            {
                result.RiskScore += 20;
                result.DetectionDetails.Add("URL şüpheli derecede uzun.");
            }

            // Başka kural yoksa varsayılan
            if (result.RiskScore == 0) result.RiskLevel = RiskLevel.Safe;

            return result;
        }
    }
}
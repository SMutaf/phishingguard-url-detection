using PhishingGuard.Core.Enums;
using System.Collections.Generic;

namespace PhishingGuard.Core.DTOs
{
    public class ScanResult
    {
        public string Url { get; set; }
        public bool IsPhishing { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public double RiskScore { get; set; } // 0 - 100 arası puan

        // Kullanıcıya göstereceğimiz "Neden?" listesi
        public List<string> DetectionDetails { get; set; } = new List<string>();

        // Hangi motorun yakaladığı (Rule Engine mi, AI mı?)
        public string DetectionSource { get; set; }
    }
}
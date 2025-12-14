using PhishingGuard.Core.Enums;
using System.Collections.Generic;

namespace PhishingGuard.Core.DTOs
{
    public class ScanResult
    {
        public string Url { get; set; }
        public bool IsPhishing { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public double RiskScore { get; set; } 

        public List<string> DetectionDetails { get; set; } = new List<string>();

        public string DetectionSource { get; set; }
    }
}
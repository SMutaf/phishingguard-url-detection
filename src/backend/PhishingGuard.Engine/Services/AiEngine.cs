using PhishingGuard.Core.DTOs;
using PhishingGuard.Core.Enums;
using PhishingGuard.Core.Interfaces;

namespace PhishingGuard.Engine.Services
{
    public class AiEngine : IPhishingAnalyzer
    {
        // Burada ileride MLModel.zip yüklenecek

        public ScanResult Analyze(string url)
        {
            var result = new ScanResult { Url = url, DetectionSource = "AI_Model_v1" };

            // ŞİMDİLİK MOCK (SAHTE) DATA DÖNÜYORUZ
            // İleride buraya _predictionEngine.Predict() gelecek.

            // Örnek simülasyon: URL içinde 'login' geçiyorsa yapay zeka tetiklensin
            if (url.Contains("login"))
            {
                result.RiskScore = 85;
                result.RiskLevel = RiskLevel.HighRisk;
                result.IsPhishing = true;
                result.DetectionDetails.Add("AI Modeli: URL karakter dizilimi phishing paternleriyle eşleşiyor (%85).");
            }
            else
            {
                result.RiskScore = 10;
                result.RiskLevel = RiskLevel.LowRisk;
            }

            return result;
        }
    }
}
using PhishingGuard.Core.DTOs;

namespace PhishingGuard.Core.Interfaces
{
    public interface IPhishingAnalyzer
    {
        // Her motor (AI veya Kural) bu metodu uygulamak zorunda
        ScanResult Analyze(string url);
    }
}
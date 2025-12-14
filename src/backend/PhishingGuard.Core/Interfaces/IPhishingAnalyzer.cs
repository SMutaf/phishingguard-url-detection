using PhishingGuard.Core.DTOs;

namespace PhishingGuard.Core.Interfaces
{
    public interface IPhishingAnalyzer
    {
        ScanResult Analyze(string url);
    }
}
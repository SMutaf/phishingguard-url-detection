using PhishingGuard.Core.DTOs;
using System.Threading.Tasks;

namespace PhishingGuard.Service.Interfaces
{
    public interface IPhishingScanService
    {
        // Asenkron yapıyoruz ki API kilitlenmesin (Non-blocking)
        Task<ScanResult> ScanUrlAsync(ScanRequest request);
    }
}
using PhishingGuard.Core.DTOs;
using System.Threading.Tasks;

namespace PhishingGuard.Service.Interfaces
{
    public interface IPhishingScanService
    {
        Task<ScanResult> ScanUrlAsync(ScanRequest request);
    }
}
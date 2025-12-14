namespace PhishingGuard.Core.DTOs
{
    public class ScanRequest
    {
        public string Url { get; set; }
        // İsteğin nereden geldiği (Hover mi yaptı tıkladı mı?)
        public string ScanType { get; set; } // "Hover", "Click", "Manual"
    }
}
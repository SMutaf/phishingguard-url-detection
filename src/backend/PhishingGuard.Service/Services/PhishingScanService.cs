using PhishingGuard.Core.DTOs;
using PhishingGuard.Core.Enums;
using PhishingGuard.Engine.Services; 
using PhishingGuard.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingGuard.Service.Services
{
    public class PhishingScanService : IPhishingScanService
    {
        private readonly RuleBasedEngine _ruleEngine;
        private readonly AiEngine _aiEngine;

        public PhishingScanService(RuleBasedEngine ruleEngine, AiEngine aiEngine)
        {
            _ruleEngine = ruleEngine;
            _aiEngine = aiEngine;
        }

        public async Task<ScanResult> ScanUrlAsync(ScanRequest request)
        {
            // Sonuç nesnesi hazırlama
            var finalResult = new ScanResult
            {
                Url = request.Url,
                DetectionDetails = new List<string>()
            };

            //  Kural Motoru
            var ruleResult = _ruleEngine.Analyze(request.Url);

            // kural motorundan kesin zararlı sonucu çıktıysa  ML girmeden sonucu döndür
            if (ruleResult.RiskLevel == RiskLevel.Malicious)
            {
                return ruleResult;
            }

            //AI motorunu çalıştırır
            var aiResult = await Task.Run(() => _aiEngine.Analyze(request.Url));

            // 3. ADIM: KARAR MEKANİZMASI (Aggregation Logic)

            // Puanları birleştiriyoruz (Hangi motor daha yüksek risk verdiyse onu alalım)
            double finalScore = Math.Max(ruleResult.RiskScore, aiResult.RiskScore);

            finalResult.RiskScore = finalScore;
            finalResult.IsPhishing = finalScore >= 50; // Eşik değerimiz 50

            // Risk Seviyesini puana göre güncelle
            if (finalScore >= 90) finalResult.RiskLevel = RiskLevel.Malicious;
            else if (finalScore >= 70) finalResult.RiskLevel = RiskLevel.HighRisk;
            else if (finalScore >= 40) finalResult.RiskLevel = RiskLevel.Suspicious;
            else finalResult.RiskLevel = RiskLevel.Safe;

            // Raporları Birleştir
            finalResult.DetectionDetails.AddRange(ruleResult.DetectionDetails);
            finalResult.DetectionDetails.AddRange(aiResult.DetectionDetails);

            // Kaynak bilgisi ekle
            if (finalResult.IsPhishing)
            {
                if (ruleResult.IsPhishing && aiResult.IsPhishing)
                    finalResult.DetectionSource = "Hibrit (AI + Kurallar)";
                else if (aiResult.IsPhishing)
                    finalResult.DetectionSource = "Yapay Zeka Modeli";
                else
                    finalResult.DetectionSource = "Güvenlik Kuralları";
            }
            else
            {
                finalResult.DetectionSource = "Temiz";
            }

            return finalResult;
        }
    }
}
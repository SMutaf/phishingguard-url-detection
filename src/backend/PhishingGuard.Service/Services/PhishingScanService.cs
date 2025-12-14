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
            var finalResult = new ScanResult
            {
                Url = request.Url,
                DetectionDetails = new List<string>()
            };

            var ruleResult = _ruleEngine.Analyze(request.Url);

            if (ruleResult.RiskLevel == RiskLevel.Malicious)
            {
                return ruleResult;
            }

            var aiResult = await Task.Run(() => _aiEngine.Analyze(request.Url));

            double finalScore = Math.Max(ruleResult.RiskScore, aiResult.RiskScore);

            finalResult.RiskScore = finalScore;
            finalResult.IsPhishing = finalScore >= 50; 

            if (finalScore >= 90) finalResult.RiskLevel = RiskLevel.Malicious;
            else if (finalScore >= 70) finalResult.RiskLevel = RiskLevel.HighRisk;
            else if (finalScore >= 40) finalResult.RiskLevel = RiskLevel.Suspicious;
            else finalResult.RiskLevel = RiskLevel.Safe;

            finalResult.DetectionDetails.AddRange(ruleResult.DetectionDetails);
            finalResult.DetectionDetails.AddRange(aiResult.DetectionDetails);

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
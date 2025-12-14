using Microsoft.ML;
using PhishingGuard.Core.DTOs;
using PhishingGuard.Core.Enums;
using PhishingGuard.Core.Interfaces;
using PhishingGuard.Engine.DataModels;
using System;
using System.IO;

namespace PhishingGuard.Engine.Services
{
    public class AiEngine : IPhishingAnalyzer
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<UrlData, UrlPrediction> _predictionEngine;

        public AiEngine()
        {
            _mlContext = new MLContext();

            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModel.zip");

            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"ML Modeli bulunamadı: {modelPath}.  MLModel.zip dosyasını kontrol et.");
            }

            ITransformer trainedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);

            _predictionEngine = _mlContext.Model.CreatePredictionEngine<UrlData, UrlPrediction>(trainedModel);
        }

        public ScanResult Analyze(string url)
        {
            var result = new ScanResult { Url = url, DetectionSource = "AI_Model_v1" };

            var input = new UrlData { UrlText = url };
            var prediction = _predictionEngine.Predict(input);

            double probability = prediction.Probability; 
            double riskScore = probability * 100;

            result.RiskScore = Math.Round(riskScore, 2);
            result.IsPhishing = prediction.Prediction;

            if (riskScore >= 90) result.RiskLevel = RiskLevel.Malicious;
            else if (riskScore >= 70) result.RiskLevel = RiskLevel.HighRisk;
            else if (riskScore >= 40) result.RiskLevel = RiskLevel.Suspicious;
            else result.RiskLevel = RiskLevel.Safe;

            if (result.IsPhishing)
            {
                result.DetectionDetails.Add($"AI Modeli: URL yapısı zararlı paternlerle eşleşiyor (Güven Oranı: %{result.RiskScore}).");
            }
            else
            {
                result.DetectionDetails.Add($"AI Modeli: URL yapısı güvenli görünüyor (Risk: %{result.RiskScore}).");
            }

            return result;
        }
    }
}
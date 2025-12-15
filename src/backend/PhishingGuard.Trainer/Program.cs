using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace PhishingGuard.Trainer
{
    public class UrlDataInput
    {
        [LoadColumn(0)] public string UrlText { get; set; }
        [LoadColumn(1)] public string Label { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== PhishingGuard PRO - İyileştirilmiş Eğitim Modu ===");

            var mlContext = new MLContext(seed: 1);

            string dataPath = Path.Combine(Environment.CurrentDirectory, "dataset_clean.csv");

            if (!File.Exists(dataPath))
            {
                Console.WriteLine("dataset_clean.csv bulunamadı.");
                return;
            }

            Console.WriteLine("Veri seti yükleniyor");
            IDataView dataView = mlContext.Data.LoadFromTextFile<UrlDataInput>(
                path: dataPath,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true,
                trimWhitespace: true,
                allowSparse: true);

            Console.WriteLine("Veriler karıştırılıyor (Shuffle)...");
            var shuffledData = mlContext.Data.ShuffleRows(dataView);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(UrlDataInput.UrlText))
                .Append(mlContext.Transforms.Conversion.MapValue("LabelBool",
                    new[] {
                        new KeyValuePair<string, bool>("Phishing", true),
                        new KeyValuePair<string, bool>("phishing", true),
                        new KeyValuePair<string, bool>("bad", true),
                        new KeyValuePair<string, bool>("legitimate", false),
                        new KeyValuePair<string, bool>("good", false)
                    },
                    nameof(UrlDataInput.Label)))
                .AppendCacheCheckpoint(mlContext); 

            var trainer = mlContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: "LabelBool",
                featureColumnName: "Features",
                numberOfLeaves: 20,     
                numberOfTrees: 100,    
                learningRate: 0.2);     

            var trainingPipeline = pipeline.Append(trainer);

            Console.WriteLine("Model eğitiliyor (FastTree)... Bu işlem işlemci gücüne göre 1-3 dakika sürebilir.");

            var splitData = mlContext.Data.TrainTestSplit(shuffledData, testFraction: 0.2);
            var model = trainingPipeline.Fit(splitData.TrainSet);

            Console.WriteLine("Model test ediliyor");
            var predictions = model.Transform(splitData.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "LabelBool");

            Console.WriteLine($"\nDetaylı Sonuçlar");
            Console.WriteLine($"Doğruluk : {metrics.Accuracy:P2}");
            Console.WriteLine($"F1 Skoru: {metrics.F1Score:P2}");
            Console.WriteLine($"Alan: {metrics.AreaUnderRocCurve:P2}");

            string modelPath = Path.Combine(Environment.CurrentDirectory, "MLModel.zip");
            mlContext.Model.Save(model, shuffledData.Schema, modelPath);
            Console.WriteLine($"\nModel kaydedildi: {modelPath}");
            Console.ReadKey();
        }
    }
}
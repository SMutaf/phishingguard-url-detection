using Microsoft.ML;
using System;
using System.IO;

namespace PhishingGuard.Trainer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== PhishingGuard AI Model Eğitimi Başlıyor ===");

            var mlContext = new MLContext(seed: 0);

            string dataPath = Path.Combine(Environment.CurrentDirectory, "dataset.csv");
            if (!File.Exists(dataPath))
            {
                Console.WriteLine($"HATA: '{dataPath}' bulunamadı!");
                return;
            }

            IDataView dataView = mlContext.Data.LoadFromTextFile<UrlData>(
                path: dataPath,
                hasHeader: false,
                separatorChar: ',');

            var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);


            var pipeline = mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "Features",
                    inputColumnName: nameof(UrlData.UrlText))

                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "Features"));

            Console.WriteLine("Model eğitiliyor...");

            var model = pipeline.Fit(splitData.TrainSet);

            Console.WriteLine("Model eğitimi tamamlandı!");

            Console.WriteLine("Model test ediliyor...");
            var predictions = model.Transform(splitData.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");

            Console.WriteLine($"Doğruluk (Accuracy): {metrics.Accuracy:P2}");
            Console.WriteLine($"F1 Skoru: {metrics.F1Score:P2}");

            string modelPath = Path.Combine(Environment.CurrentDirectory, "MLModel.zip");
            mlContext.Model.Save(model, dataView.Schema, modelPath);

            Console.WriteLine($"Model başarıyla kaydedildi: {modelPath}");
            Console.WriteLine("Devam etmek için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}
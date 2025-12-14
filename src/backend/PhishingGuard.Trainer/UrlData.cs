using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace PhishingGuard.Trainer
{
    public class UrlData
    {
        [LoadColumn(0)] 
        public string UrlText { get; set; }

        [LoadColumn(1)] 
        public bool Label { get; set; }
    }

    public class UrlPrediction : UrlData
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
        public float Probability { get; set; }
    }
}

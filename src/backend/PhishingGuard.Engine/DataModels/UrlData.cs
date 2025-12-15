using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace PhishingGuard.Engine.DataModels
{
    public class UrlData
    {
        [LoadColumn(0)]
        public string UrlText { get; set; }

        [LoadColumn(1)]
        public string Label { get; set; }
    }
}

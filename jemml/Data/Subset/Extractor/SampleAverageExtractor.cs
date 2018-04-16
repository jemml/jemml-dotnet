using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Subset.Extractor
{
    public class SampleAverageExtractor : SubsetExtractor
    {
        public List<double> Extract(Sample sample)
        {
            List<double> averagedSubset = new List<double>();
            foreach (Tuple<double, double[]> sampleRow in sample.GetDataRows())
            {
                averagedSubset.Add(sampleRow.Item2.Average());
            }
            return averagedSubset;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Subset.Extractor
{
    public class SampleMaximumContourExtractor : SubsetExtractor
    {
        public List<double> Extract(Sample sample)
        {
            List<double> maximumContourSubset = new List<double>();
            foreach (Tuple<double, double[]> sampleRow in sample.GetDataRows())
            {
                maximumContourSubset.Add(sampleRow.Item2.Max());
            }
            return maximumContourSubset;
        }
    }
}

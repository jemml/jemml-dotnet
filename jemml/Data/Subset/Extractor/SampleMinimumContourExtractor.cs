using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Subset.Extractor
{
    public class SampleMinimumContourExtractor : ISubsetExtractor
    {
        public List<double> Extract(ISample sample)
        {
            List<double> minimumContourSubset = new List<double>();
            foreach (Tuple<double, double[]> sampleRow in sample.GetDataRows())
            {
                minimumContourSubset.Add(sampleRow.Item2.Min());
            }
            return minimumContourSubset;
        }
    }
}

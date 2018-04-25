using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Subset.Extractor
{
    public class SampleGRFExtractor : ISubsetExtractor
    {
        public List<double> Extract(ISample sample)
        {
            List<double[]> sampleGRF = new List<double[]>();
            double[] previousRow = Enumerable.Repeat(0.0, 88).ToArray();
            // get individual GRFs
            foreach (Tuple<double, double[]> sampleRow in sample.GetDataRows())
            {
                double[] GRF = sampleRow.Item2.Zip(previousRow, (x, y) => x + y).ToArray();
                sampleGRF.Add(GRF);
                previousRow = GRF;
            }
            // get global GRF
            List<double> globalGRFSubset = new List<double>();
            foreach (double[] GRF in sampleGRF)
            {
                globalGRFSubset.Add(Convert.ToInt32(GRF.Average()));
            }
            return globalGRFSubset;
        }
    }
}

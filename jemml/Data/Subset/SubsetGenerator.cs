using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Subset
{
    /// <summary>
    /// 
    /// </summary>
    public class SubsetGenerator
    {
        private ISample baseSample;
        private List<List<double>> subset;

        private SubsetGenerator(ISample sample)
        {
            baseSample = sample;
            subset = new List<List<double>>(new List<double>[sample.GetDataRows().Count]);
        }

        public static SubsetGenerator GenerateFromSample(ISample sample)
        {
            return new SubsetGenerator(sample);
        }

        public SubsetGenerator ApplyExtractors(ISubsetExtractor[] extractors)
        {
            foreach (ISubsetExtractor extractor in extractors)
            {
                List<double> extractedSubset = extractor.Extract(baseSample);
                for (int i = 0; i < extractedSubset.Count; i++)
                {
                    if (subset[i] == null)
                    {
                        subset[i] = new List<double>();
                    }
                    subset[i].Add(extractedSubset[i]); // add the extracted row for this subset transform (i.e. average, min, max, ...)
                }
            }
            return this;
        }

        public ISample GetSubset()
        {
            List<Tuple<double, double[]>> subsetRows = new List<Tuple<double, double[]>>(new Tuple<double, double[]>[baseSample.GetDataRows().Count]);
            for (int i = 0; i < baseSample.GetDataRows().Count; i++)
            {
                subsetRows[i] = new Tuple<double, double[]>(baseSample.GetDataRows()[i].Item1, subset[i].ToArray());
            }
            return baseSample.AcceptVisitor<ISample>(new SampleDataVisitor(subsetRows, false));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Subset
{
    public class SubsetGenerator
    {
        private Sample baseSample;
        private List<List<double>> subset;

        private SubsetGenerator(Sample sample)
        {
            baseSample = sample;
            subset = new List<List<double>>(new List<double>[sample.GetDataRows().Count]);
        }

        public static SubsetGenerator GenerateFromSample(Sample sample)
        {
            return new SubsetGenerator(sample);
        }

        public SubsetGenerator ApplyExtractors(SubsetExtractor[] extractors)
        {
            foreach (SubsetExtractor extractor in extractors)
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

        public Sample GetSubset()
        {
            List<Tuple<double, double[]>> subsetRows = new List<Tuple<double, double[]>>(new Tuple<double, double[]>[baseSample.GetDataRows().Count]);
            for (int i = 0; i < baseSample.GetDataRows().Count; i++)
            {
                subsetRows[i] = new Tuple<double, double[]>(baseSample.GetDataRows()[i].Item1, subset[i].ToArray());
            }
            return baseSample.AcceptVisitor<Sample>(new SampleDataVisitor(subsetRows, false));
        }
    }
}

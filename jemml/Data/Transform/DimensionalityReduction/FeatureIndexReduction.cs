using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.DimensionalityReduction
{
    public class FeatureIndexReduction : DimensionalityReduction
    {
        [JsonProperty]
        protected int[] extractIndexes;

        protected FeatureIndexReduction(int[] extractIndexes, Preprocessor predecessor)
            : base(predecessor)
        {
            if (extractIndexes.Min() < 0)
            {
                throw new ArgumentException("List of indices should not contain a value less than 0");
            }
            this.extractIndexes = extractIndexes;
        }

        public FeatureIndexReduction(int[] extractIndexes) : this(extractIndexes, null) { }

        protected override double[] Reduce(Sample sample)
        {
            double[] originalDimensions = sample.GetDimensions();
            if (originalDimensions.Length < extractIndexes.Max())
            {
                throw new ArgumentException("Index out of bounds");
            }
            return extractIndexes.Select(i => originalDimensions[i]).ToArray();
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new FeatureIndexReduction(extractIndexes, predecessor);
        }
    }
}

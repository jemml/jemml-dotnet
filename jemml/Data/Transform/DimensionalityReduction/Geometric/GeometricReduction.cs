using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public class GeometricReduction : DimensionalityReduction
    {
        [JsonProperty]
        protected GeometricFeatureExtractor extractor;

        protected GeometricReduction(GeometricFeatureExtractor extractor, Preprocessor predecessor)
            : base(predecessor)
        {
            this.extractor = extractor;
        }

        public GeometricReduction(GeometricFeatureExtractor extractor) : this(extractor, null) { }

        protected override double[] Reduce(Sample sample)
        {
            return extractor.ExtractFrom(sample);
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new GeometricReduction(extractor, predecessor);
        }
    }
}

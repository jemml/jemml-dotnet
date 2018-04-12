using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Subset
{
    public class SampleSubsetExtractor : Preprocessor
    {
        [JsonProperty]
        private SubsetExtractor[] subsetExtractors;

        protected SampleSubsetExtractor(Preprocessor predecessor, params SubsetExtractor[] subsetExtractors)
        {
            this.subsetExtractors = subsetExtractors;
        }

        public SampleSubsetExtractor(params SubsetExtractor[] subsetExtractors) : this(null, subsetExtractors) { }

        public Sample Extract(Sample sample)
        {
            return SubsetGenerator.GenerateFromSample(sample).ApplyExtractors(subsetExtractors).GetSubset();
        }

        protected override Sample Process(Sample sample)
        {
            return Extract(sample);
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new SampleSubsetExtractor(predecessor, subsetExtractors);
        }
    }
}

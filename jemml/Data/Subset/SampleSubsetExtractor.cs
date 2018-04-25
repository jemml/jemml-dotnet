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
        private ISubsetExtractor[] subsetExtractors;

        protected SampleSubsetExtractor(Preprocessor predecessor, params ISubsetExtractor[] subsetExtractors)
        {
            this.subsetExtractors = subsetExtractors;
        }

        public SampleSubsetExtractor(params ISubsetExtractor[] subsetExtractors) : this(null, subsetExtractors) { }

        public ISample Extract(ISample sample)
        {
            return SubsetGenerator.GenerateFromSample(sample).ApplyExtractors(subsetExtractors).GetSubset();
        }

        protected override ISample Process(ISample sample)
        {
            return Extract(sample);
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new SampleSubsetExtractor(predecessor, subsetExtractors);
        }
    }
}

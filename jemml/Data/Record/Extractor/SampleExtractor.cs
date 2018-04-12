using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Record.Extractor
{
    public abstract class SampleExtractor : Preprocessor
    {
        protected SampleExtractor(Preprocessor predecessor) : base(predecessor) { }

        public Sample Extract(Sample sample)
        {
            return sample.AcceptVisitor<Sample>(new SampleDataVisitor(ExtractRows(sample), RecalculateDuration()));
        }

        protected abstract List<Tuple<double, double[]>> ExtractRows(Sample sample);

        protected abstract bool RecalculateDuration();

        protected override Sample Process(Sample sample)
        {
            return this.Extract(sample);
        }
    }
}

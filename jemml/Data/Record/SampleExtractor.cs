using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Record
{
    public abstract class SampleExtractor : Preprocessor
    {
        protected SampleExtractor(Preprocessor predecessor) : base(predecessor) { }

        public ISample Extract(ISample sample)
        {
            return sample.AcceptVisitor<ISample>(new SampleDataVisitor(ExtractRows(sample), RecalculateDuration()));
        }

        protected abstract List<Tuple<double, double[]>> ExtractRows(ISample sample);

        protected abstract bool RecalculateDuration();

        protected override ISample Process(ISample sample)
        {
            return this.Extract(sample);
        }
    }
}

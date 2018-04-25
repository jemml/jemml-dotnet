using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Transform.DimensionalityReduction
{
    public abstract class DimensionalityReduction : Preprocessor
    {
        protected DimensionalityReduction(Preprocessor predecessor) : base(predecessor) { }

        public ISample ApplyReduction(ISample sample)
        {
            return sample.AcceptVisitor(new SampleDataVisitor(GetReducedDimensions(sample), false));
        }

        protected List<Tuple<double, double[]>> GetReducedDimensions(ISample sample)
        {
            return Reduce(sample).Select((dimension, i) => new Tuple<double, double[]>(i, new double[] { dimension })).ToList();
        }

        protected abstract double[] Reduce(ISample sample);

        protected override ISample Process(ISample sample)
        {
            return ApplyReduction(sample);
        }
    }
}

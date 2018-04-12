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

        public Sample ApplyReduction(Sample sample)
        {
            return sample.AcceptVisitor(new SampleDataVisitor(GetReducedDimensions(sample), false));
        }

        protected List<Tuple<double, double[]>> GetReducedDimensions(Sample sample)
        {
            return Reduce(sample).Select((dimension, i) => new Tuple<double, double[]>(i, new double[] { dimension })).ToList();
        }

        protected abstract double[] Reduce(Sample sample);

        protected override Sample Process(Sample sample)
        {
            return ApplyReduction(sample);
        }
    }
}

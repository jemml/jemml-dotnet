using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Transform.DimensionalityReduction
{
    public class SampleDimensionVisitor : SampleVisitor<Sample>
    {
        private List<Tuple<double, double[]>> dimensionsRows;

        public SampleDimensionVisitor(double[] dimensions)
        {
            // convert dimensions to data rows (we don't really care about the time index value at this point)
            dimensionsRows = dimensions.Select((value, i) => new Tuple<double, double[]>(i, new double[] { value })).ToList();
        }

        public Sample Accept(StandardSample sample)
        {
            return new StandardSample(sample.GetIdentifier(), sample.IsImposter(), dimensionsRows, sample.GetOrder(), sample.GetDuration().HasValue);
        }

        public Sample Accept(CrossValidatedSample sample)
        {
            return new CrossValidatedSample(sample.GetIdentifier(), sample.IsImposter(), dimensionsRows, sample.GetOrder(), sample.GetCrossValidation(), sample.GetDuration().HasValue);
        }
    }
}

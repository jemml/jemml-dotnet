using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using jemml.Utilities;

namespace jemml.Data.Transform.DimensionalityReduction
{
    public class SampleScaledDimensionVisitor : ISampleVisitor<ISample>
    {
        private double[] featureScaling;
        private double[] featureShift;

        public SampleScaledDimensionVisitor(double[] featureScaling, double[] featureShift)
        {
            this.featureScaling = featureScaling;
            this.featureShift = featureShift;
        }

        public List<Tuple<double, double[]>> GetScaledDimensions(ISample sample)
        {
            return StandardizationHelpers.GenerateTransformedData(sample.GetDimensions(), (value, i) => (value * featureScaling[i]) + featureShift[i])
                .Select((value, i) => new Tuple<double, double[]>(i, new double[] { value })).ToList();
        }

        public ISample Accept(StandardSample sample)
        {
            return new StandardSample(sample.GetIdentifier(), sample.IsImposter(), GetScaledDimensions(sample), sample.GetOrder(), sample.GetDuration().HasValue);
        }

        public ISample Accept(CrossValidatedSample sample)
        {
            return new CrossValidatedSample(sample.GetIdentifier(), sample.IsImposter(), GetScaledDimensions(sample), sample.GetOrder(), sample.GetCrossValidation(), sample.GetDuration().HasValue);
        }
    }
}

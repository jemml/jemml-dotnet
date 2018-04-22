using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public class GeometricStandardDeviationTemplate : GeometricFeatureTemplate
    {
        protected override double[] Extract(double[] intervals, double[] amplitude)
        {
            double avg = amplitude.Average();
            double sum = amplitude.Sum(d => Math.Pow(d - avg, 2));
            return new double[] { Math.Sqrt(sum / (amplitude.Length - 1)) };
        }
    }
}

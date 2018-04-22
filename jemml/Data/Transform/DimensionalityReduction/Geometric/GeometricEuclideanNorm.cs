using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public class GeometricEuclideanNorm : GeometricFeatureTemplate
    {
        protected override double[] Extract(double[] intervals, double[] amplitude)
        {
            double sqsum = amplitude.Select(d => Math.Pow(d, 2)).Sum();
            return new double[] { Math.Sqrt(sqsum) };
        }
    }
}

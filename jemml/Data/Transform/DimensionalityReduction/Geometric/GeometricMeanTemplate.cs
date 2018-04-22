using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public class GeometricMeanTemplate : GeometricFeatureTemplate
    {
        protected override double[] Extract(double[] intervals, double[] amplitude)
        {
            return new double[] { amplitude.Average() };
        }
    }
}

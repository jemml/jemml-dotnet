using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public class GeometricAreaTemplate : GeometricFeatureTemplate
    {
        protected override double[] Extract(double[] intervals, double[] amplitude)
        {
            double areaSum = Enumerable.Range(0, intervals.Length - 1)
                .Select(i => (intervals[i + 1] - intervals[i]) * (amplitude[i + 1] + amplitude[i])).Sum();

            return new double[] { areaSum * 0.5 };
        }
    }
}

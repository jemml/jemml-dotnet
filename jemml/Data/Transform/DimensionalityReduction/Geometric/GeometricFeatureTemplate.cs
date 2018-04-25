using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public abstract class GeometricFeatureTemplate
    {
        public double[] ExtractFrom(ISample sample, params int[] columns)
        {
            if (columns.Length < 1)
            {
                // apply to all columns if no specific columns specified
                return Enumerable.Range(0, sample.GetColumnCount()).SelectMany(i => Extract(sample.GetIntervals(), sample.GetDataRows(i))).ToArray();
            }
            else
            {
                return columns.SelectMany(i => Extract(sample.GetIntervals(), sample.GetDataRows(i))).ToArray();
            }
        }

        protected abstract double[] Extract(double[] intervals, double[] dimensions);
    }
}

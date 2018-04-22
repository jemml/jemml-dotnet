using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Regression.Linear;

namespace jemml.Data.Transform.Transform.Normalization
{
    /// <summary>
    /// Class representing the regression to be performed
    /// </summary>
    public class Regressor
    {
        Dictionary<int, double[]> amplitudeSlopesByColumn = new Dictionary<int, double[]>();
        public virtual void AddColumnRegression<T>(int column, List<T> calibratedSet) where T : RegressionColumn
        {
            // take the regression with all points per user standardized around a minimum of (0, 0) (all we care about is the slope anyway)
            // group by row index for row-based regression
            double[] amplitudeSlopes = calibratedSet.SelectMany(calibratedColumn => calibratedColumn.GetRows().Select((row, rowIndex) => new { rowIndex, duration = calibratedColumn.Duration, value = row }))
                .GroupBy(r => r.rowIndex, r => new { duration = r.duration, value = r.value })
                .Select(r => GenerateRegression(r.Select(d => d.duration).ToArray(), r.Select(v => v.value).ToArray())).ToArray();

            amplitudeSlopesByColumn.Add(column, amplitudeSlopes);
        }

        public Dictionary<int, double[]> GetAmplitudeSlopes()
        {
            return amplitudeSlopesByColumn; // column => slopes
        }

        protected double GenerateRegression(double[] x, double[] y)
        {
            OrdinaryLeastSquares ols = new OrdinaryLeastSquares();
            SimpleLinearRegression regression = ols.Learn(x, y);
            return regression.Slope;
        }
    }
}

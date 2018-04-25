using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Transform.Transform.Normalization.Regression
{
    public class AlignedRegressor : Regressor
    {
        Dictionary<int, double[]> phaseSlopesByColumn = new Dictionary<int, double[]>();
        public override void AddColumnRegression<T>(int column, List<T> calibratedSet)
        {
            // calculate amplitude slopes
            base.AddColumnRegression(column, calibratedSet);

            // calculate phase slopes (T must be of the AlignedRegressionColumn type)
            List<AlignedRegressionColumn> alignedCalibratedSet = calibratedSet.Cast<AlignedRegressionColumn>().ToList();
            // standardize row indices with positions from 0 - 1 to give a base for calculating the regression
            double[] phaseSlopes = alignedCalibratedSet.SelectMany(calibratedColumn => calibratedColumn.StandardizedIntervals.Select((standardInterval, positionIndex) => new { positionIndex, duration = calibratedColumn.Duration, interval = standardInterval }))
                .GroupBy(r => r.positionIndex, r => new { r.duration, r.interval })
                .Select(r => GenerateRegression(r.Select(d => d.duration).ToArray(), r.Select(i => i.interval).ToArray())).ToArray();
            phaseSlopesByColumn.Add(column, phaseSlopes);
        }

        public Dictionary<int, double[]> GetPhaseSlopes()
        {
            return phaseSlopesByColumn;
        }
    }
}

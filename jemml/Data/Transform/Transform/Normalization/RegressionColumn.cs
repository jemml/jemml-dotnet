using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Transform.Transform.Normalization
{
    /// <summary>
    /// Class representing the aligned rows/indices in a column
    /// </summary>
    public class RegressionColumn
    {
        public string Identifier { get; private set; }
        public double Duration { get; private set; }
        protected double[] rows;
        public RegressionColumn(double[] rows, string identifier, double duration)
        {
            this.rows = rows;
            Identifier = identifier;
            Duration = duration;
        }
        public virtual T ApplyCalibration<T>(T calibrationSample) where T : RegressionColumn
        {
            // calibrate column amplitude
            double[] calibratedRows = GetCalibratedRows(calibrationSample);
            return new RegressionColumn(calibratedRows, Identifier, Duration) as T;
        }
        protected double[] GetCalibratedRows<T>(T calibrationSample) where T : RegressionColumn
        {
            return rows.Select((r, i) => r - calibrationSample.rows[i]).ToArray();
        }
        public virtual double[] GetRows()
        {
            return rows;
        }
    }
}

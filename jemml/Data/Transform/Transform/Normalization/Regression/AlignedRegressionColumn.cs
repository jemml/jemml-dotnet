using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Transform.Transform.Normalization.Regression
{
    public class AlignedRegressionColumn : RegressionColumn
    {
        public List<int> RowIndices { get; private set; }
        public Boolean IsTemplate { get; private set; }
        public List<double> StandardizedIntervals { get; private set; } // row index values standardized into a range from 0 - 1

        private AlignedRegressionColumn(List<int> rowIndices, double[] rows, string identifier, double duration, List<double> standardizedIntervals, bool isTemplate)
            : base(rows, identifier, duration)
        {
            RowIndices = rowIndices;
            StandardizedIntervals = standardizedIntervals == null ? GetStandardIntervals(RowIndices) : standardizedIntervals;
            IsTemplate = isTemplate;
        }

        public AlignedRegressionColumn(List<int> rowIndices, double[] rows, string identifier, double duration, bool isTemplate = false) : this(rowIndices, rows, identifier, duration, null, isTemplate) { }

        private List<double> GetStandardIntervals(List<int> rowIndices)
        {
            return rowIndices.Select(index => ((double)index / rowIndices.Last())).ToList();
        }
        public void RepeatIndices(List<int> pathWithNewRepeatedIndices)
        {
            RowIndices = pathWithNewRepeatedIndices.Select(i => RowIndices[i - 1]).ToList();
            StandardizedIntervals = pathWithNewRepeatedIndices.Select(i => StandardizedIntervals[i - 1]).ToList();
        }
        public override double[] GetRows()
        {
            return RowIndices.Select(i => rows[i - 1]).ToArray(); // get aligned rows
        }
        public override T ApplyCalibration<T>(T calibrationSample)
        {
            AlignedRegressionColumn alignedCalibrationSample = calibrationSample as AlignedRegressionColumn;
            if (alignedCalibrationSample == null)
            {
                throw new ArgumentException("Must provided AlignedRegressionColumn to RegressionColumn");
            }

            // calibrate column amplitude
            double[] calibratedRows = GetCalibratedRows(alignedCalibrationSample);

            // calibrate column phase
            List<double> calibratedStandardIntervals = StandardizedIntervals.Select((interval, index) => interval - alignedCalibrationSample.StandardizedIntervals[index]).ToList();

            return new AlignedRegressionColumn(RowIndices, calibratedRows, Identifier, Duration, calibratedStandardIntervals, IsTemplate) as T;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Normalization.Regression
{
    public class LLSRNormalizer : SampleTransform, ITrainable
    {
        [JsonProperty]
        protected int[] columns;
        [JsonProperty]
        protected double standardTime; // time to standardize to (the mean of the training set)
        [JsonProperty]
        protected int trainingRowCount = 0;
        [JsonProperty]
        protected int trainingColumnCount = 0;

        [JsonProperty]
        public Dictionary<int, double[]> AmplitudeSlopes { get; protected set; } // slopes marking the sample data amplitudes (DataRows) with respect to sample duration

        protected LLSRNormalizer(int[] columns, double standardTime, int trainingRowCount, int trainingColumnCount, Dictionary<int, double[]> amplitudeSlopes, Preprocessor predecessor) : base(predecessor)
        {
            // copy constructor
            this.columns = columns;
            this.standardTime = standardTime;
            this.trainingRowCount = trainingRowCount;
            this.trainingColumnCount = trainingColumnCount;
            this.AmplitudeSlopes = amplitudeSlopes;
        }

        public LLSRNormalizer(params int[] columns) : this(columns, 0, 0, 0, null, null) { }

        public virtual bool IsTrained()
        {
            return AmplitudeSlopes != null;
        }

        public virtual P Train<P>(List<ISample> trainingSamples) where P : Preprocessor
        {
            InitializeTimeAndCountProperties(trainingSamples, columns);
            Dictionary<int, List<RegressionColumn>> trainingRegressionColumns = GenerateRegressionColumns(trainingSamples, trainingColumnCount);
            Regressor regressor = GenerateRegressionSlopes<RegressionColumn, Regressor>(trainingRegressionColumns);
            AmplitudeSlopes = regressor.GetAmplitudeSlopes();
            return this as P;
        }

        protected void InitializeTimeAndCountProperties(List<ISample> trainingSet, int[] columns)
        {
            List<int> trainingRows = trainingSet.Select(sample => sample.GetDataRows().Count).Distinct().ToList();
            if (trainingRows.Count != 1)
            {
                throw new ArgumentException("All training samples must be the same length");
            }
            trainingRowCount = trainingRows[0];
            trainingColumnCount = trainingSet[0].GetColumnCount();
            this.columns = GetAppliedColumns(columns, trainingSet[0].GetColumnCount()); // default to applying to all columns if no particular column provided
            this.standardTime = trainingSet.Select(sample => sample.GetDuration().Value).Average();
        }

        private Dictionary<int, List<RegressionColumn>> GenerateRegressionColumns(List<ISample> trainingSet, int trainingColumnCount)
        {
            // convert sample list to regression list organized by column
            return Enumerable.Range(0, trainingColumnCount)
                .Select(index => new { index, columns = trainingSet.Select(sample => new RegressionColumn(sample.GetDataRows(index), sample.GetIdentifier(), sample.GetDuration().Value)).ToList() })
                .ToDictionary(s => s.index, s => s.columns);
        }

        protected Dictionary<string, T> GetMinimumDurationColumnSample<T>(List<T> trainingColumnSamples) where T : RegressionColumn
        {
            // get the minimum duration regression column (grouped by identifier)
            return trainingColumnSamples.GroupBy(columnSample => columnSample.Identifier, columnSample => columnSample)
                .Select(columnList => columnList.OrderBy(n => n.Duration).First()) // minimum duration column
                .ToDictionary(column => column.Identifier, column => column);
        }

        /**
         * Return calibrated column
         * 
         */
        protected List<T> GenerateCalibratedColumnSamples<T>(List<T> trainingColumnSamples) where T : RegressionColumn
        {
            // get each minimum duration sample grouped by identifier
            Dictionary<string, T> minimumDurationRegressionColumns = GetMinimumDurationColumnSample(trainingColumnSamples);

            // get the samples with each data entry shifted with respect to the sample with the minimum duration for each identifier
            return trainingColumnSamples.Select(columnSample => columnSample.ApplyCalibration(minimumDurationRegressionColumns[columnSample.Identifier])).ToList();
        }

        protected R GenerateRegressionSlopes<T, R>(Dictionary<int, List<T>> trainingRegressionColumns) where T : RegressionColumn where R : Regressor, new()
        {
            R regressor = new R();
            // for each column calibrate the data and regress
            Parallel.For(0, trainingColumnCount, (i, alignedIndices) =>
            {
                // calibration - for each identifier set the point at the minimum time to (0, 0) and standardize other points around it
                List<T> calibratedSet = GenerateCalibratedColumnSamples(trainingRegressionColumns[i]);

                regressor.AddColumnRegression<T>(i, calibratedSet);
            });

            return regressor;
        }

        protected override List<Tuple<double, double[]>> GetTransformedRows(ISample sample, int[] columns)
        {
            if (!sample.GetDuration().HasValue)
            {
                throw new ArgumentException("Sample does not have duration");
            }
            // normalize applicable columns
            // NOTE - the lambda expression references an outer variable - this will be stored in the lambda and not garbaged collected until the lambda is (see Variable Scope in Lambda Expressions)
            return TransformData(sample, (interval, i) => i * (standardTime / sample.GetDataRows().Count), (value, i, j) => value + AmplitudeSlopes[j][i] * (standardTime - sample.GetDuration().Value), columns);
        }

        protected override int[] GetColumns()
        {
            return columns;
        }

        protected override bool RecalculateDuration()
        {
            return true; // regression transforms each sample into similar time domain (the mean)
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new LLSRNormalizer(columns, standardTime, trainingRowCount, trainingColumnCount, AmplitudeSlopes, predecessor);
        }
    }
}

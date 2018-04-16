using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform
{
    public class DerivativeTransform : SampleTransform
    {
        [JsonProperty]
        protected int[] columns;

        protected DerivativeTransform(Preprocessor predecessor, params int[] columns) : base(predecessor)
        {
            this.columns = columns;
        }

        public DerivativeTransform(params int[] columns) : this(null, columns) { }

        protected override List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns)
        {
            List<Tuple<double, double[]>> dataRows = sample.GetDataRows();
            List<Tuple<double, double[]>> derivativeData = new List<Tuple<double, double[]>>();

            for (int i = 0; i < (dataRows.Count - 1); i++)
            {
                double timeInterval = dataRows[i + 1].Item1 - dataRows[i].Item1;
                derivativeData.Add(new Tuple<double, double[]>(dataRows[i].Item1, CalculateColumnDerivatives(columns, dataRows[i].Item2, dataRows[i + 1].Item2, timeInterval)));
            }

            // calculate the last derivative to equalize the list length
            double approximateEndInterval = sample.GetDuration().HasValue ? sample.GetDuration().Value / dataRows.Count : 1;
            derivativeData.Add(new Tuple<double, double[]>(dataRows[dataRows.Count - 1].Item1, CalculateColumnDerivatives(columns, dataRows[dataRows.Count - 1].Item2, Enumerable.Repeat(0.0, sample.GetColumnCount()).ToArray(), approximateEndInterval)));

            return derivativeData;
        }

        private double[] CalculateColumnDerivatives(int[] appliedToColumns, double[] currentColumns, double[] nextColumns, double timeInterval)
        {
            return TransformRow(appliedToColumns, (value, j) => CalculateDerivative(currentColumns[j], nextColumns[j], timeInterval), currentColumns);
        }

        private double CalculateDerivative(double currentValue, double nextValue, double timeInterval)
        {
            return (nextValue - currentValue) / timeInterval; // slope
        }

        protected override bool RecalculateDuration()
        {
            return false;
        }

        protected override int[] GetColumns()
        {
            return columns;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new DerivativeTransform(predecessor, columns);
        }
    }
}

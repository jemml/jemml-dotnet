using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Transform
{
    /// <summary>
    /// Transformation over specific columns without changing number of columns or dimensionality such as domain transforms or derivative calculation 
    /// </summary>
    public abstract class SampleTransform : Preprocessor
    {
        protected SampleTransform(Preprocessor predecessor) : base(predecessor) { }

        public Sample ApplyTransform(Sample sample)
        {
            int[] columns = GetColumns();
            if (columns.Length >= 0 && columns.Where(i => i > sample.GetColumnCount() - 1 || i < 0).Count() > 0)
            {
                throw new ArgumentException("Provided columns must be a number between 0 and the total number of columns minus one");
            }
            return sample.AcceptVisitor(new SampleDataVisitor(GetTransformedRows(sample, GetAppliedColumns(columns, sample.GetColumnCount())), RecalculateDuration()));
        }

        protected abstract List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns);

        protected abstract int[] GetColumns();

        protected abstract bool RecalculateDuration();

        protected override Sample Process(Sample sample)
        {
            return ApplyTransform(sample);
        }

        protected int[] GetAppliedColumns(int[] columns, int columnCount)
        {
            return columns.Length > 0 ? columns : Enumerable.Range(0, columnCount).ToArray(); // default to applying to all columns if no particular column provided
        }

        protected List<Tuple<double, double[]>> TransformData(Sample sample, Func<double, double> dataTransformation, int[] columns)
        {
            return TransformData(sample, (interval, i) => interval, (value, i, j) => dataTransformation.Invoke(value), columns);
        }

        protected double[] TransformRow(int[] columns, Func<double, double> dataTransformation, double[] currentValues)
        {
            return TransformRow(columns, 0, (value, i, j) => dataTransformation.Invoke(value), currentValues);
        }

        protected List<Tuple<double, double[]>> TransformData(Sample sample, Func<double, int, double> dataTransformation, int[] columns)
        {
            return TransformData(sample, (interval, i) => interval, (value, i, j) => dataTransformation.Invoke(value, j), columns);
        }

        protected double[] TransformRow(int[] columns, Func<double, int, double> transformation, double[] currentValues)
        {
            return TransformRow(columns, 0, (value, i, j) => transformation.Invoke(value, j), currentValues);
        }

        protected List<Tuple<double, double[]>> TransformData(Sample sample, Func<double, int, double> intervalTransformation, Func<double, int, int, double> dataTransformation, int[] columns)
        {
            List<Tuple<double, double[]>> dataRows = sample.GetDataRows();
            List<Tuple<double, double[]>> transformedData = new List<Tuple<double, double[]>>();

            for (int i = 0; i < dataRows.Count; i++)
            {
                transformedData.Add(new Tuple<double, double[]>(intervalTransformation.Invoke(dataRows[i].Item1, i), TransformRow(columns, i, dataTransformation, dataRows[i].Item2)));
            }

            return transformedData;
        }

        protected double[] TransformRow(int[] columns, int row, Func<double, int, int, double> dataTransformation, double[] currentValues)
        {
            return currentValues.Select((value, i) => columns.Contains(i) ? dataTransformation.Invoke(value, row, i) : value).ToArray();
        }
    }
}

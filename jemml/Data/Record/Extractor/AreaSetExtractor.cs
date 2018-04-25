using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Data.Record.Extractor
{
    public class AreaSetExtractor : SampleExtractor
    {
        [JsonProperty]
        private int areaRows;

        protected AreaSetExtractor(int areaRows, Preprocessor predecessor)
            : base(predecessor)
        {
            this.areaRows = areaRows;
        }

        public AreaSetExtractor(int areaRows) : this(areaRows, null) { }

        protected override List<Tuple<double, double[]>> ExtractRows(ISample sample)
        {
            double interval = (double)(sample.GetDataRows().Count - 1) / (double)areaRows;
            double startPoint = 0.0;
            List<Tuple<double, double[]>> areaData = new List<Tuple<double, double[]>>();

            // loop areaDimensions
            for (int areaRow = 0; areaRow < areaRows; areaRow++)
            {
                double adjustedTime = GetAreaTime(sample, areaRow, interval, startPoint);
                areaData.Add(new Tuple<double, double[]>(adjustedTime, GetAreaColumns(sample, areaRow, interval, startPoint)));
                startPoint = Math.Round((interval * (double)(areaRow + 1)), 8);
            }
            return areaData;
        }

        private double[] GetAreaColumns(ISample sample, int areaRow, double interval, double startPoint)
        {
            double[] areaColumns = new double[sample.GetColumnCount()];
            for (int column = 0; column < sample.GetColumnCount(); column++)
            {
                areaColumns[column] = GetReducedAreasForColumn(sample, areaRow, column, interval, startPoint);
            }
            return areaColumns;
        }

        private double GetAreaTime(ISample sample, int areaRow, double interval, double startPoint)
        {
            List<Tuple<double, double[]>> dataRows = sample.GetDataRows();

            int currentStartIndexLower = (int)Math.Floor(startPoint);
            int currentStartIndexUpper = (int)Math.Ceiling(startPoint);
            double startTime = (double)(startPoint - currentStartIndexLower) * (dataRows[currentStartIndexUpper].Item1 - dataRows[currentStartIndexLower].Item1) + dataRows[currentStartIndexLower].Item1;

            double endTime;
            if (areaRow != (areaRows - 1))
            {
                int currentEndIndexLower = (int)Math.Floor((Math.Round((interval * (double)(areaRow + 1)), 8)));
                int currentEndIndexUpper = (int)Math.Ceiling(Math.Round(startPoint + interval, 8));
                endTime = (double)(startPoint + interval - currentEndIndexLower) * (dataRows[currentEndIndexUpper].Item1 - dataRows[currentEndIndexLower].Item1) + dataRows[currentEndIndexLower].Item1;
            }
            else
            {
                endTime = dataRows.Last().Item1; // take the time at the last point
            }

            return (endTime + startTime) / 2;
        }

        private double GetReducedAreasForColumn(ISample sample, int areaRow, int column, double interval, double startPoint)
        {
            double area = 0.0;

            int prevEndIndex = (int)Math.Floor(startPoint);
            int currentStartIndex = (int)Math.Ceiling(startPoint);
            double startBoundary = (double)currentStartIndex - startPoint;

            int currentEndIndex = (int)Math.Floor((Math.Round((interval * (double)(areaRow + 1)), 8)));
            int nextStartIndex = (int)Math.Ceiling(Math.Round(startPoint + interval, 8));
            double endBoundary = (startPoint + interval) - (double)currentEndIndex;

            List<Tuple<double, double[]>> dataRows = sample.GetDataRows();

            // calculate all full trapezoids
            for (int j = currentStartIndex; j < currentEndIndex; j++)
            {
                area += 0.5 * (dataRows[(j + 1)].Item1 - dataRows[j].Item1) * (dataRows[(j + 1)].Item2[column] + dataRows[j].Item2[column]);
            }

            // add lower boundary area
            area += startBoundary * 0.5 * (dataRows[currentStartIndex].Item1 - dataRows[prevEndIndex].Item1) * (dataRows[currentStartIndex].Item2[column] + dataRows[prevEndIndex].Item2[column]);

            // no end boundary on the last point
            if (areaRow != (areaRows - 1))
            {
                // add upper boundary area
                area += endBoundary * 0.5 * (dataRows[nextStartIndex].Item1 - dataRows[currentEndIndex].Item1) * (dataRows[nextStartIndex].Item2[column] + dataRows[currentEndIndex].Item2[column]);
            }

            return area;
        }

        protected override bool RecalculateDuration()
        {
            return false;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new AreaSetExtractor(areaRows, predecessor);
        }
    }
}

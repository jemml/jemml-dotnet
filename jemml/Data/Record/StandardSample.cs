using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Data.Record
{
    public class StandardSample : Sample
    {
        [JsonProperty]
        private string identifier; // unique identifier (owner of the sample)

        [JsonProperty]
        private bool isImposter; // whether this user is in-class or an imposter (out-of-class sample)

        [JsonProperty]
        private int order; // sequential number of the sample (i.e. trail #) 

        [JsonProperty]
        private int columnCount; // the number of columns in each data row

        [JsonProperty]
        private List<Tuple<double, double[]>> dataRows; // <interval, data>

        [JsonProperty]
        private double? duration;

        protected StandardSample()
        {
            // for serialization only
        }

        public StandardSample(string identifier, bool isImposter, List<Tuple<double, double[]>> dataRows, int order, bool hasDuration) :
            this(identifier, isImposter, dataRows, order, CalculateDuration(dataRows, hasDuration))
        {

        }

        protected static double? CalculateDuration(List<Tuple<double, double[]>> dataRows, bool hasDuration)
        {
            // validate rows available
            if (dataRows.Count < 1)
            {
                throw new ArgumentException("A sample cannot have empty data rows");
            }
            double? timeDelta = dataRows.Last().Item1 - dataRows.First().Item1;
            return hasDuration ? timeDelta : null;
        }

        public StandardSample(string identifier, bool isImposter, List<Tuple<double, double[]>> dataRows, int order, double? duration)
        {
            this.identifier = identifier;
            this.isImposter = isImposter;
            this.order = order;
            this.dataRows = dataRows;
            // validate rows available
            if (dataRows.Count < 1)
            {
                throw new ArgumentException("A sample cannot have empty data rows");
            }
            // validate columns per row
            List<int> columns = dataRows.Select(row => row.Item2.Length).Distinct().ToList();
            if (columns.Count > 1)
            {
                throw new ArgumentException("All sample rows must have the same number of columns");
            }
            columnCount = columns.DefaultIfEmpty(0).FirstOrDefault();
            this.duration = duration;
        }

        public string GetIdentifier()
        {
            return identifier;
        }

        public bool IsImposter()
        {
            return isImposter;
        }

        public int GetOrder()
        {
            return order;
        }

        public List<Tuple<double, double[]>> GetDataRows()
        {
            return dataRows;
        }

        public int GetColumnCount()
        {
            return columnCount;
        }

        public double[] GetDataRows(int column)
        {
            return dataRows.Select(row => row.Item2[column]).ToArray();
        }

        public double[] GetDimensions()
        {
            return dataRows.Select(row => row.Item2).SelectMany(i => i).ToArray();
        }

        public int GetDimensionCount()
        {
            return columnCount * dataRows.Count;
        }

        public double[] GetIntervals()
        {
            return dataRows.Select(row => row.Item1).ToArray();
        }

        public double? GetDuration()
        {
            return duration;
        }

        public double[] GetAllValuesForColumns(int[] columns)
        {
            if (columns.Length <= 0)
            {
                return new double[0]; // nothing to return if no columns
            }
            return GetDataRows().SelectMany(row => row.Item2.Where((value, column) => columns.Contains(column))).ToArray(); // a flat array of values for all provided columns
        }

        public virtual T AcceptVisitor<T>(SampleVisitor<T> visitor)
        {
            return visitor.Accept(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Subset.Extractor
{
    public class SampleColumnExtractor : ISubsetExtractor
    {
        [JsonProperty]
        private int column;

        public SampleColumnExtractor(int column)
        {
            if (column < 1)
            {
                throw new ArgumentException("Column must be a valid number between 1 and the number of columns");
            }
            this.column = column;
        }

        public List<double> Extract(ISample sample)
        {
            if (column > sample.GetColumnCount())
            {
                throw new ArgumentException("Column must be a valid number between 1 and the number of columns");
            }
            List<double> columnSubset = new List<double>();
            foreach (Tuple<double, double[]> sampleRow in sample.GetDataRows())
            {
                columnSubset.Add(sampleRow.Item2[(column - 1)]);
            }
            return columnSubset;
        }
    }
}

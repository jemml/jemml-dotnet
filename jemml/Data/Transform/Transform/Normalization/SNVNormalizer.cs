using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Normalization
{
    public class SNVNormalizer : SampleTransform
    {
        [JsonProperty]
        private int[] columns;

        protected SNVNormalizer(Preprocessor predecessor, params int[] columns) : base(predecessor)
        {
            this.columns = columns;
        }

        public SNVNormalizer(params int[] columns) : this(null, columns)
        {
        }

        protected double GetMean(Sample sample, int[] columns)
        {
            return sample.GetAllValuesForColumns(columns).Average();
        }

        protected double GetStandardDeviation(Sample sample, int[] columns)
        {
            double avg = sample.GetAllValuesForColumns(columns).Average();
            return Math.Sqrt(sample.GetAllValuesForColumns(columns).Average(v => Math.Pow(v - avg, 2)));
        }

        protected override List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns)
        {
            return TransformData(sample, value => (value - GetMean(sample, columns)) / GetStandardDeviation(sample, columns), columns);
        }

        protected override int[] GetColumns()
        {
            return columns;
        }

        protected override bool RecalculateDuration()
        {
            return false;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new SNVNormalizer(predecessor, columns);
        }
    }
}

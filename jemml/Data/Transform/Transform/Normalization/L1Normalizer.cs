using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Normalization
{
    public class L1Normalizer : SampleTransform
    {
        [JsonProperty]
        private int[] columns;

        protected L1Normalizer(Preprocessor predecessor, params int[] columns)
            : base(predecessor)
        {
            this.columns = columns;
        }

        public L1Normalizer(params int[] columns) : this(null, columns) { }

        protected double GetAbsoluteSum(Sample sample, int[] columns)
        {
            return sample.GetAllValuesForColumns(columns).Sum(value => Math.Abs(value)); // sum the absolute value of each column value
        }

        protected override List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns)
        {
            return TransformData(sample, value => value / GetAbsoluteSum(sample, columns), columns);
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
            return new L1Normalizer(predecessor, columns);
        }
    }
}

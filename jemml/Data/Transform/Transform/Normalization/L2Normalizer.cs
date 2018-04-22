using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Normalization
{
    public class L2Normalizer : SampleTransform
    {
        [JsonProperty]
        private int[] columns;

        protected L2Normalizer(Preprocessor predecessor, params int[] columns)
            : base(predecessor)
        {
            this.columns = columns;
        }

        public L2Normalizer(params int[] columns) : this(null, columns) { }

        protected double GetEuclideanDistance(Sample sample, int[] columns)
        {
            return Math.Sqrt(sample.GetAllValuesForColumns(columns).Sum(value => Math.Pow(value, 2)));
        }

        protected override List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns)
        {
            return TransformData(sample, value => value / GetEuclideanDistance(sample, columns), columns);
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
            return new L2Normalizer(predecessor, columns);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Normalization
{
    public class LinfNormalizer : SampleTransform
    {
        [JsonProperty]
        private int[] columns;

        protected LinfNormalizer(Preprocessor predecessor, params int[] columns) : base(predecessor)
        {
            this.columns = columns;
        }

        public LinfNormalizer(params int[] columns) : this(null, columns) { }

        protected double GetMaximumAmplitude(Sample sample, int[] columns)
        {
            return sample.GetAllValuesForColumns(columns).Select(value => Math.Abs(value)).Max();
        }

        protected override List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns)
        {
            return TransformData(sample, value => value / GetMaximumAmplitude(sample, columns), columns);
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
            return new LinfNormalizer(predecessor, columns);
        }
    }
}

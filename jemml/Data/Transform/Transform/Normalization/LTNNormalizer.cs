using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Normalization
{
    public class LTNNormalizer : SampleTransform
    {
        [JsonProperty]
        private int[] columns;
        [JsonProperty]
        private double standardTime; // time to standardize to (note this will just scale according to the standard time - may only make sense records that are a factor of time such as area)

        protected LTNNormalizer(double standardTime, Preprocessor predecessor, params int[] columns) : base(predecessor)
        {
            this.columns = columns;
            this.standardTime = standardTime;
        }

        // be warned this alters the sample duration and time intervals
        public LTNNormalizer(double standardTime, params int[] columns) : this(standardTime, null, columns)
        {
        }

        protected override List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns)
        {
            if (!sample.GetDuration().HasValue)
            {
                throw new ArgumentException("Sample does not have duration");
            }
            // normalize applicable columns
            double normConst = (standardTime / sample.GetDuration().Value);
            List<Tuple<double, double[]>> normalizedData = TransformData(sample, value => value * normConst, columns);
            // normalize the time interval
            return normalizedData.Select((value, i) => new Tuple<double, double[]>(i * (standardTime / normalizedData.Count), value.Item2)).ToList();
        }

        protected override int[] GetColumns()
        {
            return this.columns;
        }

        protected override bool RecalculateDuration()
        {
            return true;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new LTNNormalizer(standardTime, predecessor, columns);
        }
    }
}

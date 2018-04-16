using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Data.Record.Extractor
{
    public class PointSetExtractor : SampleExtractor
    {
        [JsonProperty]
        private int pointRows;

        protected PointSetExtractor(int pointRows, Preprocessor predecessor)
            : base(predecessor)
        {
            this.pointRows = pointRows;
        }

        // interval will be used to calculate time for padded rows
        public PointSetExtractor(int pointRows) : this(pointRows, null) { }

        protected override List<Tuple<double, double[]>> ExtractRows(Sample sample)
        {
            List<Tuple<double, double[]>> pointData = new List<Tuple<double, double[]>>();

            int sampleRowCount = (sample.GetDataRows().Count < pointRows) ? sample.GetDataRows().Count : pointRows;

            // first get all the values already in the sample
            for (int i = 0; i < sampleRowCount; i++)
            {
                pointData.Add(sample.GetDataRows()[i]);
            }

            double time = sample.GetDataRows().Last().Item1;
            double interval = sample.GetDuration().HasValue ? sample.GetDuration().Value / sampleRowCount : 1;

            // pad the remaining rows with zeros
            for (int i = sampleRowCount; i < pointRows; i++)
            {
                time += interval;
                pointData.Add(new Tuple<double, double[]>(time, Enumerable.Repeat(0.0, sample.GetColumnCount()).ToArray()));
            }

            return pointData;
        }

        protected override bool RecalculateDuration()
        {
            return true;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new PointSetExtractor(pointRows, predecessor);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public class GeometricFeatureExtractor
    {
        [JsonProperty]
        private List<TemplateColumnPair> templates;

        public GeometricFeatureExtractor()
        {
            templates = new List<TemplateColumnPair>();
        }

        public GeometricFeatureExtractor FindFeatures(GeometricFeatureTemplate template, params int[] columns)
        {
            templates.Add(new TemplateColumnPair(template, columns));
            return this;
        }

        public double[] ExtractFrom(Sample sample)
        {
            return templates.AsParallel().AsOrdered().SelectMany(template => template.Template.ExtractFrom(sample, template.Columns)).ToArray();
        }

        private class TemplateColumnPair
        {
            [JsonProperty]
            public GeometricFeatureTemplate Template { get; private set; }
            [JsonProperty]
            public int[] Columns { get; private set; }
            public TemplateColumnPair(GeometricFeatureTemplate template, int[] columns)
            {
                this.Template = template;
                this.Columns = columns;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public class GeometricRecoveryConfig
    {
        [JsonProperty]
        public int FirstTriangleLength { get; private set; }
        [JsonProperty]
        public int FirstPointSensitivity { get; private set; }
        [JsonProperty]
        public int SecondTriangleLength { get; private set; }
        [JsonProperty]
        public int SecondPointSensitivity { get; private set; }

        public GeometricRecoveryConfig(int firstTriangleLength, int firstPointSensitivity, int secondTriangleLength, int secondPointSensitivity)
        {
            this.FirstTriangleLength = firstTriangleLength;
            this.FirstPointSensitivity = firstPointSensitivity;
            this.SecondTriangleLength = secondTriangleLength;
            this.SecondPointSensitivity = secondPointSensitivity;
        }
    }
}

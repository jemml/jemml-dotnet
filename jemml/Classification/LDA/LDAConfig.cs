using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Classification.LDA
{
    public class LDAConfig
    {
        [JsonProperty]
        public LDAType Type { get; private set; }

        [JsonConstructor]
        LDAConfig() { /* for serialization */ }

        public LDAConfig(LDAType type = LDAType.ULDA)
        {
            Type = type;
        }

        public virtual double[,] FormatInput(double[,] input)
        {
            return input; // standard LDA does nothing to its input
        }

        public virtual double[] FormatRow(double[] input)
        {
            return input; // standard LDA does nothing to its input
        }
    }
}

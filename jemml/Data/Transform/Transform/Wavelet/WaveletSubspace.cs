using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class WaveletSubspace
    {
        [JsonProperty]
        public int DecompositionLevel { get; private set; }
        [JsonProperty]
        public int SubspaceIndex { get; private set; }

        public WaveletSubspace(int decompositionLevel, int subspaceIndex)
        {
            this.DecompositionLevel = decompositionLevel;
            this.SubspaceIndex = subspaceIndex;
        }
    }
}

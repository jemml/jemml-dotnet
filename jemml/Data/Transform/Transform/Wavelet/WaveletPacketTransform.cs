using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public abstract class WaveletPacketTransform : SampleTransform
    {
        protected WaveletPacketTransform(Preprocessor preprocessor) : base(preprocessor) { }

        public WaveletPacket GetWaveletPacketDecomposition(Sample sample, int column, Wavelet wavelet, int toLevel)
        {
            WaveletPacket top = new WaveletPacket(sample.GetDataRows(column), sample.GetIdentifier());
            BuildWaveletTree(top, wavelet, toLevel);
            return top;
        }

        private void BuildWaveletTree(WaveletPacket parent, Wavelet wavelet, int toLevel)
        {
            if (parent.Coefficients.Length >= wavelet.GetWaveLength() && parent.DecompositionLevel < toLevel)
            {
                Tuple<WaveletPacket, WaveletPacket> forwardTransform = wavelet.Forward(parent);
                parent.SetChildren(forwardTransform.Item1, forwardTransform.Item2); // set children and recurse down to desired level
                BuildWaveletTree(parent.LeftChild, wavelet, toLevel);
                BuildWaveletTree(parent.RightChild, wavelet, toLevel);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Accord.Audio;
using jemml.Data.Record;

namespace jemml.Data.Transform.Transform.Spectral
{
    public class MagnitudeSpectrumTransform : FourierTransform
    {
        protected MagnitudeSpectrumTransform(Preprocessor predecessor) : base(predecessor) { }

        public MagnitudeSpectrumTransform() : this(null) { }

        protected override List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns)
        {
            return GetFourierTransformedList(GetComplexDataRows(sample));
        }

        protected override double[] GetFourierProperty(Complex[] frequencyDomain)
        {
            return Tools.GetMagnitudeSpectrum(frequencyDomain);
        }

        // transforms data from frequency to time domain (so time is lost and become integer values) - dimensionality can change so must be applied across all columns
        protected override int[] GetColumns()
        {
            return new int[0]; // default for empty is to apply to all columns
        }

        protected override bool RecalculateDuration()
        {
            return false;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new MagnitudeSpectrumTransform(predecessor);
        }
    }
}

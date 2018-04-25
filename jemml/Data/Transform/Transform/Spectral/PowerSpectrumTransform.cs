using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Accord.Audio;
using jemml.Data.Record;

namespace jemml.Data.Transform.Transform.Spectral
{
    public class PowerSpectrumTransform : FourierTransform
    {
        protected PowerSpectrumTransform(Preprocessor predecessor) : base(predecessor) { }

        public PowerSpectrumTransform() : this(null) { }

        protected override List<Tuple<double, double[]>> GetTransformedRows(ISample sample, int[] columns)
        {
            return GetFourierTransformedList(GetComplexDataRows(sample));
        }

        protected override double[] GetFourierProperty(Complex[] frequencyDomain)
        {
            return Tools.GetPowerSpectrum(frequencyDomain);
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
            return new PowerSpectrumTransform(predecessor);
        }
    }
}

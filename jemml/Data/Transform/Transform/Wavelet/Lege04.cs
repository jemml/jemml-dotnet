using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class Lege04 : Wavelet
    {
        public override int GetWaveLength()
        {
            return 4; // minimal array size for transform
        }

        public override double[] GetCoeffs()
        {
            double[] coeffs = new double[GetWaveLength()]; // can be done in static way also; faster?

            coeffs[0] = GetScales()[3]; //    h3
            coeffs[1] = -GetScales()[2]; //  -h2
            coeffs[2] = GetScales()[1]; //    h1
            coeffs[3] = -GetScales()[0]; //  -h0

            return coeffs;
        }

        public override double[] GetScales()
        {
            double[] scales = new double[GetWaveLength()]; // can be done in static way also; faster?

            scales[0] = (-5.0 / 8.0) / 1.4142135623730951;
            scales[1] = (-3.0 / 8.0) / 1.4142135623730951;
            scales[2] = (-3.0 / 8.0) / 1.4142135623730951;
            scales[3] = (-5.0 / 8.0) / 1.4142135623730951;

            return scales;
        }
    }
}

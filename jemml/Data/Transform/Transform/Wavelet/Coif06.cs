using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class Coif06 : Wavelet
    {
        public override int GetWaveLength()
        {
            return 6; // minimal array size for transform
        }

        public override double[] GetCoeffs()
        {
            double[] coeffs = new double[GetWaveLength()]; // can be done in static way also; faster?

            coeffs[0] = GetScales()[5]; //    h5
            coeffs[1] = -GetScales()[4]; //  -h4
            coeffs[2] = GetScales()[3]; //    h3
            coeffs[3] = -GetScales()[2]; //  -h2
            coeffs[4] = GetScales()[1]; //    h1
            coeffs[5] = -GetScales()[0]; //  -h0

            return coeffs;
        }

        public override double[] GetScales()
        {
            double[] scales = new double[GetWaveLength()]; // can be done in static way also; faster?

            double sqrt15 = Math.Sqrt(15.0);
            scales[0] = 1.4142135623730951 * (sqrt15 - 3.0) / 32.0;
            scales[1] = 1.4142135623730951 * (1.0 - sqrt15) / 32.0;
            scales[2] = 1.4142135623730951 * (6.0 - 2 * sqrt15) / 32.0;
            scales[3] = 1.4142135623730951 * (2.0 * sqrt15 + 6.0) / 32.0;
            scales[4] = 1.4142135623730951 * (sqrt15 + 13.0) / 32.0;
            scales[5] = 1.4142135623730951 * (9.0 - sqrt15) / 32.0;

            return scales;
        }
    }
}

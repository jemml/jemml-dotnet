using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class Lege06 : Wavelet
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

            scales[0] = -63.0 / 128.0 / 1.4142135623730951; // h0
            scales[1] = -35.0 / 128.0 / 1.4142135623730951; // h1
            scales[2] = -30.0 / 128.0 / 1.4142135623730951; // h2
            scales[3] = -30.0 / 128.0 / 1.4142135623730951; // h3
            scales[4] = -35.0 / 128.0 / 1.4142135623730951; // h4
            scales[5] = -63.0 / 128.0 / 1.4142135623730951; // h5

            return scales;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class Daub02 : Wavelet
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

            double sqrt3 = Math.Sqrt(3.0); // 1.7320508075688772
            scales[0] = ((1.0 + sqrt3) / 4.0) / 1.4142135623730951;
            scales[1] = ((3.0 + sqrt3) / 4.0) / 1.4142135623730951;
            scales[2] = ((3.0 - sqrt3) / 4.0) / 1.4142135623730951;
            scales[3] = ((1.0 - sqrt3) / 4.0) / 1.4142135623730951;

            return scales;
        }
    }
}

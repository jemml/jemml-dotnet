using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class Daub04 : Wavelet
    {
        public override int GetWaveLength()
        {
            return 8; // minimal array size for transform
        }

        public override double[] GetCoeffs()
        {
            double[] coeffs = new double[GetWaveLength()]; // can be done in static way also; faster?

            coeffs[0] = GetScales()[7]; //  h7
            coeffs[1] = -GetScales()[6]; // -h6
            coeffs[2] = GetScales()[5]; //  h5
            coeffs[3] = -GetScales()[4]; // -h4
            coeffs[4] = GetScales()[3]; //  h3
            coeffs[5] = -GetScales()[2]; // -h2
            coeffs[6] = GetScales()[1]; //  h1
            coeffs[7] = -GetScales()[0]; // -h0

            return coeffs;
        }

        public override double[] GetScales()
        {
            double[] scales = new double[GetWaveLength()]; // can be done in static way also; faster?

            double sqrt02 = 1.4142135623730951;
            // TODO Get analytical formulation, due to its precision; this is around 1.e-3 only
            scales[0] = 0.32580343; //  0.32580343
            scales[1] = 1.01094572; //  1.01094572
            scales[2] = 0.8922014; //  0.8922014
            scales[3] = -0.03967503; // -0.03967503
            scales[4] = -0.2645071; // -0.2645071
            scales[5] = 0.0436163; //  0.0436163
            scales[6] = 0.0465036; //  0.0465036
            scales[7] = -0.01498699; // -0.01498699

            // normalize to square root of 2 for being orthonormal 
            for (int i = 0; i < GetWaveLength(); i++)
            {
                scales[i] /= sqrt02;
            }

            return scales;
        }
    }
}

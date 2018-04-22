using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public abstract class Wavelet
    {
        /**
         * Performs the forward transform for the given array from time domain to
         * Hilbert domain and returns a new array of the same size keeping
         * coefficients of Hilbert domain and should be of length 2 to the power of p
         * -- length = 2^p where p is a positive integer.
         * 
         * Returns Tuple<LeftWaveletPacket, RightWaveletPacket>
         */
        public Tuple<WaveletPacket, WaveletPacket> Forward(WaveletPacket parent)
        {
            if (parent.Coefficients.Length % 2 != 0)
            {
                throw new ArgumentException("Tree must contain an even number of features");
            }

            double[] arrHilbLeft = new double[parent.Coefficients.Length / 2];
            double[] arrHilbRight = new double[parent.Coefficients.Length / 2];

            int k = 0;
            int h = parent.Coefficients.Length >> 1;

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < GetWaveLength(); j++)
                {
                    k = (i << 1) + j;
                    while (k >= parent.Coefficients.Length)
                    {
                        k -= parent.Coefficients.Length;
                    }

                    arrHilbLeft[i] += parent.Coefficients[k] * GetScales()[j]; // low pass filter - energy (approximation)
                    arrHilbRight[i] += parent.Coefficients[k] * GetCoeffs()[j]; // high pass filter - details 
                } // wavelet
            } // h
            return new Tuple<WaveletPacket, WaveletPacket>(new WaveletPacket(arrHilbLeft, parent.Identifier, parent, parent.SubspaceIndex * 2), new WaveletPacket(arrHilbRight, parent.Identifier, parent, parent.SubspaceIndex * 2 + 1));
        }

        /**
         * Returns the number of coeffs (and scales).
         * 
         * @date 08.02.2010 13:11:47
         * @author Christian Scheiblich
         * @return integer representing the number of coeffs.
         */
        public int GetLength()
        {
            return GetCoeffs().Length;
        } // getLength

        /**
         * minimal wavelength of the used wavelet and scaling coefficients
         */
        public abstract int GetWaveLength();

        /**
         * coefficients of the wavelet; wavelet function
         */
        public abstract double[] GetCoeffs();

        /**
         * coefficients of the scales; scaling function
         */
        public abstract double[] GetScales();
    }
}

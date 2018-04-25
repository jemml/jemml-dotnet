using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Accord.Math.Transforms;
using jemml.Data.Record;

namespace jemml.Data.Transform.Transform.Spectral
{
    public abstract class FourierTransform : SampleTransform
    {
        protected FourierTransform(Preprocessor predecessor) : base(predecessor) { }

        protected Complex[][] GetComplexDataRows(ISample sample)
        {
            Complex[][] dataRows = new Complex[sample.GetColumnCount()][];
            for (int column = 0; column < sample.GetColumnCount(); column++)
            {
                Complex[] fftColumn = new Complex[sample.GetDataRows().Count];
                for (int row = 0; row < sample.GetDataRows().Count; row++)
                {
                    fftColumn[row] = sample.GetDataRows()[row].Item2[column];
                }
                dataRows[column] = fftColumn;
            }
            return dataRows;
        }

        protected List<Tuple<double, double[]>> GetFourierTransformedList(Complex[][] dataRows)
        {
            double[][] frequencyEstimates = new double[dataRows.GetLength(0)][];
            for (int column = 0; column < dataRows.GetLength(0); column++)
            {
                FourierTransform2.FFT(dataRows[column], Accord.Math.FourierTransform.Direction.Forward); // fft transform on rows for provided column
                frequencyEstimates[column] = GetFourierProperty(dataRows[column]);
            }
            List<Tuple<double, double[]>> frequencySpectraData = new List<Tuple<double, double[]>>();
            for (int frequencyBin = 0; frequencyBin < frequencyEstimates[0].Length; frequencyBin++)
            {
                // NOTE - frequency transforms are applied to all columns
                double[] frequencyColumn = TransformRow(GetAppliedColumns(GetColumns(), dataRows.GetLength(0)), (value, column) => frequencyEstimates[column][frequencyBin], new double[dataRows.GetLength(0)]);
                frequencySpectraData.Add(new Tuple<double, double[]>(frequencyBin, frequencyColumn));
            }
            return frequencySpectraData;
        }

        protected abstract double[] GetFourierProperty(Complex[] frequencyDomain);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class OptimalWaveletPacketTransform : WaveletPacketTransform, Trainable
    {
        [JsonProperty]
        private int[] columns;
        [JsonProperty]
        private int toLevel;
        [JsonProperty]
        private Wavelet wavelet;
        [JsonProperty]
        private double r;
        [JsonProperty]
        private List<WaveletSubspace>[] optimalSubspace = null;
        [JsonProperty]
        private int trainingColumnCount = 0;
        [JsonProperty]
        private int trainingRowCount = 0;

        protected OptimalWaveletPacketTransform(Wavelet wavelet, int toLevel, double r, int[] columns, List<WaveletSubspace>[] optimalSubspace, int trainingColumnCount, int trainingRowCount, Preprocessor predecessor) : base(predecessor)
        {
            // Copy-constructor
            this.columns = columns;
            this.toLevel = toLevel;
            this.wavelet = wavelet;
            this.r = r;
            this.optimalSubspace = optimalSubspace;
            this.trainingColumnCount = trainingColumnCount;
            this.trainingRowCount = trainingRowCount;
        }

        public OptimalWaveletPacketTransform(Wavelet wavelet, int toLevel, double r, params int[] columns) : this(wavelet, toLevel, r, columns, null, 0, 0, null) { }

        public bool IsTrained()
        {
            return optimalSubspace != null;
        }

        public P Train<P>(List<Sample> trainingSamples) where P : Preprocessor
        {
            List<int> trainingRows = trainingSamples.Select(sample => sample.GetDataRows().Count).Distinct().ToList();
            if (trainingRows.Count != 1)
            {
                throw new ArgumentException("All training samples must be the same length");
            }
            trainingRowCount = trainingRows[0];
            trainingColumnCount = trainingSamples[0].GetColumnCount();
            optimalSubspace = GenerateOptimalWaveletPacketDecomposition(trainingSamples, GetAppliedColumns(columns, trainingSamples[0].GetColumnCount()));
            return this as P;
        }

        protected List<WaveletSubspace>[] GenerateOptimalWaveletPacketDecomposition(List<Sample> trainingSet, int[] columns)
        {
            List<WaveletSubspace>[] columnOptimalSubspaces = new List<WaveletSubspace>[columns.Length];
            for (int j = 0; j < trainingColumnCount; j++)
            {
                // only need to calculate optimal decomposition for applicable columns
                if (columns.Contains(j))
                {
                    columnOptimalSubspaces[j] = GetColumnOptimalWaveletPacketDecomposition(trainingSet, j);
                }
            }
            return columnOptimalSubspaces;
        }

        protected List<WaveletSubspace> GetColumnOptimalWaveletPacketDecomposition(List<Sample> trainingSet, int column)
        {
            // perform the wavelet packet decomposition of the data rows for the given column
            List<WaveletPacket> packetList = trainingSet.Select(sample => GetWaveletPacketDecomposition(sample, column, wavelet, toLevel)).ToList();

            // calculate the optimal wavelet packet decomposition using a fuzzy membership function criterion
            FuzzySet fcmTop = new FuzzySet(packetList, r);
            return fcmTop.GetOptimalSubspaceList();
        }

        protected override List<Tuple<double, double[]>> GetTransformedRows(Sample sample, int[] columns)
        {
            if (sample.GetDataRows().Count != trainingRowCount)
            {
                throw new ArgumentException("Sample must match the length of the training samples");
            }

            // transform each applicable column with optimal wavelet packet decomposition
            double[][] transformedColumns = new double[trainingColumnCount][];
            for (int j = 0; j < trainingColumnCount; j++)
            {
                // only need to calculate optimal decomposition for applicable columns
                if (columns.Contains(j))
                {
                    WaveletPacket top = GetWaveletPacketDecomposition(sample, j, wavelet, toLevel);
                    transformedColumns[j] = optimalSubspace[j].Select(subspace => top.FindSubspace(subspace)).SelectMany(packet => packet.Coefficients).ToArray();
                }
                else
                {
                    transformedColumns[j] = sample.GetDataRows(j);
                }
            }
            List<Tuple<double, double[]>> waveletData = new List<Tuple<double, double[]>>();
            for (int row = 0; row < trainingRowCount; row++)
            {
                waveletData.Add(new Tuple<double, double[]>(sample.GetDataRows()[row].Item1, transformedColumns.Select(coefficientList => coefficientList[row]).ToArray()));
            }
            return waveletData;
        }

        protected override int[] GetColumns()
        {
            return columns;
        }

        protected override bool RecalculateDuration()
        {
            return false;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new OptimalWaveletPacketTransform(wavelet, toLevel, r, columns, optimalSubspace, trainingColumnCount, trainingRowCount, predecessor);
        }
    }
}

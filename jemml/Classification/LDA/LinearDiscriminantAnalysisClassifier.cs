using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using jemml.Data.Record;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Classification.LDA
{
    public class LinearDiscriminantAnalysisClassifier : IClassifier
    {
        [JsonProperty]
        protected Dictionary<string, int> identifiersMap;
        [JsonProperty]
        protected double[][] projectedClassMeans;
        [JsonProperty]
        protected double[,] G;
        [JsonProperty]
        protected int reducedDimensions;
        [JsonProperty]
        protected LDAConfig config;

        [JsonConstructor]
        LinearDiscriminantAnalysisClassifier() { /* for serialization */ }

        public LinearDiscriminantAnalysisClassifier(LDAConfig config, List<ISample> trainingSamples, string[] trainingIdentifiers)
        {
            Train(config, trainingSamples, trainingIdentifiers);
        }

        protected void Train(LDAConfig config, List<ISample> trainingSamples, string[] trainingIdentifiers)
        {
            // store confige
            this.config = config;

            // generate a numeric mapping of our string identifiers to unique numeric values
            identifiersMap = trainingIdentifiers.Select((identifier, index) => new { identifier, index }).ToDictionary(id => id.identifier, id => id.index);

            // generate a numeric mapping of our string identifiers to unique numeric values
            double[][] inputSamples = trainingSamples.Select(sample => sample.GetDimensions()).ToArray();
            double[,] input = config.FormatInput(DataHelpers.Convert(inputSamples));
            int[] ideal = trainingSamples.Select(sample => identifiersMap[sample.GetIdentifier()]).ToArray();

            // for this implementation we only reduce down to the number of classes
            reducedDimensions = Math.Min(identifiersMap.Count - 1, input.GetLength(1));

            Tuple<double[,], double[][]> transform = Compute(input, ideal, config.Type);
            double[][] classMeans = transform.Item2;
            G = transform.Item1;

            // get projected class means
            projectedClassMeans = Enumerable.Range(0, identifiersMap.Count).Select(i => Transform(classMeans[i], G)).ToArray();
        }

        public Tuple<double[,], double[][]> Compute(double[,] input, int[] rowClasses, LDAType type)
        {
            double[] totalMeans = Measures.Mean(input, dimension: 0);
            double[][] classMeans = Enumerable.Range(0, identifiersMap.Count).Select(i =>
            {
                double[,] subset = MatrixHelpers.Subset(input, rowClasses, i);
                return Measures.Mean(subset, dimension: 0); // get the class mean
            }).ToArray();

            // divide Sb/Sw by 1/n, Hb/Ht by 1/sqrt(n)
            // Sb = Hb*Hb', St = Ht*Ht'
            double[,] Sb = MatrixHelpers.GetBetweenClassScatter(input, rowClasses, totalMeans);
            double[,] Sw = MatrixHelpers.GetWithinClassScatter(input, rowClasses);
            double[,] Hb = MatrixHelpers.GetHalfBetweenClassScatter(input, rowClasses, totalMeans);
            double[,] Ht = MatrixHelpers.GetHalfTotalClassScatter(input, rowClasses, totalMeans);

            double[,] G = CalculateTransformationMatrix(Sb, Sw, Hb, Ht, input, type, rowClasses);

            return new Tuple<double[,], double[][]>(G, classMeans);
        }

        // calculate the transformation matrix
        protected double[,] CalculateTransformationMatrix(double[,] Sb, double[,] Sw, double[,] Hb, double[,] Ht, double[,] source, LDAType type, int[] rowClasses)
        {
            // define matrices (transpose so that they work like http://www-users.cs.umn.edu/~jieping/uLDA/ULDA.m)
            double[,] HbM = Hb.Transpose();
            double[,] HtM = Ht.Transpose();

            // calculate the transformation matrix G
            int size_low = HbM.Rank(); // to-do: maybe this should be min of dimensions/classes-1
            int s = HtM.Rank();

            SingularValueDecomposition svdHt = new SingularValueDecomposition(HtM, true, false); // partial svd

            double[,] D1Full = svdHt.DiagonalMatrix; // S;
            double[,] U1Full = svdHt.LeftSingularVectors; // U

            double[,] D1 = MatrixHelpers.CopyDiagonal(D1Full, s, d => 1.0 / d);
            double[,] U1 = MatrixHelpers.Copy(U1Full, U1Full.GetLength(0), s);

            double[,] B = D1.Dot(U1.Transpose()).Dot(HbM);

            SingularValueDecomposition svdB = new SingularValueDecomposition(B); // full svd

            double[,] P = svdB.LeftSingularVectors;
            double[,] X = U1.Dot(D1).Dot(P);

            if (reducedDimensions > size_low)
            {
                throw new ArgumentException("Reduced dimensions should be less than size_low");
            }

            double[,] G = MatrixHelpers.Copy(X, X.GetLength(0), reducedDimensions);

            if (type == LDAType.OLDA)
            {
                G = new QrDecomposition(G).OrthogonalFactor;
            }

            // get projected covariance matrix (ext transpose to get Sw back to consistent form)
            double[,] transformedSource = Transform(source, G);
            double[,] S = MatrixHelpers.GetWithinClassScatter(transformedSource, rowClasses);

            // transform G down to the rank of S if rank(S) < reducedDimensions (i.e this means all our covariance sits in a lower dimensional space)
            if (S.Rank() < reducedDimensions)
            {
                // recalculate G
                G = MatrixHelpers.Copy(X, X.GetLength(0), S.Rank);

                // reset reduced dimensions
                reducedDimensions = S.Rank();
                // recalculate S in lower dimensional space
                S = MatrixHelpers.GetWithinClassScatter(transformedSource, rowClasses);
            }
            return G;
        }

        protected double[,] Transform(double[,] source, double[,] G)
        {
            // get transformed source
            return G.Transpose().Dot(source.Transpose()).Transpose();
        }

        // return the projection of a vector in the discriminant space
        private double[] Transform(double[] sample, double[,] G)
        {
            return G.Transpose().Dot(sample.Transpose()).Transpose().GetRow(0);
        }

        public double Verify(double[] features, string identifier)
        {
            int p = this.reducedDimensions;

            // calculate P(k)
            double pk = 1.0 / (double)identifiersMap.Count;

            // calculate PDF(x|k) for each class
            double[] pdfs = Enumerable.Range(0, identifiersMap.Count)
                .Select(i => GetSquaredEuclideanDistance(config.FormatRow(features), G, i)) // get the Squared Euclidean Distance / 2
                .Select(d => Math.Log(pk) + Math.Log(1 / (Math.Pow(Math.Exp(2.0 * Math.PI), p / 2.0))) - d) // p = # of discriminants
                .ToArray();

            // find to maximum log probability
            double maximumProbability = pdfs.Max();
            pdfs = pdfs.Select(pdf => Math.Exp(pdf - maximumProbability)).ToArray();

            // find PDF(k|x) for provided user using Bayes rule
            double pdfSum = pdfs.Sum();

            return pdfs[identifiersMap[identifier]] / pdfSum;
        }

        // get euclidean distance from given sample to the mean of the given classNumber
        protected double GetSquaredEuclideanDistance(double[] sample, double[,] G, int classNumber)
        {
            double[,] diff = Transform(sample, G).Subtract(projectedClassMeans[classNumber]).Transpose();
            return diff.Transpose().Dot(diff)[0, 0];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Accord.Math;
using Accord.Statistics;

namespace jemml.Utilities
{
    public class MatrixHelpers
    {
        public static double[,] GetHalfTotalClassScatter(double[,] input, int[] rowClasses, double[] totalMeans)
        {
            // calculate the partial total scatter H_t
            double[,] Ht = new double[input.GetLength(0), input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                double[] Hti = input.GetRow(i).Subtract(totalMeans);
                for (int j = 0; j < Hti.GetLength(0); j++)
                {
                    Ht[i, j] = Hti[j];
                }
            }
            return Ht.Multiply(1.0 / Math.Sqrt(input.GetLength(0)));
        }

        public static double[,] GetBetweenClassScatter(double[,] input, int[] rowClasses, double[] totalMeans)
        {
            double[,] Sb = new double[input.GetLength(1), input.GetLength(1)];
            for (int i = 0; i < rowClasses.DistinctCount(); i++)
            {
                // Get the class subset
                double[,] subset = Subset(input, rowClasses, i);
                int count = subset.GetLength(0);

                // Get the class mean
                double[] classMean = Measures.Mean(subset, dimension: 0);
                double[] meanDif = classMean.Subtract(totalMeans);
                double[,] Sbi = meanDif.Outer(meanDif).Multiply(count);

                // summation
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    for (int k = 0; k < input.GetLength(1); k++)
                    {
                        Sb[j, k] += Sbi[j, k];
                    }
                }
            }
            return Sb.Multiply(1.0 / input.GetLength(0));
        }

        public static double[,] GetHalfBetweenClassScatter(double[,] input, int[] rowClasses, double[] totalMeans)
        {
            double[,] Hb = new double[input.GetLength(0), input.GetLength(1)];
            for (int i = 0; i < rowClasses.DistinctCount(); i++)
            {
                // Get the class subset
                double[,] subset = Subset(input, rowClasses, i);
                int count = subset.GetLength(0);

                // Get the class mean
                double[] classMean = Measures.Mean(subset, dimension: 0);
                double[] meanDif = classMean.Subtract(totalMeans);

                for (int j = 0; j < input.GetLength(1); j++)
                {
                    Hb[i, j] = meanDif[j] * Math.Sqrt(count);
                }
            }
            return Hb.Multiply(1.0 / Math.Sqrt(input.GetLength(0)));
        }

        public static double[,] GetWithinClassScatter(double[,] input, int[] rowClasses)
        {
            double[,] Sw = new double[input.GetLength(1), input.GetLength(1)];
            for (int i = 0; i < rowClasses.DistinctCount(); i++)
            {
                // Get the class subset
                double[,] subset = Subset(input, rowClasses, i);
                int count = subset.GetLength(0);

                // Get the class mean
                double[] classMean = Measures.Mean(subset, dimension: 0);

                // calculate within class scatter (Sw)
                for (int x = 0; x < count; x++)
                {
                    double[] xMeanDif = subset.GetRow(x).Subtract(classMean);
                    double[,] Swix = xMeanDif.Outer(xMeanDif);

                    for (int j = 0; j < input.GetLength(1); j++)
                    {
                        for (int k = 0; k < input.GetLength(1); k++)
                        {
                            Sw[j, k] += Swix[j, k];
                        }
                    }
                }
            }
            return Sw.Multiply(1.0 / input.GetLength(0));
        }

        // Gets the subset of the original data spawned by this class.
        public static double[,] Subset(double[,] input, int[] rowClasses, int classNumber)
        {
            return input.Get(Matrix.Find(rowClasses, y => y == classNumber));
        }

        public static double[,] Copy(double[,] matrix, int rowsToCopy, int colsToCopy)
        {
            double[,] matrixToCopy = new double[rowsToCopy, colsToCopy];
            for (int i = 0; i < rowsToCopy; i++)
            {
                for (int j = 0; j < colsToCopy; j++)
                {
                    matrixToCopy[i, j] += matrix[i, j];
                }
            }
            return matrixToCopy;
        }

        public static double[,] CopyDiagonal(double[,] matrix, int rowsToCopy, Func<double, double> diagonalTransform)
        {
            double[,] diagonalMatrix = new double[rowsToCopy, rowsToCopy];
            for (int i = 0; i < rowsToCopy; i++)
            {
                diagonalMatrix[i, i] = diagonalTransform.Invoke(matrix[i, i]);
            }
            return diagonalMatrix;
        }

        public static double[,] Identity(int rows, int columns)
        {
            double[,] X = new double[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    X[i, j] = (i == j ? 1.0 : 0.0);
                }
            }
            return X;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Utilities
{
    public class DynamicTimeWarping
    {
        public static List<Tuple<int, int>> FindDTWPath(double[] x, double[] y, double bandwidth)
        {
            List<Tuple<int, int>> pathIndices = new List<Tuple<int, int>>();

            double[,] costArray = GenerateCostArray(x, y, bandwidth);

            int i = x.Length;
            int j = y.Length;

            while ((i != 0) && (j != 0))
            {
                pathIndices.Add(new Tuple<int, int>(i, j));

                if ((costArray[i - 1, j] <= costArray[i, j - 1]) && (costArray[i - 1, j] <= costArray[i - 1, j - 1]))
                {
                    i = i - 1;
                }
                else if ((costArray[i, j - 1] <= costArray[i - 1, j]) && (costArray[i, j - 1] <= costArray[i - 1, j - 1]))
                {
                    j = j - 1;
                }
                else
                {
                    i = i - 1;
                    j = j - 1;
                }
            }

            if ((i != 0) || (j != 0))
            {
                throw new InvalidOperationException("DTW Path Must End At (0, 0)");
            }

            // return the path in the correct order
            pathIndices.Reverse();
            return pathIndices;
        }

        public static double FindDTWCost(double[] x, double[] y, double bandwidth)
        {
            double[,] costArray = GenerateCostArray(x, y, bandwidth);
            return costArray[costArray.GetLength(0) - 1, costArray.GetLength(1) - 1];
        }

        public static double[,] GenerateCostArray(double[] x, double[] y, double bandWidth)
        {
            int n = x.Length;
            int m = y.Length;

            double[,] DTW = new double[n + 1, m + 1];

            // take bandwidth as a % of search space
            int w = Convert.ToInt32(Math.Max(n, m) * bandWidth);
            w = Math.Max(w, Math.Abs(n - m));


            for (int i = 0; i <= n; i++)
            {
                for (int j = 0; j <= m; j++)
                {
                    DTW[i, j] = double.PositiveInfinity;
                }
            }

            DTW[0, 0] = 0;

            for (int i = 1; i <= n; i++)
            {
                for (int j = Math.Max(1, i - w); j <= Math.Min(m, i + w); j++)
                {
                    // DWT[i, j] = d(s[i], t[j]) + min(DWT[i-1, j], DWT[i, j-1], DWT[i-1, j-1]
                    double cost = Math.Abs(x[i - 1] - y[j - 1]);

                    if ((DTW[i - 1, j] <= DTW[i, j - 1]) && (DTW[i - 1, j] <= DTW[i - 1, j - 1]))
                    {
                        DTW[i, j] = cost + DTW[i - 1, j];
                    }
                    else if ((DTW[i, j - 1] <= DTW[i - 1, j]) && (DTW[i, j - 1] <= DTW[i - 1, j - 1]))
                    {
                        DTW[i, j] = cost + DTW[i, j - 1];
                    }
                    else
                    {
                        DTW[i, j] = cost + DTW[i - 1, j - 1];
                    }
                }
            }

            return DTW;
        }

        /**
         * Take a set of values with a corresponding index path mapping then flatten the values so repeated indices are merged
         * 
         */
        public static double[] GenerateFlatPath(int[] path, double[] values)
        {
            if (values.Length > path.Length)
            {
                throw new ArgumentException("Cannot flatten path if path is greater than values");
            }
            // group values by index then convert to their average
            return path.Select((index, i) => new { index, value = values[i] }).GroupBy(map => map.index, map => map.value)
                .Select(map => map.Average()).ToArray();
        }
    }
}

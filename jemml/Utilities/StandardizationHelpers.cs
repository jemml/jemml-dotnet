using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Utilities
{
    public class StandardizationHelpers
    {
        /**
 * Apply L-infinity normalization to the provided values
 */
        public static double[] GenerateLinfNormalizedValues(double[] values)
        {
            double maxValue = values.Select(value => Math.Abs(value)).Max();
            return values.Select(value => value / maxValue).ToArray();
        }

        public static double[] GenerateL1NormalizedValues(double[] values)
        {
            double unitArea = values.Sum(value => Math.Abs(value));
            return values.Select(value => value / unitArea).ToArray();
        }

        /**
         * Resample the provided values to a new set of the provided length
         */
        public static double[] Resample(double[] values, int newLength)
        {
            double[] resampled = new double[newLength];
            double interval = (double)(values.Length - 1) / newLength;
            double startPoint = 0.0;

            for (int i = 0; i < newLength; i++)
            {
                // find the mid point between the start value and end value (that will be the resampled value)
                int prevEndIndex = Convert.ToInt32(Math.Floor(startPoint));
                int currentStartIndex = Convert.ToInt32(Math.Ceiling(startPoint));
                double startProportion = startPoint - prevEndIndex;

                int currentEndIndex = Convert.ToInt32(Math.Floor(Math.Round((interval * (i + 1)), 8)));
                int nextStartIndex = Convert.ToInt32(Math.Ceiling(Math.Round((interval * (i + 1)), 8)));
                double endProportion = (interval * (i + 1)) - currentEndIndex;

                double startValue = values[prevEndIndex] + (values[currentStartIndex] - values[prevEndIndex]) * startProportion;
                double endValue = values[currentEndIndex] + (values[nextStartIndex] - values[currentEndIndex]) * endProportion;

                // if the difference from the calculated start proportion point to end proportion point aren't near the interval value the standardization may not be good
                double propDiff = ((endProportion + currentEndIndex) - (startProportion + prevEndIndex)) - interval;
                if (Math.Abs(propDiff) > 0.0001)
                {
                    throw new InvalidOperationException("Proportions don't match");
                }

                double resampledValue = 0.5 * (startValue + endValue);
                startPoint = Math.Round((interval * (i + 1)), 8);
                resampled[i] = resampledValue;
            }


            // TODO - make this and other warnings configurable so they can be disabled for better performance
            // test area before vs area after standardization (if it's substantially different the standardization may not be good)
            double newArea = Enumerable.Range(1, newLength - 1).Sum(i => (0.5 * (resampled[i] + resampled[i - 1])) * interval);
            double oldArea = Enumerable.Range(1, values.Length - 1).Sum(i => (0.5 * (values[i] + values[i - 1])));

            if (Math.Abs(newArea - oldArea) > 1.0)
            {
                Console.WriteLine("The area standardization was weak: areaDif[" + (newArea - oldArea) + "] areas(" + newArea + ", " + oldArea + ")");
                //throw new InvalidOperationException("The area standardization was weak: areaDif[" + (newArea - oldArea) + "] areas(" + newArea + ", " + oldArea + ")");
            }
            return resampled;
        }

        public static double[] GenerateTransformedData(double[] data, Func<double, int, double> transform)
        {
            return data.Select((value, i) => transform.Invoke(value, i)).ToArray();
        }
    }
}

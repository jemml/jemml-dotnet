using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Data.Transform.DimensionalityReduction.Geometric
{
    public class GeometricExtractorBuilder
    {
        protected List<ExtremaSearchConfig> searchConfig;
        protected ExtremaPointType start;

        public GeometricExtractorBuilder(ExtremaPointType start)
        {
            searchConfig = new List<ExtremaSearchConfig>();
            this.start = start;
        }

        public GeometricExtractorBuilder FindExtremaPoint(int sensitivity, ExtremaDataModel dataModel = ExtremaDataModel.TIME_AMPLITUDE, int? searchIndexLimit = null, GeometricRecoveryConfig geometricRecoveryConfig = null, double? startAmplitude = null)
        {
            searchConfig.Add(new ExtremaSearchConfig(sensitivity, dataModel, searchIndexLimit, geometricRecoveryConfig, startAmplitude));
            return this;
        }

        public ExtremaPointTemplate Build()
        {
            return new ExtremaPointTemplateImpl(start, searchConfig);
        }

        public abstract class ExtremaPointTemplate : GeometricFeatureTemplate { }

        protected class ExtremaPointTemplateImpl : ExtremaPointTemplate
        {
            [JsonProperty]
            protected List<ExtremaSearchConfig> searchConfig;
            [JsonProperty]
            protected ExtremaPointType start;

            public ExtremaPointTemplateImpl(ExtremaPointType start, List<ExtremaSearchConfig> searchConfig)
            {
                this.searchConfig = searchConfig;
                this.start = start;
            }

            protected override double[] Extract(double[] intervals, double[] amplitude)
            {
                ExtremaPointType nextLocalPoint = start;
                int fromIndex = 0;
                List<double[]> pointFeatures = new List<double[]>();

                using (IEnumerator<ExtremaSearchConfig> iterator = searchConfig.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        ExtremaSearchConfig config = iterator.Current;
                        PointIndexPair extremaPoint = Find(nextLocalPoint, fromIndex, config, intervals, amplitude);

                        if (extremaPoint.IndexOutOfRange && config.GeometricRecoveryConfig != null)
                        {
                            GeometricRecoveryConfig recoveryConfig = config.GeometricRecoveryConfig;

                            // use triangular approximation to attempt missing point recovery (need to do 2 points - this + next to approximate missing point)
                            extremaPoint = FindTriangular(nextLocalPoint, fromIndex, recoveryConfig.FirstPointSensitivity, recoveryConfig.FirstTriangleLength, intervals, amplitude);
                            // add first recovered point
                            pointFeatures.Add(GetFeatures(extremaPoint, intervals, config.DataModel));

                            // skip forward one if we can since we've already found the next point
                            if (!iterator.MoveNext())
                            {
                                break;
                            }
                            config = iterator.Current;

                            // recover the second point (which would also be "missing")
                            nextLocalPoint = Alternate(nextLocalPoint);
                            extremaPoint = FindTriangular(nextLocalPoint, extremaPoint.PointIndex, recoveryConfig.SecondPointSensitivity, recoveryConfig.SecondTriangleLength, intervals, amplitude);
                        }

                        pointFeatures.Add(GetFeatures(extremaPoint, intervals, config.DataModel));
                        fromIndex = extremaPoint.PointIndex;
                        nextLocalPoint = Alternate(nextLocalPoint);
                    }
                }
                return pointFeatures.SelectMany(p => p).ToArray();
            }

            protected double[] GetFeatures(PointIndexPair extremaPoint, double[] intervals, ExtremaDataModel model)
            {
                double extremaTime = intervals[extremaPoint.PointIndex] - intervals[0];

                // add the time / amplitude of the extrema point
                double[] extremaFeatures;
                switch (model)
                {
                    case ExtremaDataModel.AMPLITUDE:
                        extremaFeatures = new double[] { extremaPoint.PointAmplitude };
                        break;
                    case ExtremaDataModel.TIME:
                        extremaFeatures = new double[] { extremaTime };
                        break;
                    default:
                        extremaFeatures = new double[] { extremaPoint.PointAmplitude, extremaTime };
                        break;
                }
                return extremaFeatures;
            }

            protected ExtremaPointType Alternate(ExtremaPointType type)
            {
                return type == ExtremaPointType.LOCAL_MAX ? ExtremaPointType.LOCAL_MIN : ExtremaPointType.LOCAL_MAX;
            }

            protected PointIndexPair Find(ExtremaPointType type, int fromIndex, ExtremaSearchConfig config, double[] intervals, double[] amplitude)
            {
                if (type == ExtremaPointType.LOCAL_MAX)
                {
                    return Find(fromIndex, config, intervals, amplitude, (smoothedValue, extremaAmplitide) => smoothedValue > extremaAmplitide);
                }
                else
                {
                    return Find(fromIndex, config, intervals, amplitude, (smoothedValue, extremaAmplitide) => smoothedValue < extremaAmplitide);
                }
            }

            protected PointIndexPair Find(int fromIndex, ExtremaSearchConfig config, double[] intervals, double[] amplitude, Func<double, double, bool> isMatchingCriteria)
            {
                int maxSmoothSize = 5;
                double extremaAmplitude = config.StartAmplitude.HasValue ? config.StartAmplitude.Value : amplitude[fromIndex]; // begin with initial search amplitude
                int extremaInterval = fromIndex;
                int extremaCount = 0;

                if ((fromIndex + maxSmoothSize) >= intervals.Length)
                {
                    maxSmoothSize = 1;
                }

                // look at the average of a group of points rather than a single point to determine min/max
                LinkedList<double> smoothedData = new LinkedList<double>(Enumerable.Range(0, maxSmoothSize).Select(i => amplitude[fromIndex + i]));

                // find the first local min/max
                bool indexOutOfRange = false;
                for (int i = (fromIndex + maxSmoothSize - 1), j = 0; i < amplitude.Length; i++, j++)
                {
                    if (config.SearchIndexLimit.HasValue && j > config.SearchIndexLimit.Value)
                    {
                        Console.WriteLine("MISSING POINT!!!");
                        indexOutOfRange = true;
                        break;
                    }

                    double smoothedAverage = smoothedData.Average();

                    if (isMatchingCriteria.Invoke(smoothedAverage, extremaAmplitude))
                    {
                        extremaAmplitude = smoothedAverage;
                        extremaInterval = (i - (int)Math.Floor(maxSmoothSize / 2.0));
                        extremaCount = 0;
                    }
                    else
                    {
                        // if sensitivity readings are lower then the previous min/max then the first local min/max was reached
                        extremaCount++;
                        if (extremaCount > config.Sensitivity)
                        {
                            break;
                        }
                    }

                    // remove the first force value for the list and add the next
                    smoothedData.RemoveFirst();
                    if ((i + 1) < amplitude.Length)
                    {
                        smoothedData.AddLast(amplitude[(i + 1)]);
                    }
                }
                return new PointIndexPair(extremaAmplitude, extremaInterval, indexOutOfRange);
            }

            protected PointIndexPair FindTriangular(ExtremaPointType type, int fromIndex, int sensitivity, int triDist, double[] intervals, double[] amplitude)
            {
                if (type == ExtremaPointType.LOCAL_MAX)
                {
                    return FindTriangular(fromIndex, sensitivity, triDist, intervals, amplitude, (yline, y) => yline > y);
                }
                else
                {
                    return FindTriangular(fromIndex, sensitivity, triDist, intervals, amplitude, (yline, y) => yline < y);
                }
            }

            protected PointIndexPair FindTriangular(int fromIndex, int sensitivity, int triDist, double[] intervals, double[] amplitude, Func<double, double, bool> isMatchingCriteria)
            {
                double extremaAmplitude = amplitude[fromIndex]; // begin with initial search amplitude
                int extremaInterval = fromIndex;
                double maxArea = 0, minCount = 0, maxX = 0, maxY = 0;

                for (int i = fromIndex; i < (amplitude.Length - triDist); i++)
                {
                    // pick a region to calculate the triangular area for
                    double x1 = (double)i;
                    double x2 = (double)(i + triDist - 1);
                    double y1 = amplitude[i];
                    double y2 = amplitude[i + triDist - 1];

                    // pick a point in the middle to form the tip of the triangle
                    double x = (double)(i + triDist / 2);
                    double y = amplitude[i + triDist / 2];

                    // calculate the shortest distance from (x,y) to the line formed by (x1,y1) (x2,y2)
                    double A = x - x1;
                    double B = y - y1;
                    double C = x2 - x1;
                    double D = y2 - y1;

                    // cross product magnitude of line region
                    double a = Math.Abs(A * D - C * B) / Math.Sqrt(C * C + D * D);

                    // calculate magnitude from both corners to the middle point
                    double E = x2 - x;
                    double F = y2 - y;

                    double h1 = Math.Sqrt(C * C + D * D);
                    double h2 = Math.Sqrt(E * E + F * F);

                    // calculate the area of two triangles
                    double area = (0.5 * a * h1) * (0.5 * a * h2);

                    // determine whether the area is above or below the line based on the position of the middle point (we're only looking for area above the line)
                    double m = D / C;
                    double b = (y2 - m * x2);
                    double yline = (m * x + b);

                    if (!isMatchingCriteria.Invoke(yline, y))
                    {
                        continue; // don't process if the area is not positive or negative depending on the point being examined
                    }

                    if (area > maxArea)
                    {
                        maxArea = area;
                        minCount = 0;
                        maxX = (int)x;
                        maxY = y;
                    }
                    else
                    {
                        // if sensitivity successive readings are higher than the previous min then the first local min was reached
                        minCount++;
                        if (minCount > sensitivity)
                        {
                            break;
                        }
                    }
                }
                return new PointIndexPair(extremaAmplitude, extremaInterval, false);
            }
        }

        protected class ExtremaSearchConfig
        {
            [JsonProperty]
            public int Sensitivity { get; private set; }
            [JsonProperty]
            public ExtremaDataModel DataModel { get; private set; }
            [JsonProperty]
            public int? SearchIndexLimit { get; private set; }
            [JsonProperty]
            public GeometricRecoveryConfig GeometricRecoveryConfig { get; private set; }
            [JsonProperty]
            public double? StartAmplitude { get; private set; }

            public ExtremaSearchConfig(int sensitivity, ExtremaDataModel dataModel, int? searchIndexLimit = null, GeometricRecoveryConfig geometricRecoveryConfig = null, double? startAmplitude = null)
            {
                this.Sensitivity = sensitivity;
                this.DataModel = dataModel;
                this.GeometricRecoveryConfig = geometricRecoveryConfig;
                this.SearchIndexLimit = searchIndexLimit;
                this.StartAmplitude = startAmplitude;
            }
        }

        protected class PointIndexPair
        {
            public double PointAmplitude { get; private set; }
            public int PointIndex { get; private set; }
            public bool IndexOutOfRange { get; private set; }

            public PointIndexPair(double amplitude, int index, bool indexOutOfRange)
            {
                this.PointAmplitude = amplitude;
                this.PointIndex = index;
                this.IndexOutOfRange = indexOutOfRange;
            }
        }
    }
}

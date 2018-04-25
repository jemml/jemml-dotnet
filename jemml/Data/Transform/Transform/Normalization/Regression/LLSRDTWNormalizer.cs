using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jemml.Data.Record;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Data.Transform.Transform.Normalization.Regression
{
    public class LLSRDTWNormalizer : LLSRNormalizer
    {
        [JsonProperty]
        public Dictionary<int, double[]> PhaseSlopes { get; protected set; } // slopes marking the offset in phase as a result of DTW with respect to sample duration
        [JsonProperty]
        public Dictionary<int, double[]> Templates { get; protected set; } // best fit center star templates per column
        [JsonProperty]
        protected double bandwidth;

        protected LLSRDTWNormalizer(double bandwidth, int[] columns, double standardTime, int trainingRowCount, int trainingColumnCount, Dictionary<int, double[]> amplitudeSlopes, Dictionary<int, double[]> phaseSlopes, Dictionary<int, double[]> templates, Preprocessor predecessor) :
            base(columns, standardTime, trainingRowCount, trainingColumnCount, amplitudeSlopes, predecessor)
        {
            this.bandwidth = bandwidth;
            this.columns = columns;
            this.AmplitudeSlopes = amplitudeSlopes;
            this.PhaseSlopes = phaseSlopes;
            this.Templates = templates;
        }

        public LLSRDTWNormalizer(double bandwidth, params int[] columns) : base(columns)
        {
            this.bandwidth = bandwidth;
            PhaseSlopes = null;
            Templates = null;
        }

        public override bool IsTrained()
        {
            return AmplitudeSlopes != null && PhaseSlopes != null && Templates != null;
        }

        public override P Train<P>(List<ISample> trainingSamples)
        {
            InitializeTimeAndCountProperties(trainingSamples, columns);
            // get the aligned training samples
            Dictionary<int, List<AlignedRegressionColumn>> csAlignedSamples = FindApproximateDTWAlignment(trainingSamples, base.trainingColumnCount, base.trainingRowCount, bandwidth); // resample back to original number of training rows

            // the dtw path in the alignment for the sample with the lowest total cost (marked as IsTemplate)
            Templates = csAlignedSamples.Select(column => new { column = column.Key, template = column.Value.First(row => row.IsTemplate) }).ToDictionary(col => col.column, col => StandardizationHelpers.Resample(col.template.GetRows(), trainingRowCount));

            // calculate the regression slopes for the set of aligned signals (key = time slopes, value = force slopes)
            AlignedRegressor regressor = GenerateRegressionSlopes<AlignedRegressionColumn, AlignedRegressor>(csAlignedSamples);
            AmplitudeSlopes = regressor.GetAmplitudeSlopes().ToDictionary(slopes => slopes.Key, slopes => StandardizationHelpers.Resample(slopes.Value, trainingRowCount)); // resample back to original number of training rows
            PhaseSlopes = regressor.GetPhaseSlopes().ToDictionary(slopes => slopes.Key, slopes => StandardizationHelpers.Resample(slopes.Value, trainingRowCount));

            return this as P;
        }

        public static Dictionary<int, List<double[]>> FindApproximateDTWAlignment(List<ISample> samples, double bandwidth)
        {
            List<int> rows = samples.Select(sample => sample.GetDataRows().Count).Distinct().ToList();
            if (rows.Count != 1)
            {
                throw new ArgumentException("All training samples must be the same length");
            }
            int rowCount = rows[0];
            int columnCount = samples[0].GetColumnCount();
            return FindApproximateDTWAlignment(samples, columnCount, rowCount, bandwidth).ToDictionary(alignedCol => alignedCol.Key, alignedCol => alignedCol.Value.Select(col => col.GetRows()).ToList());
        }

        protected static Dictionary<int, List<AlignedRegressionColumn>> FindApproximateDTWAlignment(List<ISample> trainingSet, int trainingColumnCount, int trainingRowCount, double bandwidth)
        {
            // find the best cost indices for each column <column, min cost sample index>
            Dictionary<int, int> bestCostIndex = GetMinDTWCostIndices(trainingSet, bandwidth);

            // initialize the training set alignment <column, List<List<rowIndex>>>
            Dictionary<int, List<AlignedRegressionColumn>> columnAlignedIndices = new Dictionary<int, List<AlignedRegressionColumn>>();

            // run center star in parallel for each column - calculate and apply
            Parallel.For(0, trainingColumnCount, (i, alignedIndices) =>
            {
                columnAlignedIndices[i] = CalculateCenterStarColumnAlignment(i, bestCostIndex[i], trainingSet, trainingRowCount, bandwidth);
            });

            // return aligned samples
            return columnAlignedIndices;
        }

        protected static List<AlignedRegressionColumn> CalculateCenterStarColumnAlignment(int column, int bestCostIndex, List<ISample> trainingSet, int trainingRowCount, double bandwidth)
        {
            // create new alignment list (adding the sample with the best cost index in the first position)
            List<AlignedRegressionColumn> alignedColumn = new List<AlignedRegressionColumn>
            {
                new AlignedRegressionColumn(Enumerable.Range(1, trainingRowCount).ToList(), trainingSet[bestCostIndex].GetDataRows(column), trainingSet[bestCostIndex].GetIdentifier(), trainingSet[bestCostIndex].GetDuration().Value, true)
            };

            // calculate the center star approximation for the given column in the provided training set with the previously discovered bestCostIndex
            // TODO - cleanup -> tolist foreach ...
            trainingSet.Select((s, i) => new { sample = s, index = i }).Where(si => si.index != bestCostIndex).Select(s => s.sample).ToList().ForEach(sampleSi =>
            {
                // 1 - get column row values for s[0]' (our template)
                double[] s0 = alignedColumn[0].GetRows();

                // 2 - get the column row values for s[i]
                double[] si = sampleSi.GetDataRows(column);

                // 3 - get the DTW path between both
                List<Tuple<int, int>> bestPath = FindDTWPath(s0, si, bandwidth);

                // 4 - save the DTW path for s[i] as s[i]'
                alignedColumn.Add(new AlignedRegressionColumn(bestPath.Select(s => s.Item2).ToList(), si, sampleSi.GetIdentifier(), sampleSi.GetDuration().Value));

                // 5 - update the DTW path for s[1]' to s[i-1]' to include any additional indices (extend the template path be repeating indices where they were repeated in this iteration)

                // ignoring the first index (the template) align previously calculated paths by repeating indices where necessary
                List<int> pathWithNewRepeatedIndices = bestPath.Select(s => s.Item1).ToList();
                for (int j = 1; j < alignedColumn.Count - 1; j++)
                {
                    alignedColumn[j].RepeatIndices(pathWithNewRepeatedIndices);
                }

                // 6 - reset s[0]' to s[0]''
                alignedColumn[0].RepeatIndices(pathWithNewRepeatedIndices);
            });

            // place the template feature alignment back into its appropriate position
            alignedColumn.Insert(bestCostIndex + 1, alignedColumn[0]);
            alignedColumn.RemoveAt(0);

            return alignedColumn;
        }

        protected static Dictionary<int, int> GetMinDTWCostIndices(List<ISample> trainingSet, double bandwidth)
        {
            // record training set with indices for easier tracking
            var indexedTrainingSet = trainingSet.Select((sample, index) => new { sample, index });

            // <sample1, sample2, column>, cost
            var pairCosts = indexedTrainingSet.AsParallel().SelectMany(s1 => indexedTrainingSet.Where(r => r.index > s1.index)
                .SelectMany(s2 => Enumerable.Range(0, s2.sample.GetColumnCount()).Select((value, column) => new { sampleIndex1 = s1.index, sampleIndex2 = s2.index, column, dtwCost = FindDTWCost(s1.sample.GetDataRows(column), s2.sample.GetDataRows(column), bandwidth) })))
                .ToDictionary(s => new { s.sampleIndex1, s.sampleIndex2, s.column }, s => s.dtwCost);

            // for each column/sample index find the total cost against all other samples
            var indexTotalCosts = indexedTrainingSet.SelectMany(s => Enumerable.Range(0, s.sample.GetColumnCount()).Select(column => new { column, s.index }))
                .Select(t => new
                {
                    key = t,
                    totalCost = pairCosts
                        .Where(s => s.Key.column == t.column && (s.Key.sampleIndex1 == t.index || s.Key.sampleIndex2 == t.index))
                        .Select(n => n.Value).Sum()
                })
                .ToDictionary(costs => costs.key, costs => costs.totalCost);

            // get the minimum cost index per column
            return indexTotalCosts.GroupBy(c => c.Key.column, c => new Tuple<int, double>(c.Key.index, c.Value))
                .ToDictionary(d => d.Key, d => FindMinimumIndex(d.ToArray()));
        }

        protected static int FindMinimumIndex(Tuple<int, double>[] values)
        {
            return values.OrderBy(x => x.Item2).First().Item1;
        }

        protected static List<Tuple<int, int>> FindDTWPath(double[] template, double[] sample, double bandwidth)
        {
            // normalize each signal to the same height to ensure transformation only affects phase of the signals
            return DynamicTimeWarping.FindDTWPath(StandardizationHelpers.GenerateLinfNormalizedValues(template), StandardizationHelpers.GenerateLinfNormalizedValues(sample), bandwidth);
        }

        protected static double FindDTWCost(double[] d1, double[] d2, double bandwidth)
        {
            // normalize each signal to the same height to ensure transformation only affects phase of the signals
            return DynamicTimeWarping.FindDTWCost(StandardizationHelpers.GenerateLinfNormalizedValues(d1), StandardizationHelpers.GenerateLinfNormalizedValues(d2), bandwidth);
        }

        protected override List<Tuple<double, double[]>> GetTransformedRows(ISample sample, int[] columns)
        {
            if (!sample.GetDuration().HasValue)
            {
                throw new ArgumentException("Sample does not have duration");
            }
            // find the dtw path between the sample and template for each column, use it to get the slopes mapping, then apply the amplitude/phase transformation for each feature
            var pathsToTemplates = Templates.Where(template => columns.Contains(template.Key)).Select(template => new { column = template.Key, pathToTemplate = FindDTWPath(template.Value, sample.GetDataRows(template.Key), bandwidth).Select(s => s.Item2).ToArray() });

            Dictionary<int, double[]> dtwAmplitudeSlopes = pathsToTemplates.Select(path => new { path.column, slopes = DynamicTimeWarping.GenerateFlatPath(path.pathToTemplate, StandardizationHelpers.Resample(AmplitudeSlopes[path.column], path.pathToTemplate.Length)) })
                .ToDictionary(path => path.column, path => path.slopes);

            Dictionary<int, double[]> dtwPhaseSlopes = pathsToTemplates.Select(path => new { path.column, slopes = DynamicTimeWarping.GenerateFlatPath(path.pathToTemplate, StandardizationHelpers.Resample(AmplitudeSlopes[path.column], path.pathToTemplate.Length)) })
                .ToDictionary(path => path.column, path => path.slopes);

            // create amplitude warping
            var amplitudeWarp = dtwAmplitudeSlopes.Select(amplitude => new { column = amplitude.Key, amplitudeWarp = sample.GetDataRows(amplitude.Key).Select((value, j) => value + dtwAmplitudeSlopes[amplitude.Key][j] * (standardTime - sample.GetDuration().Value)) });

            // create phase warping
            Dictionary<int, double[]> phaseWarp = dtwPhaseSlopes.Select(phase => new { column = phase.Key, phaseWarp = StandardizationHelpers.GenerateL1NormalizedValues(sample.GetDataRows().Select((data, i) => phase.Value[i] * (standardTime - sample.GetDuration().Value)).ToArray()) })
                .ToDictionary(warp => warp.column, warp => warp.phaseWarp);

            // get phase warping average
            Dictionary<int, double> phaseWarpAverage = phaseWarp.ToDictionary(warp => warp.Key, warp => warp.Value.Average());

            // normalize the phase warp to have a mean value of 1 then multiply it by the respective amplitude slopes to scale the phase accordingly
            double amplitudeSum = amplitudeWarp.SelectMany(warp => warp.amplitudeWarp).Sum(); // TODO - make this configurable so can be disabled for performance

            Dictionary<int, double[]> phaseAmplitudeWarp = amplitudeWarp.Select(warp => new { warp.column, phaseWarped = warp.amplitudeWarp.Select((value, j) => value * (phaseWarp[warp.column][j] - phaseWarpAverage[warp.column] + 1)).ToArray() })
                .ToDictionary(warp => warp.column, warp => warp.phaseWarped);

            double phaseSum = phaseAmplitudeWarp.SelectMany(warp => warp.Value).Sum(); // TODO - make this configurable so can be disabled for performance

            // warn if difference is beyond tolerance (TODO - make configurable)
            if (Math.Abs(amplitudeSum - phaseSum) > (Math.Abs(amplitudeSum) * 0.05))
            {
                Console.WriteLine("Potentially weak phase normalization. Amplitude Norm Sum: " + amplitudeSum + ", Phase Norm Sum: " + phaseSum);
            }

            // NOTE - the lambda expression references an outer variable - this will be stored in the lambda and not garbaged collected until the lambda is (see Variable Scope in Lambda Expressions)
            return TransformData(sample, (interval, i) => i * (standardTime / sample.GetDataRows().Count), (value, i, j) => phaseAmplitudeWarp[j][i], columns);
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new LLSRDTWNormalizer(bandwidth, columns, standardTime, trainingColumnCount, trainingColumnCount, AmplitudeSlopes, PhaseSlopes, Templates, predecessor);
        }
    }
}

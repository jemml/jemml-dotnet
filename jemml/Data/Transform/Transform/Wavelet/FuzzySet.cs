using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class FuzzySet
    {
        public FuzzySet Parent { get; private set; }
        public FuzzySet LeftChild { get; private set; }
        public FuzzySet RightChild { get; private set; }
        public FuzzySet Sibling { get; private set; }
        public int SubspaceIndex { get; private set; }
        public int DecompositionLevel { get; private set; }
        public double FX { get; private set; }
        public double R { get; private set; }

        public FuzzySet(List<WaveletPacket> packetList, double r, FuzzySet parent = null)
        {
            List<int> dataLength = packetList.Select(packet => packet.Coefficients.Length).Distinct().ToList();
            if (dataLength.Count != 1)
            {
                throw new ArgumentException("Training packet lengths must match to perform fuzzy c-means");
            }

            Parent = parent;
            Sibling = null;
            R = r;
            DecompositionLevel = packetList[0].DecompositionLevel;
            SubspaceIndex = packetList[0].SubspaceIndex;

            // if there are any children then build the child nodes
            if (packetList[0].LeftChild != null)
            {
                List<WaveletPacket> leftChildren = packetList.Select(packet => packet.LeftChild).ToList();
                List<WaveletPacket> rightChildren = packetList.Select(packet => packet.RightChild).ToList();
                LeftChild = new FuzzySet(leftChildren, r, this);
                RightChild = new FuzzySet(rightChildren, r, this);
                LeftChild.Sibling = RightChild;
                RightChild.Sibling = LeftChild;
            }

            // calculate standard deviations for each feature in this subspace
            List<double> standardDeviations = GetStandardDeviations(packetList);

            // calculate the mean for each user (class)
            Dictionary<String, List<double>> means = GetMeans(packetList);

            // determine features to exclude (store index list of excluded features)
            List<int> exclusionCriterion = ExcludionCriterion(packetList, means, standardDeviations);

            // using list of excluded feature calculate the fuzzy c-means clustering membership allocation
            FX = CalculateFuzzyMembershipCriterion(packetList, means, standardDeviations, exclusionCriterion);
        }

        public List<FuzzySet> GetOptimalFuzzySetList()
        {
            List<FuzzySet> fsList = GetFuzzySetList();

            // sort fsList in descending order according to fuzzy membership criterion
            fsList.Sort(CompareFuzzySets);

            List<FuzzySet> optimalDecomposition = new List<FuzzySet>();
            while (fsList.Count > 0)
            {
                // move the first subspace into the optimal decomposition
                FuzzySet next = fsList.First();
                optimalDecomposition.Add(next);
                fsList.RemoveAt(0);

                // remove all remaining ancestors or decendents of this subspace
                List<FuzzySet> ancestors = next.GetAncestors(next.Parent);
                List<FuzzySet> descendants = next.GetDescendants(next);
                fsList.RemoveAll(fs => ancestors.Contains(fs));
                fsList.RemoveAll(fs => descendants.Contains(fs));
            }
            return optimalDecomposition;
        }

        public List<WaveletSubspace> GetOptimalSubspaceList()
        {
            return GetOptimalFuzzySetList().Select(fuzzySet => new WaveletSubspace(fuzzySet.DecompositionLevel, fuzzySet.SubspaceIndex)).ToList();
        }

        private List<FuzzySet> GetAncestors(FuzzySet parent)
        {
            List<FuzzySet> ancestorList = new List<FuzzySet>();
            if (parent != null)
            {
                ancestorList.Add(parent);
                ancestorList.AddRange(GetAncestors(parent.Parent));
            }
            return ancestorList;
        }

        private List<FuzzySet> GetDescendants(FuzzySet parent)
        {
            List<FuzzySet> descendants = new List<FuzzySet>();

            if (parent.LeftChild != null)
            {
                descendants.Add(parent.LeftChild);
                descendants.AddRange(GetDescendants(parent.LeftChild));
            }

            if (parent.RightChild != null)
            {
                descendants.Add(parent.RightChild);
                descendants.AddRange(GetDescendants(parent.RightChild));
            }
            return descendants;
        }

        private static int CompareFuzzySets(FuzzySet fSet1, FuzzySet fSet2)
        {
            if (fSet1.FX < fSet2.FX)
            {
                return 1;
            }
            else if (fSet1.FX == fSet2.FX)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        private List<FuzzySet> GetFuzzySetList(FuzzySet parent = null)
        {
            if (parent == null)
            {
                parent = this;
            }

            List<FuzzySet> fsList = new List<FuzzySet>();
            fsList.Add(parent);

            if (parent.LeftChild != null)
            {
                fsList.AddRange(GetFuzzySetList(parent.LeftChild));
            }

            if (parent.RightChild != null)
            {
                fsList.AddRange(GetFuzzySetList(parent.RightChild));
            }
            return fsList;
        }

        private List<double> GetStandardDeviations(List<WaveletPacket> packetList)
        {
            List<double> standardDeviations = new List<double>();
            for (int i = 0; i < packetList[0].Coefficients.Length; i++)
            {
                List<double> packetData = packetList.Select(packet => packet.Coefficients[i]).ToList();
                double avg = packetData.Average();
                double deviation = Math.Sqrt(packetData.Average(v => Math.Pow(v - avg, 2)));
                standardDeviations.Add(deviation == 0 ? Double.Epsilon : deviation);
            }
            return standardDeviations;
        }

        private Dictionary<string, List<double>> GetMeans(List<WaveletPacket> packetList)
        {
            List<Dictionary<string, double>> meanSums = new List<Dictionary<string, double>>();
            for (int i = 0; i < packetList[0].Coefficients.Length; i++)
            {
                meanSums.Add(packetList.GroupBy(packet => packet.Identifier, packet => packet.Coefficients[i]).Select(g => new Tuple<string, double>(g.Key, g.Sum())).ToDictionary(gdc => gdc.Item1, gdc => gdc.Item2));
            }

            // get the number of samples for each user
            Dictionary<string, int> sampleN = packetList.GroupBy(packet => packet.Identifier, packet => packet).Select(g => new Tuple<string, int>(g.Key, g.Count())).ToDictionary(gdc => gdc.Item1, gdc => gdc.Item2);

            Dictionary<string, List<double>> means = new Dictionary<string, List<double>>();
            for (int i = 0; i < packetList[0].Coefficients.Length; i++)
            {
                foreach (string identifier in meanSums[i].Keys)
                {
                    if (!means.ContainsKey(identifier))
                    {
                        means[identifier] = new List<double>();
                    }
                    means[identifier].Add(meanSums[i][identifier] / sampleN[identifier]);
                }
            }
            return means;
        }

        private List<int> ExcludionCriterion(List<WaveletPacket> packetList, Dictionary<string, List<double>> means, List<double> standardDeviations)
        {
            List<int> excludedRows = new List<int>();
            // for each feature find the largest/smallest class mean
            for (int i = 0; i < packetList[0].Coefficients.Length; i++)
            {
                double meanMin = means.Select(mean => mean.Value[i]).Min();
                double meanMax = means.Select(mean => mean.Value[i]).Max();

                // D(j)
                double Dj = (meanMax - meanMin) / (2 * standardDeviations[i]);

                if (Dj < this.R)
                {
                    excludedRows.Add(i);
                }
            }
            return excludedRows;
        }

        private double CalculateFuzzyMembershipCriterion(List<WaveletPacket> packetList, Dictionary<string, List<double>> means, List<double> standardDeviations, List<int> exclusionCriterion)
        {
            // calculate F(X) (Uik) i=1->c,keAi (Ai is the set of indexes of training patterns belonging to i
            double fx = 0.0;
            int b = 2;

            // loop through each class (user)
            List<String> identifiers = new List<string>(means.Keys);

            foreach (String identifier in identifiers)
            {
                // loop through all the trials for this user
                foreach (WaveletPacket packet in packetList)
                {
                    if (!packet.Identifier.Equals(identifier))
                    {
                        continue;
                    }

                    // calculate Xk' - Vi'
                    double xk_vi = GetNormalizedEuclidean(packet, identifier, means, standardDeviations, exclusionCriterion);

                    // if xk = vi then uik = 1
                    if (xk_vi == 0)
                    {
                        fx += 1; // fx += uik
                        continue;
                    }

                    // calculate Xk' - Vj'
                    // for each user (j=1 -> c) follow steps 1-3 and calculate Uik
                    double uik = identifiers.Select(identifierJ => new Tuple<string, double>(identifierJ, GetNormalizedEuclidean(packet, identifierJ, means, standardDeviations, exclusionCriterion)))
                        .Where(xk_vj => !((xk_vj.Item2 == 0) && (identifier.Equals(xk_vj.Item1)))) // if xk = vj i != j then uik = 0 (so don't include in sum)
                        .Select(xk_vj => Math.Pow((xk_vi / xk_vj.Item2), 1 / (b - 1)))
                        .DefaultIfEmpty(0.0)
                        .Sum();

                    if (uik != 0)
                    {
                        uik += Math.Pow(uik, -1);
                    }

                    fx += uik;
                }
            }
            return fx;
        }

        private double GetNormalizedEuclidean(WaveletPacket packet, String identifier, Dictionary<string, List<double>> means, List<double> standardDeviations, List<int> exclusionCriterion)
        {
            return packet.Coefficients.Select((row, i) => row - means[identifier][i]) // 1 subtract vector V (all feature means in this feature set for this class) from Xk (all features in this trial [packet])
                .Select((mean, i) => mean / standardDeviations[i]) // 2 divide the result vector by the respective standard deviation vector
                .Where((mean, i) => !exclusionCriterion.Contains(i)) // 3 remove excluded features (ignore excluded features)
                .Aggregate(0.0, (accumulated, next) => accumulated += Math.Pow(next, 2)); // 4 find the square of the euclidean magnitude for the resulting vector srt(x^2+y^2)^2 = x^2+y^2
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Accord.Statistics.Analysis;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data.Transform.DimensionalityReduction.PCA
{
    public class PrincipalComponentAnalysisReduction : DimensionalityReduction, ITrainable
    {
        [JsonProperty]
        protected int numberOfOutputs;
        [JsonProperty]
        protected AnalysisMethod method;
        [JsonProperty]
        protected InternalPrincipalComponentAnalysis pca = null;

        protected PrincipalComponentAnalysisReduction(AnalysisMethod method, int numberOfOutputs, InternalPrincipalComponentAnalysis pca, Preprocessor predecessor)
            : base(predecessor)
        {
            this.method = method;
            this.numberOfOutputs = numberOfOutputs;
            this.pca = pca;
        }

        public PrincipalComponentAnalysisReduction(AnalysisMethod method, int numberOfOutputs) : this(method, numberOfOutputs, null, null) { }

        public bool IsTrained()
        {
            return pca != null;
        }

        public P Train<P>(List<ISample> trainingSamples) where P : Preprocessor
        {
            this.pca = new InternalPrincipalComponentAnalysis((PrincipalComponentMethod)method, numberOfOutputs: numberOfOutputs, whiten: true);

            double[][] data = trainingSamples.Select(sample => sample.GetDimensions()).ToArray();
            pca.Learn(data);

            return this as P;
        }

        protected override double[] Reduce(ISample sample)
        {
            return pca.Transform(sample.GetDimensions());
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new PrincipalComponentAnalysisReduction(method, numberOfOutputs, pca, predecessor);
        }

        [DataContract]
        protected class InternalPrincipalComponentAnalysis : PrincipalComponentAnalysis
        {
            [DataMember]
            public new double[] Eigenvalues
            {
                get
                {
                    return base.Eigenvalues;
                }
                set
                {
                    base.Eigenvalues = value;
                }
            }

            [DataMember]
            public new double[][] ComponentVectors
            {
                get
                {
                    return base.ComponentVectors;
                }
                set
                {
                    base.ComponentVectors = value;
                }
            }

            [DataMember]
            public new int NumberOfInputs
            {
                get
                {
                    return base.NumberOfInputs;
                }
                set
                {
                    base.NumberOfInputs = value;
                }
            }

            [DataMember]
            public new int NumberOfOutputs
            {
                get
                {
                    return base.NumberOfOutputs;
                }
                set
                {
                    base.NumberOfOutputs = value;
                }
            }

            [DataMember]
            public new PrincipalComponentMethod Method
            {
                get
                {
                    return base.Method;
                }
                set
                {
                    base.Method = value;
                }
            }

            [DataMember]
            public new bool Whiten
            {
                get
                {
                    return base.Whiten;
                }
                set
                {
                    base.Whiten = value;
                }
            }

            [DataMember]
            public new double[] StandardDeviations
            {
                get
                {
                    return base.StandardDeviations;
                }
                set
                {
                    base.StandardDeviations = value;
                }
            }

            [DataMember]
            public new double[] Means
            {
                get
                {
                    return base.Means;
                }
                set
                {
                    base.Means = value;
                }
            }

            public InternalPrincipalComponentAnalysis(PrincipalComponentMethod method = PrincipalComponentMethod.Center, bool whiten = false, int numberOfOutputs = 0) :
                base(method, whiten, numberOfOutputs)
            {

            }
        }
    }
}

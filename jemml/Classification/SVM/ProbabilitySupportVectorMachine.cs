using System;
using System.Collections.Generic;
using System.Text;
using Encog;
using Encog.MathUtil.LIBSVM;
using Encog.ML.Data;
using Encog.ML.SVM;

namespace jemml.Classification.SVM
{
    public class ProbabilitySupportVectorMachine : SupportVectorMachine
    {
        ProbabilitySupportVectorMachine() { /* for serialization */ }

        public ProbabilitySupportVectorMachine(int theInputCount, bool regression, double eps) : base(theInputCount, regression)
        {
            this.Params.probability = 1;
            this.Params.eps = eps;
        }

        public ProbabilitySupportVectorMachine(svm_model theModel) : base(theModel)
        {
            this.Params.probability = 1;
        }

        public double Verify(IMLData input, int identifier, int identifierCount)
        {
            if (this.Model == null)
            {
                throw new EncogError("Can't use the SVM yet, it has not been trained and no model exists");
            }

            svm_node[] formattedInput = MakeSparse(input);

            double[] probabilityEstimate = new double[identifierCount];

            svm.svm_predict_probability(this.Model, formattedInput, probabilityEstimate);
            return probabilityEstimate[identifier];
        }
    }
}

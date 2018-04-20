using System;
using System.Collections.Generic;
using System.Text;
using Accord.Statistics.Analysis;

namespace jemml.Data.Transform.DimensionalityReduction.PCA
{
    public enum AnalysisMethod
    {
        Covariance = PrincipalComponentMethod.Center, // formerly Accord AnalysisMethod.Covariance
        Correlation = PrincipalComponentMethod.Standardize // formerly Accord AnalysisMethod.Correlation
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Classification
{
    /// <summary>
    /// The base interface upon which classifiers are derived
    /// </summary>
    public interface IClassifier
    {
        double Verify(double[] features, string identifier);
    }
}

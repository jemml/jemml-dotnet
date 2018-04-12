using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Transform
{
    public interface Trainable
    {
        bool IsTrained();
        P Train<P>(List<Sample> trainingSamples) where P : Preprocessor;
    }
}

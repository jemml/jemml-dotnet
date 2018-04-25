using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Transform
{
    public interface ITrainable
    {
        bool IsTrained();
        P Train<P>(List<ISample> trainingSamples) where P : Preprocessor;
    }
}

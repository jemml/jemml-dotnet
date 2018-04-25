using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data
{
    /// <summary>
    /// Visitor for Sample
    /// </summary>
    public interface ISampleVisitor<T>
    {
        T Accept(StandardSample sample);
        T Accept(CrossValidatedSample sample);
    }
}

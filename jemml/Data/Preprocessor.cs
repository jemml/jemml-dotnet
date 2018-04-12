using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Data
{
    /// <summary>
    /// Base class upon which all preprocessors (feature extractors, normalizers, etc) are derived
    /// </summary>
    public abstract class Preprocessor
    {
        [JsonProperty]
        private Preprocessor predecessor;

        public Preprocessor()
        {
            predecessor = null;
        }

        protected Preprocessor(Preprocessor predecessor)
        {
            this.predecessor = predecessor;
        }

        public Sample Evaluate(Sample sample)
        {
            if (GetPredecessor() != null)
            {
                return Process(GetPredecessor().Evaluate(sample)); // evaluate the predecessor preprocessor before evaluating this
            }
            else
            {
                return Process(sample); // no predecessor preprocessor so evaluate the sample directly
            }
        }

        public Preprocessor SetPredecessor(Preprocessor predecessor)
        {
            // if a predecessor already exists then set this as the predecessor of the predecessor
            if (this.predecessor != null)
            {
                this.predecessor.SetPredecessor(predecessor);
            }
            else
            {
                this.predecessor = predecessor;
            }
            return this;
        }

        public Preprocessor GetPredecessor()
        {
            return predecessor;
        }

        protected abstract Sample Process(Sample sample);

        public Preprocessor Copy()
        {
            return Copy(predecessor);
        }

        protected abstract Preprocessor Copy(Preprocessor predecessor);
    }
}

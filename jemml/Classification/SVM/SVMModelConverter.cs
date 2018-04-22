using System;
using System.Collections.Generic;
using System.Text;
using Encog.MathUtil.LIBSVM;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Classification.SVM
{
    public class SVMModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(svm_model));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            SVMModel dmodel = serializer.Deserialize<SVMModel>(reader);
            svm_model model = new svm_model();
            model.SV = dmodel.SV;
            DataHelpers.SetFieldValue<int>(model, "l", dmodel.l);
            DataHelpers.SetFieldValue<int[]>(model, "label", dmodel.label);
            DataHelpers.SetFieldValue<int>(model, "nr_class", dmodel.nr_class);
            DataHelpers.SetFieldValue<int[]>(model, "nSV", dmodel.nSV);
            DataHelpers.SetFieldValue<double[]>(model, "probA", dmodel.probA);
            DataHelpers.SetFieldValue<double[]>(model, "probB", dmodel.probB);
            DataHelpers.SetFieldValue<double[]>(model, "rho", dmodel.rho);
            DataHelpers.SetFieldValue<double[][]>(model, "sv_coef", dmodel.sv_coef);
            DataHelpers.SetFieldValue<svm_parameter>(model, "param", dmodel.param);
            return model;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            svm_model model = value as svm_model;
            SVMModel smodel = new SVMModel(model);
            serializer.Serialize(writer, smodel);
        }

        private class SVMModel
        {
            [JsonProperty]
            public int l;
            [JsonProperty]
            public int[] label;
            [JsonProperty]
            public int nr_class;
            [JsonProperty]
            public int[] nSV;
            [JsonProperty]
            public double[] probA;
            [JsonProperty]
            public double[] probB;
            [JsonProperty]
            public double[] rho;
            [JsonProperty]
            public double[][] sv_coef;
            [JsonProperty]
            public svm_node[][] SV;
            [JsonProperty]
            public svm_parameter param;

            [JsonConstructor]
            SVMModel() { /* for serialization */ }

            public SVMModel(svm_model model)
            {
                l = DataHelpers.GetFieldValue<int>(model, "l");
                label = DataHelpers.GetFieldValue<int[]>(model, "label");
                nr_class = DataHelpers.GetFieldValue<int>(model, "nr_class");
                nSV = DataHelpers.GetFieldValue<int[]>(model, "nSV");
                probA = DataHelpers.GetFieldValue<double[]>(model, "probA");
                probB = DataHelpers.GetFieldValue<double[]>(model, "probB");
                rho = DataHelpers.GetFieldValue<double[]>(model, "rho");
                sv_coef = DataHelpers.GetFieldValue<double[][]>(model, "sv_coef");
                SV = model.SV;
                param = DataHelpers.GetFieldValue<svm_parameter>(model, "param");
            }
        }
    }
}

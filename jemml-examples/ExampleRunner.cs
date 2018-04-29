using System;
using System.IO;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace jemml_examples
{
    [Command(Name = "jemml-examples", Description = "This tool ")]
    public class ExampleRunner
    {
        public static void Main(string[] args) => CommandLineApplication.Execute<ExampleRunner>(args);

        [Option("-i | --input", Description = "The input file")]
        public string InputFile { get; }

        private void OnExecute()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            Stream test = assembly.GetManifestResourceStream("jemml-examples.Configuration.schema.json");
            using (StreamReader schemaReader = new StreamReader(assembly.GetManifestResourceStream("jemml-examples.Configuration.schema.json")))
            {
                string schema = schemaReader.ReadToEnd();
                Console.WriteLine("schema: {0}", schema);
            }

            string inputtest = File.ReadAllText(InputFile);
            Console.WriteLine("input file {0}", InputFile);
            Console.ReadLine();
        }

        //{
        //    console.writeline("hello world!");
        //    console.writeline("number of arguments: {0}", args.length);
        //    console.readline();
        //}
    }
}

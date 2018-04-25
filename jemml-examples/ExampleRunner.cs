using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

namespace jemml_examples
{
    [Command(Name = "jemml-examples", Description = "This tool ")]
    public class ExampleRunner
    {
        public static void Main(string[] args) => CommandLineApplication.Execute<ExampleRunner>(args);

        [Option("-i | --input", Description = "The input file")]
        public string InputFile { get; }

        [Option("-o | --output", Description = "The output file", ShortName = "o")]
        public string OutputFile { get; }

        private void OnExecute()
        {
            string inputtest = File.ReadAllText(InputFile);
            Console.WriteLine("input file {0}, output file {1}", InputFile, OutputFile);
            Console.ReadLine();
        }

        //{
        //    console.writeline("hello world!");
        //    console.writeline("number of arguments: {0}", args.length);
        //    console.readline();
        //}
    }
}

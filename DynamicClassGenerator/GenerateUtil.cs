using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace DynamicClassGenerator
{
    public class GenerateUtil
    {
        public static void GenerateCSFile(string fileName, CodeCompileUnit targetUnit)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions
            {
                BracingStyle = "C"
            };

            using (StreamWriter sourceWriter = new StreamWriter(fileName))
            {
                provider.GenerateCodeFromCompileUnit(targetUnit, sourceWriter, options);
            }
        }

        public static void GenerateDll(string fileName, CodeCompileUnit targetUnit)
        {
            try
            {
                var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
                var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, fileName, false)
                {
                    GenerateExecutable = false
                };
                var results = csc.CompileAssemblyFromDom(parameters, targetUnit);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

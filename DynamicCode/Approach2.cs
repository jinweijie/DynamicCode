using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DynamicCode
{
    public class Approach2
    {
        public void Run()
        {   
            var csharp = @"using DynamicCode;

            public class TempClass 
            { 
                public string TempMethod(string name)
                {
                    var model = new Model {Name = ""Model name is:"" + name};
                    return model.Name;
                }
            }

            public static class TempStaticClass 
            {
                public static string TempStaticMethod(string name)
                {
                    var model = new Model {Name = ""(Static) Model name is:"" + name};
                    return model.Name;
                }
            }";

            
            var dotnetCoreDirectory = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);

            var compilation = CSharpCompilation.Create("TempLibrary")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(Model).GetTypeInfo().Assembly.Location), // the model location
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(dotnetCoreDirectory, "System.Runtime.dll")))
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(csharp));

            // Debug output. In case your environment is different it may show some messages.
            foreach (var compilerMessage in compilation.GetDiagnostics())
                Console.WriteLine(compilerMessage);
            
            
            #region option 1
            
            //            //write to file
            //            var fileName = "TempLibrary.dll";
            //            var emitResult = compilation.Emit(fileName);
            //            if (emitResult.Success)
            //            {
            //                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(fileName));
            //
            //                assembly.GetType("TempClass").GetMethod("TempMethod").Invoke(null, null);
            //            }
            #endregion    
            
            // in memory string
            using (var memoryStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(memoryStream);
                if (emitResult.Success)
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var context = AssemblyLoadContext.Default;
                    var assembly = context.LoadFromStream(memoryStream);

                    // non-static
                    var instance = assembly.CreateInstance("TempClass");
                    var result1 = assembly.GetType("TempClass").GetMethod("TempMethod").Invoke(instance, new [] {"jinweijie"});
                    
                    // static
                    var result2 = assembly.GetType("TempStaticClass").GetMethod("TempStaticMethod").Invoke(null, new [] {"jinweijie"});
                    
                    Console.WriteLine(result1);
                    Console.WriteLine(result2);
                    
                }
            }
        }
    }
}
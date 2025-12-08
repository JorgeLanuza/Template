
using System;
using System.IO;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("START PROBE");
        var nugetCache = @"c:\Users\jorge\source\repos\BaseCore.Framework\artifacts"; 
        
        var configAssembly = LoadAssembly(Path.Combine(nugetCache, "BaseCore.Framework.Configuration.1.0.0.nupkg"), "BaseCore.Framework.Configuration.dll");

        if (configAssembly != null)
        {
            Console.WriteLine("Searching for types implementing IBaseCoreApplicationSettings...");
            var interfaceType = configAssembly.GetType("BaseCore.Framework.Configuration.Interfaces.IBaseCoreApplicationSettings");
            
            if (interfaceType != null)
            {
                Console.WriteLine($"Interface found: {interfaceType.FullName}");
            }

            Console.WriteLine("Listing all public exported types:");
            foreach (var type in configAssembly.GetExportedTypes())
            {
                 Console.WriteLine($" - {type.FullName}");
                 if (interfaceType != null && interfaceType.IsAssignableFrom(type) && type.IsClass)
                 {
                     Console.WriteLine($"   ^ IMPLEMENTS IBaseCoreApplicationSettings");
                 }
            }
        }
        Console.WriteLine("END PROBE");
    }

    static Assembly LoadAssembly(string nupkgPath, string dllName)
    {
        if (!File.Exists(nupkgPath)) { Console.WriteLine($"Nupkg not found: {nupkgPath}"); return null; }
        
        using (var archive = new System.IO.Compression.ZipArchive(File.OpenRead(nupkgPath)))
        {
            var entry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith($"lib/net10.0/{dllName}") || e.FullName.EndsWith($"lib/net8.0/{dllName}")); 
             if (entry == null) entry = archive.Entries.FirstOrDefault(e => e.Name == dllName);

            if (entry != null)
            {
                using (var stream = entry.Open())
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return Assembly.Load(ms.ToArray());
                }
            }
        }
        return null;
    }
}

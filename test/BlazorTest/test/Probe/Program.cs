using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

Console.WriteLine("Probing Assemblies...");

var assemblies = AppDomain.CurrentDomain.GetAssemblies();
// Load BaseCore assemblies explicitly if not loaded
try { Assembly.Load("BaseCore.Framework.Web"); } catch {}
try { Assembly.Load("BaseCore.Framework.IdentityServer"); } catch {}

assemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName.Contains("BaseCore")).ToArray();

Console.WriteLine($"Found {assemblies.Length} BaseCore assemblies.");

foreach (var assembly in assemblies)
{
    Console.WriteLine($"Assembly: {assembly.GetName().Name}");
    var exported = assembly.GetExportedTypes();
    Console.WriteLine($"Exported Public Types: {exported.Length}");
    foreach (var t in exported)
    {
        Console.WriteLine($"  Public Type: {t.FullName}");
        foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(x => x.Name.StartsWith("Add")))
        {
             Console.WriteLine($"    Method: {m.Name}");
        }
    }
}

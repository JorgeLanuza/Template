using System;
using System.Linq;
using System.Reflection;

Console.WriteLine("START PROBE");

try { Assembly.Load("BaseCore.Framework.Web"); } catch {}
try { Assembly.Load("BaseCore.Framework.Domain"); } catch {}
try { Assembly.Load("BaseCore.Framework.Infrastructure"); } catch {}
try { Assembly.Load("BaseCore.Framework.IdentityServer"); } catch {}
try { Assembly.Load("BaseCore.Framework.Security.Identity"); } catch (Exception e) { Console.WriteLine("Load Security failed: " + e.Message); }

var asms = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("BaseCore")).ToArray();

foreach(var a in asms) {
    if(!a.FullName.Contains("Security.Identity")) continue;
    try {
        var types = a.GetExportedTypes();
        foreach(var t in types) {
             if(t.Name.Contains("DependencyInjection")) {
                 Console.WriteLine(" Type: " + t.FullName);
                 foreach(var m in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)) {
                     Console.Write("  Method: " + m.Name + " Params: ");
                     foreach(var p in m.GetParameters()) Console.Write(p.ParameterType.Name + " " + p.Name + ", ");
                     Console.WriteLine();
                 }
             }
        }
    } catch (Exception ex) { Console.WriteLine("Ex: " + ex.Message); }
}
Console.WriteLine("END PROBE");

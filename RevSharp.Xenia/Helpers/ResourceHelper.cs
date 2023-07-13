using System.Reflection;

namespace RevSharp.Xenia.Helpers;

public static class ResourceHelper
{
    public static string GetAsString(Assembly assembly, string name)
    {
        using (var asmStream = assembly.GetManifestResourceStream(name))
        using (var reader = new StreamReader(asmStream))
        {
            return reader.ReadToEnd();
        }
    }
}
using RevSharp.Xenia.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace RevSharp.Xenia.Helpers;

public static class XeniaHelper
{
    public static string CreateSha256Hash(byte[] bytes)
    {
        // Create a SHA256
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Convert byte array to a string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
    public static string GenerateHelp(CommandModule mod, List<(string, string)> data)
    {
        var r = mod.Reflection.Config.Prefix + mod.BaseCommandName;
        var list = new List<string>();
        foreach (var i in data)
        {
            list.Add($">`{r} {i.Item1}`");
            list.Add($"{i.Item2}");
        }
        return string.Join("\n", list);
    }
}
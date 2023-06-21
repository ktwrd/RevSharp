using System.Security.Cryptography;
using System.Text;

namespace RevSharp.Skidbot.Helpers;

public static class SkidbotHelper
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
}
using System.Text.Json;
using RevSharp.Core.Models.Errors;

namespace RevSharp.Core;

public class RevoltDescriptiveException : Exception
{
    public RevoltDescriptiveException(int code, string reason, string description, Exception? innerException = null)
        : base(string.Join("\n", new string[]
        {
            $"{reason} ({code})",
            description
        }), innerException)
    {}
    public RevoltDescriptiveException(RevoltGenericErrorResponse<RevoltHTTPErrorResponse>? content, Exception? innerException = null)
        : this(content?.Error.Code ?? 0, content?.Error.Reason ?? "", content?.Error.Description ?? "", innerException)
    {}
    public RevoltDescriptiveException(string jsonContent, Exception? innerException = null)
        : this(JsonSerializer.Deserialize<RevoltGenericErrorResponse<RevoltHTTPErrorResponse>>(
            jsonContent, Client.SerializerOptions), innerException)
    {
    }
}
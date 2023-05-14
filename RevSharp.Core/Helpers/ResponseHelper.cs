using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Core.Models.Errors;

namespace RevSharp.Core.Helpers;

public static class ResponseHelper
{
    public static void ThrowException(string content)
    {
        var basedTypedResponse = JsonSerializer.Deserialize<BaseTypedResponse>(content, Client.SerializerOptions);
        switch (basedTypedResponse.Type)
        {
            case "MissingPermission":
                throw new MissingPermissionException(content);
            default:
                throw new RevoltException(basedTypedResponse.Type, content);
        }
        
    }
}
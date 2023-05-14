using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Core.Models.Errors;

namespace RevSharp.Core.Helpers;

public static class ResponseHelper
{
    public static Exception? ParseException(string content)
    {
        var basedTypedResponse = JsonSerializer.Deserialize<BaseTypedResponse>(content, Client.SerializerOptions);
        switch (basedTypedResponse.Type)
        {
            case "MissingPermission":
                return new MissingPermissionException(content);
                break;
            default:
                return new RevoltException(basedTypedResponse.Type, content);
        }
        
    }
}
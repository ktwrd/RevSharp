using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class LoginBaseResponse
{
    [JsonPropertyName("result")]
    public string Result { get; set; }

    public static LoginBaseResponse? Parse(string json)
    {
        var baseData = JsonSerializer.Deserialize<LoginBaseResponse>(json, Client.SerializerOptions);
        switch (baseData.Result)
        {
            case "Success":
                return JsonSerializer.Deserialize<LoginSuccessResponse>(json, Client.SerializerOptions);
            case "MFA":
                return JsonSerializer.Deserialize<LoginMFAResponse>(json, Client.SerializerOptions);
            case "Disabled":
                return JsonSerializer.Deserialize<LoginDisabledResponse>(json, Client.SerializerOptions);
        }

        return null;
    }
}
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.Errors;

public class MissingPermissionData : BaseTypedResponse
{
    [JsonPropertyName("permission")]
    public string Permission { get; set; }
}
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class ServerRoleUpdateMessage : BonfireGenericData<PartialRole>
{
    [JsonPropertyName("role_id")]
    public string RoleId { get; set; }
    [JsonPropertyName("clear")]
    public FieldsRole[] Clear { get; set; }
}

public enum FieldsRole
{
    [EnumMember(Value = "Colour")]
    Colour
}
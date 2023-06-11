using System.Text.Json;
using RevSharp.Core.Models.Errors;

namespace RevSharp.Core;

public class MissingPermissionException : RevoltException
{
    public MissingPermissionException(string permission, string type)
        : base($"{type} {permission}")
    {}
    public MissingPermissionException(MissingPermissionData? data)
        : this(data?.Permission ?? "Unknown", data?.Type ?? "MissingPermission")
    {}
    public MissingPermissionException(string data)
        : this(JsonSerializer.Deserialize<MissingPermissionData>(data, Client.SerializerOptions))
    {
        var deser = JsonSerializer.Deserialize<MissingPermissionData>(data, Client.SerializerOptions);
        Permission = deser.Permission;
        Message = deser.Type;
        ResponseContent = data;
    }
    public string Permission { get; private set; }
}
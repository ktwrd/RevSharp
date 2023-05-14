using System.Text.Json;
using RevSharp.Core.Models.Errors;

namespace RevSharp.Core;

public class MissingPermissionException : RevoltException
{
    public MissingPermissionException(string data)
        : base("")
    {
        var deser = JsonSerializer.Deserialize<MissingPermissionData>(data, Client.SerializerOptions);
        Permission = deser.Permission;
        Message = deser.Type;
        ResponseContent = data;
    }
    public string Permission { get; private set; }
}
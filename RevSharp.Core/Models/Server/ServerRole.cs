using System.Text.Json.Serialization;
using kate.shared.Helpers;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core.Models;

public class ServerRole
{
    /// <summary>
    /// Role Name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// This can be any valid CSS colour
    /// </summary>
    [JsonPropertyName("colour")]
    public string? Colour { get; set; }
    /// <summary>
    /// Whether this role should be shown separately on the member sidebar
    /// </summary>
    [JsonPropertyName("hoist")]
    public bool Hoist { get; set; }
    /// <summary>
    /// Ranking of this role
    /// </summary>
    [JsonPropertyName("rank")]
    public long Rank { get; set; }
    [JsonPropertyName("permissions")]
    public PermissionCompare Permissions { get; set; }

    public event VoidDelegate Deleted;
    public event VoidDelegate Update;

    internal void OnDeleted()
    {
        Deleted?.Invoke();
    }

    internal void OnUpdate()
    {
        Update?.Invoke();
    }

    public static void Inject(ServerRole source, ServerRole target)
    {
        target.Name = source.Name;
        target.Colour = source.Colour;
        target.Hoist = source.Hoist;
        target.Rank = source.Rank;
        target.Permissions = source.Permissions;
    }

    public static void Inject(PartialRole source, ServerRole target)
    {
        if (source.Name != null)
            target.Name = source.Name;
        if (source.Colour != null)
            target.Colour = source.Colour;
        if (source.Hoist != null)
            target.Hoist = (bool)source.Hoist;
        if (source.Rank != null)
            target.Rank = (long)source.Rank;
        if (source.Permissions != null)
            target.Permissions = source.Permissions;
    }

    public static void Inject(ServerRoleUpdateMessage source, ServerRole target)
    {
        Inject(source.Data, target);
        if (source.Clear != null && source.Clear.Contains(FieldsRole.Colour))
        {
            target.Colour = null;
        }
    }
    
}
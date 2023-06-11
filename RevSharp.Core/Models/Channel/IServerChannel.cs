namespace RevSharp.Core.Models;

public interface IServerChannel : INamedChannel
{
    public string ServerId { get; set; }
    public bool IsNsfw { get; set; }
    public PermissionCompare? DefaultPermissions { get; set; }
    public Dictionary<string, PermissionCompare> RolePermissions { get; set; }

    public Task<bool> SetRolePermission(Client client, string roleId, long allow, long deny);
    public Task<bool> SetRolePermission(string roleId, long allow, long deny);
    public Task<bool> SetDefaultPermission(Client client, long allow, long deny);
    public Task<bool> SetDefaultPermission(long allow, long deny);
}
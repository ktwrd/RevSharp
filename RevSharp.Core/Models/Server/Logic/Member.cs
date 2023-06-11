using System.Formats.Asn1;

namespace RevSharp.Core.Models;

public partial class Server
{
    private Dictionary<string, Member> MemberCache = new Dictionary<string, Member>();
    
    internal bool AddToCache(Member member)
    {
        if (MemberCache.ContainsKey(member.Id.UserId))
            return true;
        MemberCache.Add(member.Id.UserId, member);
        member.Client = Client;
        return false;
    }

    public async Task<bool?> MemberHasRole(string memberId, string roleId, bool forceUpdate = true)
    {
        var roles = await GetMemberRoles(memberId, forceUpdate);

        foreach (var i in roles)
        {
            if (i.Item1 == roleId)
                return true;
        }

        return false;
    }

    public async Task<bool?> CanMemberAccessRole(string memberId, string roleId, bool forceUpdate = true)
    {
        var highestMemberRole = await GetHighestMemberRole(memberId, forceUpdate);
        if (highestMemberRole == null)
            return null;
        foreach (var i in Roles)
        {
            if (i.Value.Rank > highestMemberRole.Value.Item2.Rank)
                return true;
        }

        return false;
    }
    public async Task<(string, ServerRole)?> GetHighestMemberRole(string memberId, bool forceUpdate = true)
    {
        var roles = await GetMemberRoles(memberId, forceUpdate);

        if (roles == null)
            return null;
        
        string? highestRole = null;
        var highestRoleRank = long.MaxValue;
        ServerRole? role = null;
        foreach (var (id, r) in roles)
        {
            if (r.Rank < highestRoleRank)
            {
                highestRoleRank = r.Rank;
                highestRole = id;
                role = r;
            }
        }

        if (highestRole == null || role == null)
            return null;

        return (highestRole, role);
    }

    /// <summary>
    /// Get a LinkedList of the roles that a member has.
    /// </summary>
    /// <exception cref="RevoltException">When member is not found.</exception>
    public async Task<LinkedList<(string, ServerRole)>?> GetMemberRoles(string memberId, bool forceUpdate = true)
    {
        Member? targetMember = await GetMember(memberId, forceUpdate);
        if (targetMember == null)
        {
            throw new RevoltException("MemberNotFound");

            return null;
        }

        var roles = new LinkedList<(string, ServerRole)>();
        foreach (var pair in Roles)
        {
            if (targetMember.RoleIds.Contains(pair.Key))
            {
                roles.AddLast((pair.Key, pair.Value));
            }
        }

        return roles;
    }
    
    public async Task<Member?> GetMember(string id, bool forceUpdate = true)
    {
        if (MemberCache.TryGetValue(id, out var member))
        {
            if (!forceUpdate)
                return member;
            
            if (await member.Fetch())
                return member;
            return null;
        }

        var instance = new Member();
        instance.Id.UserId = id;
        instance.Id.ServerId = Id;
        instance.Client = Client;
        if (await instance.Fetch())
        {
            AddToCache(instance);
            return MemberCache[id];
        }

        return null;
    }
}
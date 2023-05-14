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

    public async Task<Member?> GetMember(string id)
    {
        if (MemberCache.TryGetValue(id, out var member))
        {
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
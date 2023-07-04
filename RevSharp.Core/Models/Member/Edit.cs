using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public partial class Member
{
    public async Task<bool> Edit(Client client, DataMemberEdit data)
    {
        var ser = JsonSerializer.Serialize(data, Client.PutSerializerOptions);
        var con = new StringContent(ser, null, "application/json");
        var url = Client.SEndpoint.ServerMember(Id.ServerId, Id.UserId);
        var response = await client.PatchAsync(url, con);
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new Exception("NotFound");
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        return await Fetch(client);
    }

    public Task<bool> Edit(DataMemberEdit data) => Edit(Client, data);
}

public class DataMemberEdit
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
    [JsonPropertyName("avatar")]
    public string? AvatarId { get; set; }
    [JsonPropertyName("roles")]
    public string[]? RoleIds { get; set; }
    [JsonPropertyName("timeout")]
    public string? Timeout { get; set; }
    [JsonPropertyName("remove")]
    public string[]? Remove { get; set; }
}
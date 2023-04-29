using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserRelation : IUserRelation
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("status")]
    public RelationshipStatus Status { get; set; }
}
public enum UserRelationship
{
    None,
    User,
    Friend,
    Outgoing,
    Incoming,
    Blocked,
    BlockedOther
}
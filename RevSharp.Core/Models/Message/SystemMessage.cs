namespace RevSharp.Core.Models;

public class SystemMessage
{
    public string Type { get; set; }
    public string? Content { get; set; }
    public string? Id { get; set; }
    public string? By { get; set; }
    public string? Name { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
}
public enum SystemMessageType
{
    text,
    user_added,
    user_remove,
    user_joined,
    user_left,
    user_kicked,
    channel_renamed,
    channel_description_changed,
    channel_icon_changed,
    channel_ownership_changed
}
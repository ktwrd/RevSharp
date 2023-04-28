using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class Interactions
{
    [JsonPropertyName("reactions")]
    public string[] Reactions { get; set; }
    [JsonPropertyName("restrict_reactions")]
    public bool RestrictReactions { get; set; }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RevSharp.Core.Models
{
    public class DataEditRole
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("colour")]
        public string? Colour { get; set; }
        [JsonPropertyName("hoist")]
        public bool? Hoist { get; set; }
        [JsonPropertyName("rank")]
        public long? Rank { get; set; }
        [JsonPropertyName("remove")]
        public string[]? RemoveItems { get; set; }
    }
}

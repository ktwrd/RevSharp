using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class AnimalModule : BaseModule
{
    private List<string> CachedAnimals = new List<string>();
    private HttpClient _httpClient = new HttpClient();

    public class TinyFoxResponse
    {
        [JsonPropertyName("usage")]
        public string UsageUrl { get; set; }
        [JsonPropertyName("available")]
        public string[] Available { get; set; }
        [JsonPropertyName("remaining_api_calls")]
        public string RemainingApiCalls { get; set; }
        [JsonPropertyName("loc")]
        public string Location { get; set; }
    }

    public override async Task Initialize(ReflectionInclude reflection)
    {
        await InitFetch();
    }
    private async Task InitFetch()
    {
        var response = await _httpClient.GetAsync($"https://api.tinyfox.dev/img");
        var stringContent = response.Content.ReadAsStringAsync().Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Console.Error.WriteLine($"[AnimalModule->Init] Failed to fetch info (code: {response.StatusCode})\n{stringContent}");
            return;
        }

        var deser = JsonSerializer.Deserialize<TinyFoxResponse>(stringContent, Program.SerializerOptions);
        if (deser == null)
        {
            Console.Error.WriteLine($"[AnimalModule->Init] Failed to deserialize\n{stringContent}");
            return;
        }

        CachedAnimals = deser.Available.ToList();
    }
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        if (info?.Command != "animal")
            return;

        if (CachedAnimals.Count < 1)
            await InitFetch();

        var embed = new SendableEmbed()
        {
            Title = "Random Animal"
        };
        if (info.Arguments.Count < 1)
        {
            embed.Description = string.Join("\n", new string[]
            {
                "Available Animals",
                string.Join("\n", CachedAnimals.Select(v => $"- `{v}`"))
            });
            await message.Reply(embed);
            return;
        }

        if (info.Arguments[0] == "help")
        {
            embed.Title += " - Help";
            embed.Description = HelpContent();
            await message.Reply(embed);
            return;
        }

        if (CachedAnimals.Contains(info.Arguments[0]))
        {
            var url = await FetchImage(info.Arguments[0]);
            if (url == null)
            {
                embed.Description = "Failed to fetch image";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }

            await message.Reply($"https://api.tinyfox.dev{url}");
            return;
        }

        embed.Description = $"Invalid animal `{info.Arguments[0]}`";
        embed.Colour = "red";
        await message.Reply(embed);
    }

    public async Task<string?> FetchImage(string animal)
    {
        var response = await _httpClient.GetAsync($"https://api.tinyfox.dev/img?animal={animal}&json");
        var stringContent = response.Content.ReadAsStringAsync().Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Console.Error.WriteLine($"[AnimalModule->FetchImage] Failed to fetch {animal} (StatusCode: {response.StatusCode})\n{stringContent}");
            return null;
        }

        var deser = JsonSerializer.Deserialize<TinyFoxResponse>(stringContent, Program.SerializerOptions);
        if (deser == null)
        {
            Console.Error.WriteLine($"[AnimalModule->FetchImage] Failed to deserialize\n{stringContent}");
            return null;
        }

        return deser.Location;
    }
    
    public override string? HelpContent()
    {
        var p = Program.ConfigData.Prefix;
        var lines = new List<string>()
        {
            "```",
            $"{p}animal <animal_type>     - get random image of animal",
            $"{p}animal help              - display this message",
            $"{p}help animal              - display this message",
            $"{p}animal                   - list available animals",
            "```"
        };

        return string.Join("\n", lines);
    }
    public override bool HasHelpContent => true;
    public override string? InternalName => "animal";
    public override string? HelpCategory => "fun";
    public override string? BaseCommandName => "animal";
    public override bool WaitForInit => false;
}
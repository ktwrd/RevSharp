using NetVips;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Reflection;
using System.Diagnostics;
using File = System.IO.File;
using NVIPS = NetVips.NetVips;
using RevoltFile = RevSharp.Core.Models.File;

namespace RevSharp.Skidbot.ImgWiz.Controllers;

[RevSharpModule]
public partial class ImageWizardController : BaseModule
{
    public override async Task Initialize(ReflectionInclude reflectionInclude)
    {
        if (ModuleInitializer.VipsInitialized)
        {
            Log.WriteLine($"Using libvips {NVIPS.Version(0)}.{NVIPS.Version(1)}.{NVIPS.Version(2)}");
        }
        else
        {
            Log.Error($"Failed to init libvips\n{ModuleInitializer.Exception.Message}");
        }
    }

    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        string action = "help";
        if (info.Arguments.Count > 0)
        {
            action = info.Arguments[0].ToLower();
        }

        switch (action)
        {
            case "help":
                await Command_Help(info, message);
                break;
            case "1984":
                await Command_1984(info, message);
                break;
            case "yskysn":
                await Command_YSKYSN(info, message);
                break;
            case "caption":
                await Command_Caption(info, message);
                break;
        }
    }

    private async Task Command_Help(CommandInfo info, Message message)
    {
        await message.Reply(
            new SendableEmbed()
            {
                Title = "Image Wizard - Help",
                Description = HelpContent()
            });
    }
    public async Task UploadPng(Message message, Image img)
    {
        using (var pngStream = new MemoryStream(img.PngsaveBuffer()))
        {
            var uploadId = await Client.UploadFile(
                pngStream,
                "image.png",
                "attachments",
                "image/png");
            if (uploadId != null)
            {
                await message.Reply(new DataMessageSend()
                {
                    Attachments = new string[] { uploadId }
                });
            }
            else
            {
                await message.Reply($"Failed to upload image");
            }
        }
    }
    internal async Task<byte[]?> GetUrlContent(RevoltFile file)
    {
        var client = new HttpClient();
        var res = await client.GetAsync(file.GetURL(Client));
        if (res.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return res.Content.ReadAsByteArrayAsync().Result;
        }
        return null;
    }

    internal string GetAfterArgs(CommandInfo info)
    {
        string caption = "";
        if (info.Arguments.Count >= 2)
        {
            var tmpArgs = new List<string>(info.Arguments);
            tmpArgs.RemoveAt(0);
            if (tmpArgs.Count > 0)
            {
                caption = string.Join(" ", tmpArgs);
            }
        }
        return caption;
    }
    internal string[] ParsePrompt(CommandInfo info, string defaultValue)
    {
        var caption = GetAfterArgs(info);

        var captionSplit = caption.Split("|").ToList();
        if (captionSplit.Count < 2)
        {
            captionSplit.Add(defaultValue);
        }
        return captionSplit.ToArray();
    }
    
    private Image Normalize(Image img)
    {
        if (img.Bands < 3)
            img = img.Colourspace(Enums.Interpretation.Srgb);

        if (img.Bands == 3)
            img = img.Bandjoin(255);

        return img;
    }
    private async Task AttemptFontExtract()
    {
        if (!Directory.Exists(FeatureFlags.FontCache))
            Directory.CreateDirectory(FeatureFlags.FontCache);

        foreach (var pair in FontFilenamePairs)
        {
            var outputLocation = GetFontLocation(pair.Key);
            if (File.Exists(outputLocation))
                continue;
            var obj = ImageWizard.ResourceManager.GetObject(pair.Key) as byte[];
            using (var rs = new MemoryStream(obj))
            using (var fs = new FileStream(outputLocation, FileMode.Create, FileAccess.Write))
            {
                await rs.CopyToAsync(fs);
            }
        }
    }
    public static Dictionary<string, string> FontFilenamePairs => new Dictionary<string, string>()
    {
        {"font_arial", "arial.ttf"},
        {"font_AtkinsonHyperlegible_Bold", "AtkinsonHyperlegible-Bold.ttf"},
        {"font_caption", "caption.otf"},
        {"font_chirp_regular_web", "chip-regular-web.woff2"},
        {"font_HelveticaNeue", "HelveticaNeue.otf"},
        {"font_ImpactMix", "ImpactMix.ttf"},
        {"font_TAHOMABD", "TAHOMABD.TTF"},
        {"font_times_new_roman", "times new roman.ttf"},
        {"font_TwemojiCOLR0", "TwemojiCOLR0.otf"},
        {"font_Ubuntu_R", "Ubuntu-R.ttf"},
        {"font_whisper", "whisper.otf"}
    };
    private string GetFontLocation(string font)
    {
        return Path.Combine(FeatureFlags.FontCache, FontFilenamePairs[font]);
    }

    public async Task<RevoltFile?> GetTargetFile(Message message)
    {
        if (message.Attachments?.Length > 0)
        {
            foreach (var i in message.Attachments)
            {
                if (i.Metadata.Type == "Image")
                {
                    return i;
                }
            }
        }

        var channel = await Client.GetChannel(message.ChannelId);
        if (message.MessageReplyIds?.Length > 0)
        {
            foreach (var id in message.MessageReplyIds)
            {
                var innerMessage = await channel.GetMessage(id);
                if (innerMessage == null)
                    continue;

                var f = await GetTargetFile(innerMessage);
                if (f != null)
                    return f;
            }
        }

        return null;
    }

    public override bool HasHelpContent => true;
    public override string? InternalName => "imgwiz";
    public override string? HelpCategory => "fun";
    public override string? BaseCommandName => "imgwiz";
}
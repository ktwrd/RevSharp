using NetVips;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;
using System.Diagnostics;
using File = System.IO.File;
using NVIPS = NetVips.NetVips;
using RevoltFile = RevSharp.Core.Models.File;
namespace RevSharp.Xenia.ImgWiz.Controllers
{
    public partial class ImageWizardController
    {
        public async Task Command_Caption(CommandInfo info, Message message)
        {
            RevoltFile? targetRevoltFile = await GetTargetFile(message);
            if (targetRevoltFile == null)
            {
                await message.Reply($"Please attach an image to your message or reply to a message that has an image attached for this command to work.");
                return;
            }
            if (targetRevoltFile.ContentType.Contains("gif"))
            {
                await message.Reply($"Caption only supports static images. GIF's aren't supported yet.");
                return;
            }

            var caption = GetAfterArgs(info);
            if (caption == null || caption.Length < 1)
            {
                await message.Reply($"This coommand requires a caption. Please put something after the sub-command");
                return;
            }

            using (var originalFileData = await GetUrlStream(targetRevoltFile))
            {
                if (originalFileData == null)
                {
                    await message.Reply($"Failed to fetch image content");
                    return;
                }
                await message.AddReaction(Client, "✅");

                // Read detected image and scale it down if required
                var imageToCaption = Normalize(Image.NewFromStream(originalFileData));
                imageToCaption = ScaleImage(imageToCaption);

                var fontSize = imageToCaption.Width / 10f;
                var textWidth = imageToCaption.Width - ((imageToCaption.Width / 25) * 2);

                // Create caption text
                var text = Image.Text(
                    text: $"<span background='white'>{caption}</span>",
                    rgba: true,
                    align: Enums.Align.Centre,
                    font: $"FuturaExtraBlackCondensed {fontSize}px",
                    fontfile: GetFontLocation("font_caption"),
                    width: textWidth);

                // Align text and make sure all transparent stuff is white
                text = text.BandAnd()
                    .Ifthenelse(WhiteRGBA, text)
                    .Gravity(
                        Enums.CompassDirection.Centre,
                        imageToCaption.Width,
                        text.Height + 24,
                        extend: Enums.Extend.White,
                        background: WhiteRGBA);

                // Append vertically `imageToCaption` to `text`
                text = text.Join(imageToCaption, Enums.Direction.Vertical, background: WhiteRGBA, expand: true);

                // Upload and cleanup resources
                await UploadPng(message, text);
                imageToCaption.Close();
            }
        }
    }
}

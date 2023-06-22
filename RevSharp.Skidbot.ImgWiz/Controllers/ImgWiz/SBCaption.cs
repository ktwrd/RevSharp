using NetVips;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Reflection;
using System.Diagnostics;
using File = System.IO.File;
using NVIPS = NetVips.NetVips;
using RevoltFile = RevSharp.Core.Models.File;
namespace RevSharp.Skidbot.ImgWiz.Controllers
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
            var caption = GetAfterArgs(info);
            if (caption == null || caption.Length < 1)
            {
                await message.Reply($"This coommand requires a caption. Please put something after the sub-command");
                return;
            }

            var originalFileData = await GetUrlContent(targetRevoltFile);
            if (originalFileData == null)
            {
                await message.Reply($"Failed to fetch image content");
                return;
            }

            var imageToCaption = Normalize(Image.NewFromBuffer(originalFileData));

            var fontSize = targetRevoltFile.Metadata.Width / 10f;
            var textWidth = targetRevoltFile.Metadata.Width - ((targetRevoltFile.Metadata.Width  / 25) * 2);


            var text = Image.Text($"<span background='white'>{caption}</span>",
                rgba: true,
                align: Enums.Align.Centre,
                font: $"FuturaExtraBlackCondensed {fontSize}px",
                fontfile: GetFontLocation("font_caption"),
                width: textWidth);
            text = text.BandAnd().Ifthenelse(new double[] {255, 255, 255, 255}, text)
                .Gravity(
                    Enums.CompassDirection.Centre,
                    imageToCaption.Width,
                    text.Height + 24,
                    extend: Enums.Extend.White,
                    background: new double[] {255, 255, 255, 255});
            text = text.Join(imageToCaption, Enums.Direction.Vertical, background: new double[] { 255, 255, 255, 255 }, expand: true);
            await UploadPng(message, text);
        }
    }
}

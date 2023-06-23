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
        private async Task Command_1984(CommandInfo info, Message message)
        {
            await AttemptFontExtract();
            var embed = new SendableEmbed()
            {
                Title = "Image Wizard = 1984"
            };
            var captionSplit = ParsePrompt(info, "JANUARY 1984");

            bool originaldate = captionSplit[1].ToLower() == "january 1984";

            var img = Image.NewFromBuffer(ImageWizard.img_1984originaldate);
            if (!originaldate)
                img = Image.NewFromBuffer(ImageWizard.img_1984);
            var speechBubble = Image.Text(
                captionSplit[0],
                font: "Atkinson Hyperlegible Bold",
                rgba: true,
                fontfile: GetFontLocation("font_AtkinsonHyperlegible_Bold"),
                align: Enums.Align.Centre,
                width: 290,
                height: 90);
            speechBubble.Gravity(Enums.CompassDirection.Centre, 290, 90, extend: Enums.Extend.Black);
            img = img.Composite2(speechBubble, Enums.BlendMode.Over, x: 60, y: 20);

            if (!originaldate)
            {
                var dateText = Image.Text(
                    $"<span color='black'>{captionSplit[1].ToUpper()}</span>",
                    font: "ImpactMix",
                    rgba: true,
                    fontfile: GetFontLocation("font_ImpactMix"),
                    align: Enums.Align.Centre,
                    width: 124,
                    height: 34);
                dateText = dateText.Gravity(Enums.CompassDirection.Centre, 124, 34, extend: Enums.Extend.Black);
                dateText = dateText.Affine(new double[] { 1, 0, 0.176327, 1 });
                img = img.Composite2(dateText, Enums.BlendMode.Over, x: 454, y: 138);
                img = img.Composite2(Normalize(Image.NewFromBuffer(ImageWizard.img_1984cover)), Enums.BlendMode.Over);
            }

            await UploadPng(message, img);
        }

    }
}

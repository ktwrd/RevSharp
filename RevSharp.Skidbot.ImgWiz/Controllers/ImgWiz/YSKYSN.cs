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
        public async Task Command_YSKYSN(CommandInfo info, Message message)
        {
            var caption = ParsePrompt(info, "NOW!");

            var img = Normalize(Image.NewFromBuffer(ImageWizard.img_yskysn));

            int width = 500;
            int height = 582;
            
            (Image, int) GenThing()
            {
                int afdpi = 0;
                var i = Image.Text(
                        string.Join(" ", new string[]
                        {
                        $"<span foreground='white'>",
                        $"{caption[0]}\n<span size='150%'>{caption[1]}</span>",
                        $"</span>"
                        }),
                        font: "Tahoma Bold 56",
                        rgba: true,
                        fontfile: GetFontLocation("font_TAHOMABD"),
                        align: Enums.Align.Centre,
                        width: width,
                        height: height,
                        autofitDpi: out afdpi);
                return (i, afdpi);
            }
            Image text;
            var autofitDict = GenThing();
            var textPrerender = GenThing();
            if (autofitDict.Item2 <= 72)
                text = textPrerender.Item1;
            else
            {
                text = Image.Text(
                    string.Join(" ", new string[]
                    {
                        "<span foreground='white'>",
                        $"{caption[0].ToUpper()}\n<span size='150%'>{caption[1].ToUpper()}</span>",
                        "</span>"
                    }),
                    font: "Tahoma Bold 56",
                    rgba: true,
                    fontfile: GetFontLocation("font_TAHOMABD"),
                    align: Enums.Align.Centre,
                    width: width,
                    height: height,
                    dpi: 72);
            }

            text = text.Gravity(Enums.CompassDirection.Centre, width + 48, height + 48, extend: Enums.Extend.Black);

            var mask = Image.Gaussmat(5 / 2, 0.0001, separable: true);
            var glow = text[3].Convsep(mask).Cast(Enums.BandFormat.Uchar);
            glow = glow.NewFromImage(255, 255, 255)
                .Bandjoin(glow)
                .Copy(interpretation: Enums.Interpretation.Srgb);

            text = glow.Composite2(text, Enums.BlendMode.Over);

            var output = img.Composite2(text, Enums.BlendMode.Over);

            await UploadPng(message, output);
        }
    }
}

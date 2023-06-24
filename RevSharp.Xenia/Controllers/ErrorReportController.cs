using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevSharp.Xenia.Controllers
{
    [RevSharpModule]
    public class ErrorReportController : BaseModule
    {
        public async Task Report(Exception exception, Message message, string content)
        {
            Log.Error($"Error when handling message in {message.ChannelId} {content}\n{exception}");
            await SendAsMessage(exception, message, content);
        }
        public async Task Report(Exception exception, string content)
        {
            Log.Error($"{content}\n{exception}");
            await SendAsMessage(exception, null, content);
        }
        public Task Report(Exception exception)
            => Report(exception, "");
        private async Task SendAsMessage(Exception exception, Message? message, string content)
        {
            if (Reflection.Config.ErrorLogChannelId == null)
            {
                Log.Warn("ConfigData.ErrorLogChannelId is null. Can't send as message");
                return;
            }
            string messageDetails = "";
            if (message != null)
            {
                var messageServer = await message.FetchServer();
                messageDetails += string.Join("\n", new string[]
                {
                    $"`Server: {messageServer?.Id} ({messageServer?.Name})`",
                    $"`Channel: {message.ChannelId}`",
                    $"`Author: {message.AuthorId}`",
                    $"`Id: {message.Id}`"
                });
                if (messageServer != null)
                    messageDetails += $"\n[Link](https://app.revolt.chat/server/{messageServer.Id}/channel/{message.ChannelId}/{message.Id})";
                else
                    messageDetails += $"\n[Link](https://app.revolt.chat/channel/{message.ChannelId}/{message.Id})";
            }
            var embed = new SendableEmbed()
            {
                Title = "Error Report",
                Description = string.Join("\n", new string[]
                {
                    $">Message",
                    $">`{exception.Message}`",
                    "Stack trace is attached"
                })
            };
            if (messageDetails.Length > 0)
            {
                embed.Description += $"\n### Message Details\n{messageDetails}";
            }
            
            // Attempt to upload exception to revolt.
            var exceptionStr = exception.ToString().ReplaceLineEndings("\n");
            string? exceptionId = null;
            try
            {
                exceptionId = await Client.UploadFile(
                    exceptionStr,
                    "exception.txt",
                    Core.FileTag.Attachment);
            }
            catch (RevoltException rex)
            {
                Log.Warn($"Failed to upload file ({rex.Message})");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to upload file\n{ex}");
            }

            var dataSend = new DataMessageSend()
                .AddEmbed(embed);
            // Add exception attachment if it's not null
            if (exceptionId != null)
            {
                dataSend.AddAttachment(exceptionId);
            }

            try
            {
                var channel = await Client.GetChannel(Reflection.Config.ErrorLogChannelId) as MessageableChannel;
                if (channel == null)
                {
                    Log.Error($"Couldn't fetch channel from Config.ErrorLogChannelId since the result was null");
                }
                await channel.SendMessage(dataSend);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send error notification message.\n{ex}");
                return;
            }
        }
    }
}

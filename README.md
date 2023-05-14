 # RevSharp
 A C# Library for the FOSS chat platform [Revolt](https://revolt.chat) with portability in mind.

[SkidBot Revolt Edition](https://skidbot.kate.pet/posts/revolt-edition) uses this library. You can view the README [here](README.ReBot)

To add RevSharp to your project, you can install via the `RevSharp.Core` package or with the following command;
```bash
dotnet add package RevSharp.Core
```

## Example
```csharp
using RevSharp.Core;
using RevSharp.Core.Models;

public static class Program
{
    public static void Main(string[] args)
        => AsyncMain(args).Wait();
    public static async Task AsyncMain(string[] args)
    {
        // Get token and initialize client
        string token = Environment.GetEnvironmentVariable("REVSHARP_TOKEN");
        var client = new Client(token, true);

        // Login to Revolt server
        await client.LoginAsync();

        // Handle message events
        client.MessageReceived += Client_MessageReceived;

        // Infinitely wait.
        await Task.Delay(-1);
    }

    private static void Client_MessageReceived(Message message)
    {
        if (message.Content == "ping")
        {
            message.Reply("pong!").Wait();
        }
    }
}
```

## Environment Variables
| Name | Type | Default Value | Description |
| ---- | ---- | ------------- | ----------- |
| `REVSHARP_LOG_COLOR` | boolean | `true` | Custom color for log output. Anything other than `true` will disable the log color. |


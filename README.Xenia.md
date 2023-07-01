# Xenia Bot

Xenia is an Open-Source Revolt Bot designed with the balance of simplicity and power, for the end-user and developers. Multiple plugins are provided by default. This includes but isn't limited to;
- [Media Content Detection](RevSharp.Xenia.ConDetect/) - Used for automatically moderating content that contains images/media with Google's Safe Search Vision API
- [Image Wizard](RevSharp.Xenia.ImgWiz/) - esmBot-like Image Manupliation
- [Plugin SDK](RevSharp.Xenia.SDK/) - SDK for easily developing plugins for Xenia. Currently only projects references by [RevSharp.Xenia](RevSharp.Xenia/) will be included.
- [Moderation](RevSharp.Xenia.Moderation/) - Command suite to easily moderate channels and manage your server.

## Contents
- [Why](#why)
- [Links](#links)
- [Development](#development)
  - [Environment Variables](#environment-variables)
  - [Example Config](#example-config)

## Why?
There was a gap in the Revolt Bot market for a multitude of features that would be extremely useful for end-users. Xenia started out as a bot to fill in the gap that is media restrictions on Revolt, which turned into the [Content Detection Module](RevSharp.Xenia.ConDetect/). Then after that was implemented and released, more and more commands/modules have been added to the Official Xenia Bot.

## Links
- [Invite](https://r.kate.pet/xeniainvite)
- [Stats](https://r.kate.pet/xeniastats)
- [Support Server](https://r.kate.pet/revolt)
- [Setup Content Detection(#setup-content-detection)
- Developer Tools
  - [Module Generator](https://ktwrd.github.io/xenia-modulegen.html)
  - [JSON Class Generator](https://ktwrd.github.io/typegen.html)
  - [Enum String Creator](https://ktwrd.github.io/enumgen.html)

## Setup Content Detection

In order to setup content detection for Xenia, you'll have to do the following steps

1. Invite Xenia
2. Give Xenia the following permissions
  - Manage Messages
  - Send Messages
  - Send Embeds
  - View Channels
  - Kick Members
  - Upload Files
  - Use Reactions
3. Create a private channel that Xenia has access to

Once you've done those steps, you can setup Xenia and request access with the following commands;

Run the `r.cdconfig logchannel` command in the channel you just created.

Then run `r.cdconfig request`

## Project File Info
| Name | Description |
| ---- | ----------- |
| `RevSharp.Xenia` | Base Xenia bot. References all projects that include in their name `RevSharp.Xenia.*`
| `RevSharp.Xenia.SDK` | Xenia Plugin SDK. Includes stuff that plugins reference. |
| `RevSharp.Xenia.ConDetect` | Content Detection powered by Google Cloud |
| `RevSharp.Xenia.Moderaiton` | Misc Moderation stuff |
| `RevSharp.Xenia.GoogleCloud` | Google Cloud wrappers and boilerplate code |
| `RevSharp.Xenia.ImgWiz` | `imgwiz` command suite. esmBot clone |
| `RevSharp.Xenia.AdminRest` | Rest API Server for creating web-based admin pages. |
| `RevSharp.Xenia.Mongo` | MongoDB Models |

## Development
Initialize Environment
```bash
# Clone repository
git clone https://github.com/ktwrd/revsharp.git
cd revsharp

# Create MongoDB Database
## Exposed to port 27021
## username: user
## password: password
./initializeMongoDB.sh    # Bash
pwsh initializeMongoDB.ps # Powershell
```

Run Xenia
```bash
# Disable log colors
export REVSHARP_LOG_COLOR="false"

# Run Xenia
dotnet run --project RevSharp.Xenia

# Run Xenia with custom config location (optional)
XE_CONFIG_LOCATION=~/some/location/XE_config.json \
    dotnet run --project RevSharp.Xenia
```

### Environment Variables
[Extends RevSharp.Core](README#environment-variables). All are listed in [FeatureFlags.cs](RevSharp.Xenia.SDK/FeatureFlags.cs)
| Name | Type | Default Value | Description |
| ---- | ---- | ------------- | ----------- |
| `REVSHARP_LOG_COLOR` | boolean | `true` | Custom color for log output |
| `XE_CONFIG_LOCATION` | string | `./{XE_DATA_DIR}/config.json` | Custom location for config |
| `XE_DATA_DIR` | string | `./data/` | Data directory |
| `XE_DIR_IACD` | string | `./{XE_DATA_DIR}/icad` | Content Detection Cache |
| `XE_CONDETECT` | bool | `false` | Enable ContentDetection module |
| `XE_DIR_FC` | string | `./{XE_DATA_DIR}/fontcache` | Font Cache for ImgWiz |
| `XE_PLUGIN_WHITELIST` | string[] | Empty Array | Comma-seperated list of Assemblies to only include. When empty, all assemblies will be searched for ([see code](RevSharp.Xenia.SDK/Reflection/ReflectionInclude.cs#L28)) |

### Example Config
```json
{
    "Token": "",
    "IsBot": false,
    "Prefix": "r.",
    "MonogCollectionU)rl": "mongodb://username:password@localhost:27017",
    "MongoDatabaseName": "xenia_revolt",
    "GoogleCloud": {
        "DefaultCred": {},
        "VisionAPI": null
    },
    "ContentDetectionBucket": "xenia-condetect-data",
    "OwnerUserIds": [],
    "AuthentikUrl": "",
    "AuthentikToken": "",
    "PrometheusPort": 8771,
    "PrometheusEnable": true,
    "LogChannelId": "",
    "LogChannelServerId": "",
    "PublicLogChannelId": "",
    "ErrorLogChannelId": null
}
```

# ReBot README

## Development
Clone repository
```bash
git clone https://github.com/ktwrd/revsharp.git
cd revsharp
```

Run ReBot
```bash
dotnet run --project RevSharp.ReBot

# Disable log colors
export REVSHARP_LOG_COLOR="false"

# Set config location
REBOT_CONFIG_LOCATION=~/Desktop/rebot_config.json \
    dotnet run --project RevSharp.ReBot
```

## Environment Variables
| Name | Type | Default Value | Description |
| ---- | ---- | ------------- | ----------- |
| `REVSHARP_LOG_COLOR` | boolean | `true` | Custom color for log output |
| `REBOT_CONFIG_LOCATION` | string | `./config.json` | Custom location for config |

## Example Config
```json
{
    "Token": "",
    "IsBot": false
}
```
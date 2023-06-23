# ReBot README

## Development
Clone repository
```bash
git clone https://github.com/ktwrd/revsharp.git
cd revsharp
```

Create MongoDB
```bash
./initializeMongoDB.sh
# connection url;
#      mongodb://user:password@localhost:27021
```

Run ReBot
```bash
dotnet run --project RevSharp.ReBot

# Disable log colors
export REVSHARP_LOG_COLOR="false"

# Set config location
XE_CONFIG_LOCATION=~/some/location/XE_config.json \
    dotnet run --project RevSharp.ReBot
```

## Environment Variables
[Extends](README#environment-variables)
| Name | Type | Default Value | Description |
| ---- | ---- | ------------- | ----------- |
| `REVSHARP_LOG_COLOR` | boolean | `true` | Custom color for log output |
| `XE_CONFIG_LOCATION` | string | `./{XE_DATA_DIR}/config.json` | Custom location for config |
| `XE_DATA_DIR` | string | `./data/` | Data directory |
| `XE_DIR_IACD` | string | `./{XE_DATA_DIR}/icad` | Content Detection Cache |
| `XE_CONDETECT` | bool | `false` | Enable ContentDetection module |

## Example Config
```json
{
    "Token": "",
    "IsBot": false,
	"Prefix": "r.",
	"MonogCollectionUrl": "mongodb://username:password@example.com:27017",
	"MongoDatabaseName": "xenia_revolt",
	"GoogleCloud": {
		"DefaultCred": {},
		"VisionAPI": {}
	},
	"AuthentikUrl": "",
	"AuthentikToken": ""
}
```
# AfkDetector

Disconnects players who remain motionless and silent beyond a configurable timeout.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Keeps inactivity detection authoritative on the server.
- Uses server-visible movement and chat activity.
- Filters position jitter with a configurable movement tolerance.
- Shows a specific inactivity message after disconnecting the client.
- Defaults to a 30-minute timeout.
- Supports a server command-line timeout override for managed deployments.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

The client component only displays the server-provided disconnect reason. See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.AfkDetector/README.md) for configuration and detection limits.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

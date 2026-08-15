# AfkDetector

Disconnects players who remain motionless and silent beyond a configurable timeout.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Performs authoritative inactivity detection on the server.
- Samples the private server-side reference position instead of relying on public map visibility.
- Treats nearby, shout, private, and Landoria group messages as activity.
- Ignores empty map-ping chat messages.
- Filters normal position jitter through a configurable movement tolerance.
- Shows a specific inactivity reason after disconnecting the client.
- Lets CharacterVault delay the kick for a confirmed final save when it is installed.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

The client component only receives the disconnect reason and replaces Valheim's
generic kick message. Detection, configuration, and the decision to disconnect
remain server-authoritative. CharacterVault is optional. When installed, it
intercepts the normal Valheim kick and waits for a confirmed final save.

## Configuration

The server reads `--afktimeout <minutes>` once when its network session starts.
The default is `30` minutes, and `-1` disables AFK detection. Other values must be
at least one minute. Movement tolerance is fixed at `0.75` metres.

## Detection limits

AfkDetector observes movement and chat that the server can verify. Inventory use, camera rotation, and local menu interactions are not visible to a server-authoritative mod and do not reset the timer. A player carried by a moving ship may appear active because their network position changes.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

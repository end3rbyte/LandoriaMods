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

| Client required | Server required |
|---|---|
| Yes | Yes |

The client component only receives the disconnect reason and replaces Valheim's
generic kick message. Detection, configuration, and the decision to disconnect
remain server-authoritative. CharacterVault is optional. When installed, it
intercepts the normal Valheim kick and waits for a confirmed final save.

On a peer-hosted world, remote players are monitored. The hosting player is not a remote peer and cannot be disconnected from their own world by this mod.

## Configuration

The server creates `BepInEx/config/Landoria.AfkDetector.cfg` after the first launch.

| BepInEx setting | Default | Purpose |
|---|---:|---|
| `Detection.TimeoutMinutes` | `30` | Minutes without qualifying movement or chat before disconnecting a player. Values below one minute are treated as one minute. |
| `Detection.MovementToleranceMeters` | `0.75` | Minimum distance from the last active position that resets the timer. Values below 0.1 metres are treated as 0.1 metres. |

Configuration changes are applied without restarting the server.

The server command-line switch `--afktimeout <minutes>` takes precedence over
`Detection.TimeoutMinutes`. Landoria test servers launch with `--afktimeout 5`;
production servers launch with `--afktimeout 30`.

## Detection limits

AfkDetector observes movement and chat that the server can verify. Inventory use, camera rotation, and local menu interactions are not visible to a server-authoritative mod and do not reset the timer. A player carried by a moving ship may appear active because their network position changes.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

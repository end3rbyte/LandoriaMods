# AfkDetector

Disconnects players who remain motionless and silent beyond a configurable timeout.

## Features

- Performs authoritative inactivity detection on the server.
- Samples the private server-side reference position instead of relying on public map visibility.
- Treats nearby, shout, private, and Landoria group messages as activity.
- Ignores empty map-ping chat messages.
- Filters normal position jitter through a configurable movement tolerance.
- Shows a specific inactivity reason after disconnecting the client.
- Logs one server event when an inactive player is disconnected.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

The client component only receives the disconnect reason and replaces Valheim's generic kick message. Detection, configuration, and the decision to disconnect remain server-authoritative.

On a peer-hosted world, remote players are monitored. The hosting player is not a remote peer and cannot be disconnected from their own world by this mod.

## Configuration

The server creates `BepInEx/config/Landoria.AfkDetector.cfg` after the first launch.

| BepInEx setting | Default | Purpose |
|---|---:|---|
| `Detection.TimeoutMinutes` | `30` | Minutes without qualifying movement or chat before disconnecting a player. Values below one minute are treated as one minute. |
| `Detection.MovementToleranceMeters` | `0.75` | Minimum distance from the last active position that resets the timer. Values below 0.1 metres are treated as 0.1 metres. |

Configuration changes are applied without restarting the server.

## Detection limits

AfkDetector observes movement and chat that the server can verify. Inventory use, camera rotation, and local menu interactions are not visible to a server-authoritative mod and do not reset the timer. A player carried by a moving ship may appear active because their network position changes.

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

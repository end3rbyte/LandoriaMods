# ExpandedServer

Raises Valheim's server capacity to a configurable player limit.

## Features

- Increases server capacity to up to 100 players, with a default limit of 20.
- Updates admission checks and Steam or PlayFab advertised capacity.
- Supports dedicated servers and peer-hosted worlds.
- Limits configured values to a maximum of 100 players.

## Valheim.exe Command Switches

| Switch | Default | Purpose |
|---|---:|---|
| `--maxplayer <1-100>` | `20` | Sets the capacity on the dedicated server or hosting client. Invalid values use 20; values above 100 use 100. |

Joining clients need the mod but do not use the switch. PlayFab Party may impose a lower backend capacity than the configured value.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

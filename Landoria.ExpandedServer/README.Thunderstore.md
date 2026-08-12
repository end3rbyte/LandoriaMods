# ExpandedServer

Raises Valheim's server capacity to a configurable player limit.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Increases server capacity to up to 100 players, with a default limit of 20.
- Updates admission checks and advertised Steam or PlayFab capacity.
- Supports dedicated servers and peer-hosted worlds.
- Caps the configured limit at 100 players.

## Valheim.exe Command Switches

| Switch | Default | Purpose |
|---|---:|---|
| `--maxplayer <1-100>` | `20` | Sets the server or host capacity. Joining clients do not use it. |

- Every connecting player needs a compatible mod setup.
- Larger populations increase server, network, and simulation load.
- PlayFab Party may impose a lower backend capacity.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Read the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.ExpandedServer/README.md) on GitHub.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

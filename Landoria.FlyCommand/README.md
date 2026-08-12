# FlyCommand

Allows Valheim's native player flight behavior in authorized worlds without enabling debug mode, developer commands, or administrator access.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Authorization

- The client starts denied and explicitly requests authorization from the connected server.
- The server controls availability with `--flycommand true|false`; the default is `true`.
- The server authorizes flight only while both `NoBuildCost` and `PassiveMobs` are active.
- No response, an explicit denial, a modifier change, or a server change immediately disables flight.
- The `fly` command is hidden and invalid until the server explicitly authorizes it.
- The mod is required on the server and every participating client.

## Controls

| Control | Action |
|---|---|
| `fly`, `fly on`, `fly off` | Toggle, enable, or disable authorized flight. |
| `Z` | Toggle flight. |
| Movement keys | Fly horizontally using vanilla movement. |
| Jump / Space | Ascend. |
| Left Control | Descend. |
| Run / Shift | Fly faster. |

The `Z` shortcut is fixed to match Valheim's native debug-flight control.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes, wherever flight is authorized |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

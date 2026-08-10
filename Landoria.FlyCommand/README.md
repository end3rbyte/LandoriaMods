# FlyCommand

Allows Valheim's native player flight behavior in authorized worlds without enabling debug mode, developer commands, or administrator access.

## Authorization

- The client starts denied and explicitly requests authorization from the connected server.
- The server authorizes flight only while both `NoBuildCost` and `PassiveMobs` are active.
- No response, an explicit denial, a modifier change, or a server change immediately disables flight.
- The `fly` command is hidden and invalid until the server explicitly authorizes it.
- The mod is required on the server and every participating client.

## Controls

| Control | Action |
|---|---|
| `fly`, `fly on`, `fly off` | Toggle, enable, or disable authorized flight. |
| `F6` | Enable flight. |
| `F7` | Disable flight. |
| Movement keys | Fly horizontally using vanilla movement. |
| Jump / Space | Ascend. |
| Left Control | Descend. |
| Run / Shift | Fly faster. |

The enable and disable shortcuts can be changed in `BepInEx/config/Landoria.FlyCommand.cfg`.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes, wherever flight is authorized |

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

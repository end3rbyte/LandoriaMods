# HammerFreedom

Adds creative freedoms to authorized Hammer worlds without enabling debug mode,
developer commands, or administrator access.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Authorization

- The client starts denied and explicitly requests authorization from the connected server.
- The dedicated server controls flight with `--hammerfreedom-fly true|false`.
- The dedicated server controls fall damage immunity with `--hammerfreedom-fall-damage-immunity true|false`.
- The dedicated server controls unlimited stamina with `--hammerfreedom-unlimited-stamina true|false`.
- The dedicated server controls durability loss with `--hammerfreedom-no-durability-loss true|false`.
- The server controls build-material recovery with
  `--hammerfreedom-recover-build-materials true|false`.
- Each switch defaults to `true`.
- The server authorizes capabilities only while both `NoBuildCost` and `PassiveMobs` are active.
- No response, an explicit denial, a modifier change, or a server change immediately removes every capability.
- The `fly` command is hidden and invalid until the server explicitly authorizes it.
- The mod is required on the server and every participating client.

## Features

- Any fall deals zero damage when fall damage immunity is authorized.
- No action consumes stamina when unlimited stamina is authorized, including
  running, building, gardening, combat, jumping, swimming, and dodging.
- Tools, weapons, shields, armor, and other durable equipment do not lose durability when
  durability protection is authorized. Existing wear is preserved rather than repaired.
- Hammer dismantling returns the vanilla recoverable materials when build-material recovery
  is authorized, even while `NoBuildCost` or `NoCraftCost` is active. Other destruction does
  not receive this exception.

## Controls

| Control | Action |
|---|---|
| `fly`, `fly on`, `fly off` | Toggle, enable, or disable authorized flight. |
| `Z` | Toggle flight. |
| Movement keys | Fly horizontally at up to 4 metres per second. |
| Jump / Space | Ascend. |
| Left Control | Descend. |
| Run / Shift | Fly at up to 7 metres per second. |

The `Z` shortcut is fixed to match Valheim's native debug-flight control.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

# HammerFreedom

Adds flight, fall damage immunity, unlimited stamina, and durability protection to authorized
Hammer worlds without enabling debug mode, developer commands, or administrator access.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Requires an explicit authorization response from the connected server.
- Provides separate `--hammerfreedom-fly`, `--hammerfreedom-fall-damage-immunity`, and
  `--hammerfreedom-unlimited-stamina` server switches, plus
  `--hammerfreedom-no-durability-loss` and
  `--hammerfreedom-recover-build-materials`; each is enabled by default.
- Requires the Hammer world modifiers before the server grants any capability.
- Prevents all fall damage, regardless of fall height, when authorized.
- Prevents all stamina use when authorized, regardless of the action.
- Prevents durability loss for tools, weapons, shields, armor, and other durable equipment.
- Returns vanilla recoverable materials when a piece is dismantled with a hammer, even when
  building or crafting costs are disabled.
- Disables capabilities immediately when authorization is removed or the server changes.
- Hides the `fly` command until authorization is granted.
- Supports `fly`, `fly on`, `fly off`, and the fixed native `Z` toggle shortcut.
- Limits flight to 4 metres per second normally and 7 metres per second while sprinting.
- Keeps vanilla movement: Space ascends, Left Control descends, and Shift increases speed.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes, wherever HammerFreedom is authorized |

See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.HammerFreedom/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

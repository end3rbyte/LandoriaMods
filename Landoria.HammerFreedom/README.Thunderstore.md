# HammerFreedom

Adds flight, fall damage immunity, and unlimited stamina to authorized
Hammer worlds without enabling debug mode, developer commands, or administrator access.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Requires an explicit authorization response from the connected server.
- Provides separate `--hammerfreedom-fly`, `--hammerfreedom-fall-damage-immunity`, and
  `--hammerfreedom-unlimited-stamina` server switches, each enabled by default.
- Requires the Hammer world modifiers before the server grants any capability.
- Prevents all fall damage, regardless of fall height, when authorized.
- Prevents all stamina use when authorized, regardless of the action.
- Disables capabilities immediately when authorization is removed or the server changes.
- Hides the `fly` command until authorization is granted.
- Supports `fly`, `fly on`, `fly off`, and the fixed native `Z` toggle shortcut.
- Keeps vanilla movement: Space ascends, Left Control descends, and Shift increases speed.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes, wherever HammerFreedom is authorized |

See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.HammerFreedom/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

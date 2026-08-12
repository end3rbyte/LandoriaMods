# FlyCommand

Uses Valheim's native flight movement in server-authorized worlds without enabling debug mode, developer commands, or administrator access.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Requires an explicit authorization response from the connected server.
- Supports the server command-line switch `--flycommand true|false`, enabled by default.
- Disables flight immediately when authorization is removed or the server changes.
- Hides the `fly` command until authorization is granted.
- Supports `fly`, `fly on`, `fly off`, and the fixed native `Z` toggle shortcut.
- Keeps vanilla movement: Space ascends, Left Control descends, and Shift increases speed.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes, wherever flight is authorized |

See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.FlyCommand/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

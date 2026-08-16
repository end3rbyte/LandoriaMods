# QuickLaunch

Automatically resumes the last local or multiplayer Valheim session.

## Video demo

[Watch QuickLaunch in action on YouTube](https://youtu.be/5MOIaP7fJps).

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Selects the remembered character automatically.
- Reopens the remembered world for local sessions.
- Reconnects to the first valid recent server for multiplayer sessions.
- Falls back to the main menu when required data is unavailable.
- Never stores or enters server passwords.
- Remembers whether the last manually started session was local or multiplayer.

## Valheim.exe Command Switches

| Switch | Default | Purpose |
|---|---:|---|
| `--quicklaunch <true or false>` | `true` | Enables automatic resume. Use `--quicklaunch false` to stop at the menu. |

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | No | Not Supported |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

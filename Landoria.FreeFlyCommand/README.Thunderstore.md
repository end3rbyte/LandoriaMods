# FreeFlyCommand

Allows Valheim's native `freefly` and `ffsmooth` commands only after explicit server authorization, without enabling `devcommands` or `debugmode`.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Starts denied until the connected server authorizes the client.
- Enables native smoothing automatically when free camera mode starts.
- Keeps the native `ffsmooth` command available while authorized.
- Documents Valheim's complementary vanilla `fov <degrees>` command for adjusting the free-camera field of view.
- Limits the camera to 50 metres from the player.
- Disables free camera immediately when authorization is removed.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.FreeFlyCommand/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

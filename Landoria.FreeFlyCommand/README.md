# FreeFlyCommand

Allows Valheim's native `freefly` and `ffsmooth` camera commands after explicit authorization from the connected server. It does not enable `devcommands` or `debugmode`.

## Video demo

[Watch FreeFlyCommand in action on YouTube](https://www.youtube.com/watch?v=nDNg8NBXQHg).

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Authorization

- The client starts denied and requests authorization once after the local player spawns.
- The dedicated server controls authorization with `--freeflycommand true|false`; the default is `true`.
- No response, an explicit denial, or a server change immediately disables free camera mode.
- The mod is required on the server and every participating client.

## Camera behavior

- `freefly` toggles Valheim's native free camera.
- Enabling free camera also applies native smoothing equivalent to `ffsmooth 1` when smoothing is disabled.
- `ffsmooth <0-1>` remains available while authorized.
- Valheim's vanilla `fov <degrees>` command can adjust the field of view while using `freefly`; `fov` by itself reports the current value.
- Free-camera movement is limited to 20 metres per second.
- A one-metre-radius free-camera collision sphere blocks terrain, mountains, solid objects, and player-built pieces.
- The free camera is clamped to 50 metres from the local player.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

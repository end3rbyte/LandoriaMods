# DecayControl

DecayControl controls rain wear and fuel consumption on player-built pieces.

## Features

- Independently controls fuel consumption and environmental wear for player-built pieces.
- Can preserve vanilla behavior, apply an effect only while the creator is connected, or
  disable the effect entirely.
- In `player-online` mode, considers only the creator's connection state; group membership
  has no effect.
- Preserves vanilla behavior for natural pieces and pieces without an identifiable creator.
- Prevents paused fuel consumption from catching up retroactively after loading.

## Dedicated server settings

Settings are read once from dedicated-server command-line switches and sent to clients
after their local player spawns. They are not BepInEx settings.

| Switch | Values | Default |
|---|---|---|
| `--decay-control-fuel-consumption` | `default`, `player-online`, `disabled` | `default` |
| `--decay-control-environmental-building-wear` | `default`, `player-online`, `disabled` | `default` |

`default` preserves vanilla behavior. `player-online` applies the effect only while the
piece creator is connected. `disabled` stops the effect for player-built pieces.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

Install matching versions of DecayControl on the server and every participating client.
DecayControl has no dependency on Socialize.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use
[GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

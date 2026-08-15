# DecayControl

Controls rain wear and fuel consumption for player-built pieces.

## Features

- Independently controls fuel consumption and environmental wear for player-built pieces.
- Can preserve vanilla behavior, apply an effect only while the creator is connected, or
  disable the effect entirely.
- In `player-online` mode, considers only the creator's connection state; group membership
  has no effect.

## Dedicated server settings

| Switch | Values | Default |
|---|---|---|
| `--decay-control-fuel-consumption` | `default`, `player-online`, `disabled` | `default` |
| `--decay-control-environmental-building-wear` | `default`, `player-online`, `disabled` | `default` |

Settings are read once by the dedicated server and sent to clients after player spawn.
They are not BepInEx settings. `default` is vanilla, `player-online` requires the creator
to be connected, and `disabled` stops the effect for player-built pieces.

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

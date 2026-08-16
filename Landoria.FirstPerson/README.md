# First Person

Experience Valheim from your character's point of view with a stable, configurable
first-person camera at the closest mouse-wheel zoom level.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Places the camera at eye level and follows the complete look direction.
- Hides the local player's body and equipped items to keep the view unobstructed.
- Restores the complete character when returning to third person or free fly.
- Provides one persistent FOV setting for first person, third person, and free fly.
- Does not hide the character from any other player.
- Requires no server installation or configuration.

## Controls

| Action | Control |
|---|---|
| Enter first person | Zoom all the way in with the mouse wheel |
| Return to third person | Scroll away from the character |
| Enable or disable first person | `firstperson` |
| Set the shared FOV | `fov <degrees>` |
| Show the current FOV | `fov` |
| Restore the default FOV of 65 | `fov reset` |

First person is enabled by default. The toggle and FOV are saved locally in the
mod's BepInEx configuration and apply to every character on that installation.
The FOV is capped at 100.

## Camera behavior

The camera stays exactly at the animated eye point and follows the full look
direction, including when looking up or down. The character's head and attached
equipment follow the same direction without the vanilla look clamp. The local
player and equipped items are hidden only from the local first-person view.

## Installation

| Client required | Server required |
|---|---|
| Yes | No |

Install First Person only on clients that want to use it.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

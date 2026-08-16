# First Person

Experience Valheim from your character's point of view with a stable, configurable
first-person camera at the closest mouse-wheel zoom level.

## Video demo

[Watch First Person in action on YouTube](https://www.youtube.com/watch?v=Tzb9Hi6qYv4).

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Places the camera at eye level and follows the complete look direction.
- Hides the local player's body while keeping items held in either hand visible.
- Stabilizes helmet lights against movement and animation flicker in first person.
- Restores the complete character when returning to third person or free fly.
- Saves one FOV setting and adds 15 degrees silently while first person is active.
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
The configured FOV is capped at 85. First person adds 15 degrees without
changing the saved value, for an effective maximum of 100.

## Camera behavior

The camera stays exactly at the animated eye point and follows the full look
direction, including when looking up or down. The local player's body is hidden
only from the local first-person view, while held items remain visible.

## Installation

| Client required | Server required |
|---|---|
| Yes | No |

Install First Person only on clients that want to use it.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

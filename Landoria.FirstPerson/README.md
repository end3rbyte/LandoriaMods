# First Person

Adds a fixed camera above the local player's head at the closest mouse-wheel zoom level.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Fixes the first-person camera above the local player's head.
- Turns the complete player body with horizontal camera movement.
- Keeps vertical camera movement independent from body rotation.
- Keeps the local player body visible in first person.
- Restores the complete local character when zooming out or entering free-fly mode.
- Keeps Valheim's `fov <degrees>` setting active in first-person and third-person gameplay.
- Does not hide the character from any other player.
- Requires no server installation or configuration.

Use the mouse wheel to zoom all the way in. Scroll away from the character to return to third person.

First person is enabled by default. Run `firstperson` to toggle the feature. Disabling it immediately restores vanilla camera behavior. The choice is saved locally in the mod's BepInEx configuration and applies to every character on that installation.

In first person, the camera stays 10 cm below and 10 cm ahead of the eye point, near chin level, and follows the full look direction, including when looking up or down. The character's head and attached equipment follow the same direction without the vanilla look clamp. The player body remains visible, including the arms, despite the vanilla close-camera visibility rule.

Run `fov <degrees>` to change the field of view shared by first person, third person, and free fly, up to a maximum of 100. Run `fov reset` to restore the default value of 65, or `fov` without a value to report the current field of view. The selected value is saved in the same local configuration.

## Installation

| Client required | Server required |
|---|---|
| Yes | No |

Install First Person only on clients that want to use it.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

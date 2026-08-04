# GentleDeath

Keeps equipable items after death while moving other inventory items to the tombstone.

## Features

- Keeps weapons, tools, armor, shields, ammunition, utility items, and trinkets.
- Preserves the equipped state of retained items.
- Moves non-equipable treasures and materials to the tombstone.
- Keeps an item safely when the tombstone has no available space.
- Replaces the world's configured vanilla inventory death penalty.

Equipable items follow Valheim's `ItemData.IsEquipable()` classification.

## Server-controlled activation

| Setting | Default | Purpose |
|---|---:|---|
| `General.Enabled` | Dedicated: `true`; otherwise: `false` | Enables the custom death inventory rules on the server and connected clients. |

Set the value to `true` in the server configuration and restart the server.

## Installation

| Client required | Server required |
|---|---|
| Yes | No |

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

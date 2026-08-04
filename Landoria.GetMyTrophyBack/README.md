# GetMyTrophyBack

Returns a boss trophy after its Sacrificial Stone power is selected.

## Features

- Starts a five-second return timer when a player selects a guardian power.
- Drops the mounted trophy as a recoverable world item.
- Preserves the trophy's stored item data.
- Uses the stone's network owner for an authoritative synchronized drop.
- Prevents simultaneous requests from duplicating a trophy.
- Supports modded guardian stones built on vanilla `ItemStand` powers.

## Behavior

| Event | Result |
|---|---|
| Trophy mounted | Its guardian power remains available. |
| Power selected | The five-second return timer starts. |
| Timer completed | The trophy drops into the world. |
| Trophy already absent | No additional trophy is created. |

## Server-controlled activation

| Setting | Default | Purpose |
|---|---:|---|
| `General.Enabled` | Dedicated: `true`; otherwise: `false` | Enables trophy returns on the server and connected clients. |

Set the value to `true` in the server configuration and restart the server.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

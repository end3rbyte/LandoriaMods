# GetMyTrophyBack

Returns a boss trophy after its Sacrificial Stone power is selected.

## Features

- Starts a five-second timer after selecting a guardian power.
- Drops the mounted trophy as a recoverable world item.
- Keeps the selected power active.
- Preserves stored trophy data.
- Prevents duplicate drops from simultaneous requests.

Install the same version everywhere so the peer owning the stone can perform the synchronized drop.

## Server-controlled activation

| Setting | Default |
|---|---:|
| `General.Enabled` | Dedicated: `true`; otherwise: `false` |

The server synchronizes this value to connected clients after restart.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Read the [full documentation](https://github.com/end3rbyte/LandoriaMods/blob/main/Landoria.GetMyTrophyBack/README.md) on GitHub.

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

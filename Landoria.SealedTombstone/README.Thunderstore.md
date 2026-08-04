# SealedTombstone

Protects recent tombstones from unauthorized players while allowing owner-approved access.

## Features

- Locks recent tombstones to their owners.
- Uses a vanilla Yes/No popup for access requests.
- Permanently unlocks a tombstone after approval.
- Expires requests after 30 seconds and applies a two-minute cooldown.
- Makes tombstones public after ten in-game days.
- Blocks recent attackers from requesting access permanently.

## Access Rules

| Situation | Result |
|---|---|
| Owner approves | Tombstone permanently unlocks. |
| Owner denies or request expires | Tombstone remains locked. |
| Tombstone reaches ten days | Tombstone becomes public. |
| Requester attacked the owner before death | The request is blocked. |

## Server-controlled activation

| Setting | Default |
|---|---:|
| `General.Enabled` | Dedicated: `true`; otherwise: `false` |

The server synchronizes this value to connected clients after restart.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Install matching versions on the server and every participating client.

Read the [full documentation](https://github.com/end3rbyte/LandoriaMods/blob/main/Landoria.SealedTombstone/README.md) on GitHub.

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

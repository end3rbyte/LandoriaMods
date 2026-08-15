# SealedTombstone

Protects recent tombstones from unauthorized players while allowing owner-approved access.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Locks each recent tombstone to its owner.
- Sends the online owner a vanilla Yes/No access request.
- Permanently unlocks the tombstone after owner approval.
- Expires unanswered requests after 30 seconds.
- Applies a two-minute request cooldown.
- Makes tombstones public after ten in-game days.
- Permanently blocks players who hurt or killed the owner during the two minutes before death.

## Access Rules

| Situation | Result |
|---|---|
| Owner selects Yes | The tombstone permanently unlocks. |
| Owner selects No | Access remains denied. |
| Owner is offline | The requester is informed immediately and no cooldown starts. |
| Request times out | Access remains denied. |
| Tombstone reaches ten days | It becomes publicly accessible. |
| Requester is on the deny list | No request is sent to the owner. |

Tombstones created before a lock day was recorded remain protected until their owner approves access.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

Every participating client and the server must install matching versions.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

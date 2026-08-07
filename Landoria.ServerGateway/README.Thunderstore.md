# ServerGateway

Exposes live Valheim server information and authenticated commands through a local HTTP gateway.

## Features

- Reports server and world names, connected players and player capacity.
- Reports the current in-game day.
- Queues immediate vanilla world saves through an authenticated local command.
- Listens only on `127.0.0.1` for an on-machine website backend.
- Supports dedicated servers and peer-hosted worlds.

## Installation

| Client required | Server required |
|---|---|
| No | Yes |

## Configuration

| BepInEx setting | Default | Purpose |
|---|---:|---|
| `RPC.Port` | `8765` | Local port for `GET /status`. |
| `Gateway.Token` | Empty | Bearer token required by every endpoint. |

The JSON response contains server and world information plus `players` and `playerDetails`. Each player detail includes the character name, canonical platform user ID, and Steam ID when applicable.

## Commands

| Method | Path | Authentication | Behavior |
|---|---|---|---|
| `GET` | `/status` | Bearer token | Returns the current server and player snapshot. |
| `POST` | `/commands/save` | Bearer token | Queues an immediate vanilla world and player-profile save. |

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

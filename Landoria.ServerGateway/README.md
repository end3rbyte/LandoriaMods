# ServerGateway

Exposes live Valheim server information and authenticated commands through a local HTTP gateway.

## Features

- Reports the server and world names, player list and current player count to authenticated callers.
- Reports the active maximum player count and current in-game day.
- Queues an immediate vanilla world save from an authenticated local request.
- Listens only on `127.0.0.1` so an on-machine website backend can query it safely.
- Returns `worldCreatedAt` as `null` because Valheim does not store a creation date in its world save.
- Supports dedicated servers and peer-hosted worlds.

## Installation

| Client required | Server required |
|---|---|
| No | Yes |

## Configuration

| BepInEx setting | Default | Purpose |
|---|---:|---|
| `RPC.Port` | `8765` | Local TCP port for the HTTP endpoint. |
| `Gateway.Token` | Empty | Bearer token required by every endpoint. |

Restart the Valheim server after changing the port.
The `LANDORIA_SERVER_GATEWAY_TOKEN` environment variable overrides `Gateway.Token` and is recommended for dedicated servers. Every endpoint remains unavailable when no token is configured.

## Status endpoint

Request the current snapshot from the same machine as Valheim:

```http
GET http://127.0.0.1:8765/status
Authorization: Bearer <gateway-token>
```

Example response:

```json
{
  "ready": true,
  "serverName": "My server",
  "worldName": "Dedicated",
  "playerCount": 2,
  "maxPlayers": 20,
  "day": 148,
  "worldCreatedAt": null,
  "players": ["Astrid", "Eirik"],
  "playerDetails": [
    {
      "name": "Astrid",
      "platformUserId": "Steam_76561198000000001",
      "steamId": "76561198000000001"
    }
  ]
}
```

| Field | Type | Meaning |
|---|---|---|
| `ready` | Boolean | Whether a server world is currently available. |
| `serverName` | String or null | Value supplied through Valheim's `-name` argument. |
| `worldName` | String | Loaded world name. |
| `playerCount` | Number | Number of connected players. |
| `maxPlayers` | Number | Vanilla, ExpandedServer default, or `--maxplayer` capacity. |
| `day` | Number | Current in-game day. |
| `worldCreatedAt` | String or null | Always null until Valheim stores this value in the world save. |
| `players` | String array | Connected character names. |
| `playerDetails` | Object array | Character names, canonical platform user IDs, and Steam IDs when applicable. |

The endpoint is intentionally unavailable from other machines. Expose it through the website backend, with the backend's own authentication and HTTPS controls.

## Commands

The gateway accepts the save request immediately, then executes the vanilla `ZNet.instance.SaveWorldAndPlayerProfiles()` method on the Unity thread:

| Method | Path | Authentication | Behavior |
|---|---|---|---|
| `GET` | `/status` | Bearer token | Returns the current server and player snapshot. |
| `POST` | `/commands/save` | Bearer token | Queues an immediate vanilla world and player-profile save. |

```http
POST http://127.0.0.1:8765/commands/save
Authorization: Bearer <gateway-token>
```

Successful requests return `202 Accepted`:

```json
{"accepted":true,"command":"save"}
```

Only one save command can be queued at a time. A duplicate pending request returns `409 Conflict`.

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

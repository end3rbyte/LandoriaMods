# CharacterVault

CharacterVault stores authoritative Valheim character profiles on the server. A local backup cannot replace an existing server profile because the server profile is applied before the player enters the world.

## Guarantees

- Requires an exact, ModSentry-verified CharacterVault DLL on every client.
- Creates no character data until ModSentry and the Valheim peer handshake accept the client.
- Commits a new character only after its first complete profile is validated and written durably.
- Saves connected characters on world saves, disconnects, and graceful server shutdowns.
- Keeps the existing `character_vault.drp` graceful shutdown protocol.
- Uses bounded fragmented transfers, SHA-256 validation, atomic replacement, and a previous revision.

## Configuration

```ini
[Characters]
AllowMultipleCharactersPerSteamId = true

[New Characters]
StartingItems =
```

Starting items use comma-separated prefab and quantity pairs:

```ini
StartingItems = hammer:1,wood:10,stone:10
```

Command-line switches override BepInEx settings:

```text
--charactervault-allow-multiple-characters true
--charactervault-starting-items hammer:1,wood:10,stone:10
```

The server configuration is authoritative. Starting items are granted exactly once during initial enrollment.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Install matching CharacterVault and ModSentry versions on every client and server.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

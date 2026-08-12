# CharacterVault

CharacterVault stores authoritative Valheim character profiles on the server. A local backup cannot replace an existing server profile because the server profile is applied before the player enters the world.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Guarantees

- Requires an exact, ModSentry-verified CharacterVault DLL on every client.
- Creates no character data until ModSentry and the Valheim peer handshake accept the client.
- Commits a new character only after its first complete profile is validated and written durably.
- Saves connected characters on world saves, disconnects, and graceful server shutdowns.
- Keeps the existing `character_vault.drp` graceful shutdown protocol.
- Uses bounded fragmented transfers, SHA-256 validation, atomic replacement, and a previous revision.
- Supports the stable and public test Valheim save APIs for local and Steam Cloud profiles.

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

## Valheim compatibility

CharacterVault supports both the stable and public test save APIs. Valheim
changes the signatures of its public save helpers between these releases, so a
narrow runtime adapter selects only the available file writer, atomic replace,
character path, and cache invalidation signatures. This preserves local and
Steam Cloud behavior without inspecting private game state.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

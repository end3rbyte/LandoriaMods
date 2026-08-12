# Changelog

## Unreleased

- List every character-save trigger in the Thunderstore player documentation.
- Clarify player behavior, save guarantees, configuration, and graceful shutdown documentation.
- Route the in-game Quit action through the confirmed voluntary-disconnect save flow.
- Acknowledge validated voluntary-disconnect uploads immediately, then complete the durable server commit asynchronously.
- Require a server-confirmed final character save before voluntary logout or application quit.
- Add detailed client logs for voluntary disconnect saves and acknowledgements.
- Intercept server kicks without requiring other mods to depend on CharacterVault.
- Require a confirmed final character save before every server-side kick.
- Log CharacterVault profile saves
- Refine CharacterVault Thunderstore wording
- Simplify CharacterVault Thunderstore README
- Restore CharacterVault server configuration
- Document CharacterVault features (#102)
- Use transparent CharacterVault icon (#101)
- Revert "Clean CharacterVault icon contour (#99)" (#100)
- Clean CharacterVault icon contour (#99)

## 1.0.7

- Resolve the local save source by name across stable and public test enum layouts.
- Document Valheim version compatibility
- Support Valheim save API variants
- Fix CharacterVault save API compatibility
- Fix Valheim public test compatibility

## 1.0.6

- Update thunderstore readme

## 1.0.5

- Enforce CharacterVault rejection before Valheim can admit or spawn the peer.
- Require an unregistered character to have been created during the current game session.

## 1.0.4

- Replace ServerCharacters with an independently implemented authoritative server vault.
- Admit new characters only after ModSentry validation and the first durable profile commit.
- Save connected characters whenever Valheim starts a world save.
- Add server-authoritative starting items and command-line configuration overrides.
- Preserve the existing graceful dedicated-server shutdown trigger.

## 1.0.3

- Log when the mod is unloaded.

## 1.0.2

- Rebuild the package as an immutable artifact shared by test and production.

## 1.0.1

- Update the package icon.

## 1.0.0

- Initial version.

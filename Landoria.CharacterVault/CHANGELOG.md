# Changelog

## 1.0.11

- Limit retained character backups and log every retention deletion.

## 1.0.10

- Bug fix

## 1.0.9

- Display the server's admission rejection reason and return rejected players to the main menu instead of leaving them on a black screen.

## 1.0.8

- Updated documentation

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

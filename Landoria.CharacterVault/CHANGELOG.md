# Changelog

## Unreleased

- Fix CharacterVault save API compatibility [skip draft publish]
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

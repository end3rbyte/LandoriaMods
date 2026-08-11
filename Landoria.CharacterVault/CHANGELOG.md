# Changelog

## Unreleased

- Store authoritative character profiles directly in CharacterVault without ServerCharacters.
- Admit new characters only after ModSentry validation and the first durable profile commit.
- Save connected characters whenever Valheim starts a world save.
- Configure server-authoritative starting items through BepInEx or command-line arguments.
- Preserve the existing graceful dedicated-server shutdown trigger.

## 1.0.3

- Log when the mod is unloaded.

## 1.0.2

- Rebuild the package as an immutable artifact shared by test and production.

## 1.0.1

- Update the package icon.

## 1.0.0

- Initial Version

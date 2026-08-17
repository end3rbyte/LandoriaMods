# Changelog

## 1.0.9

- Optionally admit clients without ModSentry for 30 seconds, show a configurable registration message, and then disconnect them.

## 1.0.8

- Accept non-BepInEx DLLs when explicitly listed in optional or required policy entries.

## 1.0.7

- Emphasize identical mod setups and playing conditions for every player.

## 1.0.6

- Add unit tests.

## 1.0.5

- Disconnect rejected pre-admission connections directly so clients return to the main menu instead of remaining on a black screen.

## 1.0.4

- Return rejected clients to the main menu and display the server's rejection reason.

## 1.0.3

- Document Valheim version compatibility

## 1.0.2

- Update thunderstore readme

## 1.0.1

- Shorten ModSentry rejection messages.
- Deliver ModSentry rejection details reliably.

## 1.0.0

- Initial version.
- Read plugin metadata from the plugin class when validating server policy files.
- Confirm delivery of rejection details before disconnecting incompatible clients.
- Explain which mod failed validation and which version the server expects.

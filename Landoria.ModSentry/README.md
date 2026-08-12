# Landoria ModSentry

ModSentry validates the complete client plugin inventory before the Valheim peer handshake is accepted.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

The server defines two explicit directories under `BepInEx/config`:

- `ModSentry_Required`: every DLL must be installed by the client.
- `ModSentry_Optional`: each DLL may be absent, but must match exactly when installed.

Every other client plugin is rejected. Matching uses the BepInEx GUID, complete plugin version, and exact DLL SHA-256. The client confirms receipt of a specific rejection reason before the server disconnects it, with a short fallback timeout for incompatible clients that cannot acknowledge the message. A rejected client returns to the main menu, where Valheim displays the reason instead of remaining in the loading scene. Player messages identify the affected mod and expected version; client and server logs retain the diagnostic.

ModSentry was independently designed and implemented for Landoria. It does not contain AzuAntiCheat code.

## Contact

Report issues through the Landoria website.

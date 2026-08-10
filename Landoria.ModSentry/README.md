# Landoria ModSentry

ModSentry validates the complete client plugin inventory before the Valheim peer handshake is accepted.

The server defines two explicit directories under `BepInEx/config`:

- `ModSentry_Required`: every DLL must be installed by the client.
- `ModSentry_Optional`: each DLL may be absent, but must match exactly when installed.

Every other client plugin is rejected. Matching uses the BepInEx GUID, complete plugin version, and exact DLL SHA-256. Player messages are concise and non-technical; server logs retain the complete diagnostic.

ModSentry was independently designed and implemented for Landoria. It does not contain AzuAntiCheat code and performs no gameplay cheat detection.

## Contact

Report issues through the Landoria website.

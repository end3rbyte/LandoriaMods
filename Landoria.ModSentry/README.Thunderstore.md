# Landoria ModSentry

Strictly validates required and optional client mods before a Valheim server accepts the connection.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

- BepInEx GUID validation
- complete version validation
- mandatory SHA-256 validation
- clear mismatch messages at the main menu with the affected mod and expected version
- complete server-side diagnostics

All other client mods are rejected. If a mod does not match, Valheim returns to the main menu and explains what must be updated instead of remaining on a black loading screen.

[Read the complete documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.ModSentry/README.md).

## Contact

Report issues through the Landoria website.

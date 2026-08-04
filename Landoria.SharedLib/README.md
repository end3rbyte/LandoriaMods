# Plugin SharedLib

`Landoria.SharedLib` contains the common runtime infrastructure used by every Landoria plugin.

| Component | Purpose |
|---|---|
| `LandoriaPlugin` | Initializes and removes Harmony patches belonging to the concrete plugin namespace. |
| `ModLog` | Routes plugin diagnostics through the BepInEx logger and the debugger output. |
| `ServerFeaturePolicy` | Synchronizes a typed, reflection-free enabled state and enforces matching versions when enabled. |
| `ILRepack.targets` | Embeds this library into each standalone plugin DLL. |

The library is a build-time project dependency. Players and server operators do not install a separate `Landoria.SharedLib.dll`; every player-facing Landoria DLL contains the required code through ILRepack.

An enabled server feature requires an exact client/server plugin version match. A mismatch is logged with the plugin identifier and both versions before the peer is disconnected. Disabled features do not enforce version equality.

`General.Enabled` defaults to `true` in dedicated server batch mode and `false` otherwise. An existing configuration value always takes precedence over this generated default.

## Development

All plugin projects receive the project reference and ILRepack package through the repository-level `Directory.Build.props`. Each standalone plugin embeds its own copy of SharedLib.

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues). For other conversations, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

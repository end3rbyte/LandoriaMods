# Landoria Mods

This repository contains independently built Valheim plugins. See each
plugin's `README.md` for technical details and `README.Thunderstore.md` for a
concise player-oriented overview.

| Mod | Installation side | Description |
|---|---|---|
| [GentleDeath](Landoria.GentleDeath/) | Client; server when required by a game mode | Keeps equipable gear on the player after death and moves other items to the tombstone. |
| [GetMyTrophyBack](Landoria.GetMyTrophyBack/) | Both | Drops a mounted boss trophy five seconds after its guardian power is selected. |
| [Moderator](Landoria.Moderator/) | Both | Adds multiplayer moderation commands gated by server-validated administrator access. |
| [QuickLaunch](Landoria.QuickLaunch/) | Client-only | Automatically resumes the last local or multiplayer session by default. |
| [ExpandedServer](Landoria.ExpandedServer/) | Both | Raises the server player limit. |
| [NoServerPassword](Landoria.NoServerPassword/) | Server-only | Allows public and crossplay servers to start without a password. |
| [SealedTombstone](Landoria.SealedTombstone/) | Both | Protects tombstones and lets their owners approve access. |

## Shared library

Every plugin references `Landoria.SharedLib`, which provides the common plugin
base, Harmony registration, and logging. It is embedded into every standalone
plugin DLL and is never installed separately.

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

## Automated test packages

Changes merged into `main` automatically build and publish a new draft version
of each affected mod to Landoria's package repository. The workflow can also be
started manually to publish selected mods or retry an existing version after a
failed upload.

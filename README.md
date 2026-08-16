# Landoria Mods

This repository contains the public source code for Landoria's Valheim mods.
Each mod directory includes a technical `README.md`, a player-oriented
`README.Thunderstore.md`, and its changelog.

## Server-driven configuration

Multiplayer rules are controlled by the server, so players never need to copy or
maintain a separate server configuration on their clients. When a client needs a
setting to behave consistently, the mod requests the effective value from the
server after connecting. For example, HammerFreedom asks the server which creative
capabilities are enabled before allowing Fly, while AfkDetector applies the
server's inactivity timeout without any client-side setting. Preferences that
affect only the local player, such as camera options, remain local.

| Mod | Installation side | Description |
|---|---|---|
| [AfkDetector](Landoria.AfkDetector/) | Both | Disconnects players who remain motionless and silent, with a clear inactivity message. |
| [CharacterVault](Landoria.CharacterVault/) | Both | Stores authoritative server profiles and checkpoints characters with world saves. |
| [GentleDeath](Landoria.GentleDeath/) | Client; server when required by a game mode | Keeps equipable gear on the player after death and moves other items to the tombstone. |
| [GetMyTrophyBack](Landoria.GetMyTrophyBack/) | Both | Drops a mounted boss trophy five seconds after its guardian power is selected. |
| [HammerFreedom](Landoria.HammerFreedom/) | Both | Adds server-authorized creative freedoms to Hammer worlds. |
| [FreeFlyCommand](Landoria.FreeFlyCommand/) | Both | Allows server-authorized native free-camera commands within 50 metres of the player. |
| [First Person](Landoria.FirstPerson/) | Client-only | Adds first-person view at the closest camera zoom level. |
| [LandoriaModPack](Landoria.LandoriaModPack/) | Both | Installs the mods required to join the Landoria Valheim server. |
| [ModSentry](Landoria.ModSentry/) | Both | Validates the exact client mod inventory before a server accepts a connection. |
| [Moderator](Landoria.Moderator/) | Both | Adds multiplayer moderation commands gated by server-validated administrator access. |
| [QuickLaunch](Landoria.QuickLaunch/) | Client-only | Automatically resumes the last local or multiplayer session by default. |
| [ExpandedServer](Landoria.ExpandedServer/) | Both | Raises the server player limit. |
| [NoServerPassword](Landoria.NoServerPassword/) | Server-only | Allows public and crossplay servers to start without a password. |
| [Structure Protection](Landoria.StructureProtection/) | Both | Protects structures while their authorized players are offline. |
| [SealedTombstone](Landoria.SealedTombstone/) | Both | Protects tombstones and lets their owners approve access. |
| [ServerGateway](Landoria.ServerGateway/) | Server | Exposes authenticated local status and save endpoints. |
| [Socialize](Landoria.Socialize/) | Both | Adds temporary groups for missions and expeditions, private messaging, map sharing, and expanded chat channels. |

## Shared library

`Landoria.SharedLib` provides common plugin infrastructure, including the
plugin base, Harmony registration, and logging. It is an internal component and
is never installed as a standalone mod.

## Discover the mods

- [Watch Landoria mod demos on YouTube](https://www.youtube.com/channel/UC7JKJ6QyDyFbWrgQGg8k5jQ).
- [Browse all Landoria mods on Thunderstore](https://thunderstore.io/c/valheim/p/Landoria/).

## Community server

All Landoria mods can be tested on the [Landoria Valheim community server](https://valheim.landoria-gaming.com/).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

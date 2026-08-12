# Landoria Mods

`Landoria.ModSentry` validates the exact client mod inventory before a server accepts a connection.

This repository contains independently built Valheim plugins. See each
plugin's `README.md` for technical details and `README.Thunderstore.md` for a
concise player-oriented overview.

| Mod | Installation side | Description |
|---|---|---|
| [AfkDetector](Landoria.AfkDetector/) | Both | Disconnects players who remain motionless and silent, with a clear inactivity message. |
| [CharacterVault](Landoria.CharacterVault/) | Both | Stores authoritative server profiles and checkpoints characters with world saves. |
| [GentleDeath](Landoria.GentleDeath/) | Client; server when required by a game mode | Keeps equipable gear on the player after death and moves other items to the tombstone. |
| [GetMyTrophyBack](Landoria.GetMyTrophyBack/) | Both | Drops a mounted boss trophy five seconds after its guardian power is selected. |
| [FlyCommand](Landoria.FlyCommand/) | Both | Allows server-authorized vanilla flight in configured worlds. |
| [FreeFlyCommand](Landoria.FreeFlyCommand/) | Both | Allows server-authorized native free-camera commands within 50 metres of the player. |
| [LandoriaModPack](Landoria.LandoriaModPack/) | Both | Installs the mods required to join the Landoria Valheim server. |
| [Moderator](Landoria.Moderator/) | Both | Adds multiplayer moderation commands gated by server-validated administrator access. |
| [QuickLaunch](Landoria.QuickLaunch/) | Client-only | Automatically resumes the last local or multiplayer session by default. |
| [ExpandedServer](Landoria.ExpandedServer/) | Both | Raises the server player limit. |
| [NoServerPassword](Landoria.NoServerPassword/) | Server-only | Allows public and crossplay servers to start without a password. |
| [SealedTombstone](Landoria.SealedTombstone/) | Both | Protects tombstones and lets their owners approve access. |
| [ServerGateway](Landoria.ServerGateway/) | Server | Exposes authenticated local status and save endpoints. |
| [Socialize](Landoria.Socialize/) | Both | Adds persistent groups, private messaging, map sharing, and expanded chat channels. |

## Shared library

Every plugin references `Landoria.SharedLib`, which provides the common plugin
base, Harmony registration, and logging. It is embedded into every standalone
plugin DLL and is never installed separately.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

## Automated test packages

Changes merged into `main` automatically build and publish a new draft version
of each affected mod to Landoria's package repository. The workflow can also be
started manually to publish selected mods or retry an existing version after a
failed upload. When a tracked dependency changes version, the workflow updates
and republishes `LandoriaModPack` with the matching dependency version.
Until a changelog section matching the draft version exists, each build creates
or refreshes `Unreleased` from the mod's commit history. Once a matching version
section is present, builds leave the changelog unchanged.

Test storage reconciliation renders optional `items` from the central game-mode
configuration as `Landoria.CharacterVault.cfg` under each applicable mode's `mods/config`
path. Common items are inherited by mode-specific configurations, and obsolete managed
configurations are removed. Production promotion copies the exact tested files without
regenerating them.

Production publication is handled by the manual **Promote mods to Thunderstore**
workflow on the self-hosted `dev` runner. It compares each selected mod with its
latest tagged Thunderstore release, skips unchanged package inputs (including
the shared library and packaging files), increments the patch version from the
version currently published on Thunderstore, and includes the selected mod's
existing `CHANGELOG.md` unchanged in the package. Changelog entries must be
prepared and reviewed before starting the promotion workflow. Promotion fails
if `Unreleased` is still present or if the changelog has no section matching the
version being released.
After Thunderstore exposes the new version, the workflow marks the matching
package version as released in Landoria's package repository and creates a
`thunderstore/<mod>/<version>` Git tag.

The manual **Revert unused drafts** workflow removes an accidental draft only
through a reviewed pull request. It first compares the draft source with the
last release tag and refuses any change outside generated version metadata and
ModPack dependency versions. Merging the generated pull request restores the
released source versions, deletes only matching unreleased private packages,
and reconciles test storage. Released packages and versions already present on
Thunderstore can never be deleted by this workflow.

Release and deployment automation is maintained privately. The public
repository contains only a minimal event relay; operational workflows, scripts,
and credential provisioning are not part of the public source tree.

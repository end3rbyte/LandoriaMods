# CharacterVault

CharacterVault stores authoritative Valheim character profiles on the server.
For a voluntary logout or application quit, the client waits for the server to
commit one final profile save before allowing the action to continue. Client
crashes and network failures cannot use this handshake.
Client and server logs include the same save request identifier and committed
revision so each profile write can be correlated across both sides.
Once a character is enrolled, the server copy is applied before the player
enters the world. This prevents players from bringing items from other servers
or duplicating items by restoring a local backup.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- **Authoritative server profiles:** The server stores and restores enrolled
  characters. Modified or restored local copies cannot replace the server
  profile.
- **New-character enrollment:** A character without an existing server profile
  is accepted only when it was created during the current game session. This
  prevents an existing character from another server from being enrolled.
- **Configurable character limit:** A server can allow one character per Steam
  ID or multiple characters per Steam ID.
- **Configurable starting items:** The server can grant configured items and
  quantities once, when a new character is first enrolled.
- **World-synchronized saves:** Connected characters are saved with world
  saves, on disconnect, and during graceful server shutdowns and restarts. The
  shutdown coordinator waits for pending character saves before allowing the
  server to exit.
- **Exact client and server versions:** CharacterVault requires ModSentry to
  verify that every client has the same CharacterVault DLL as the server before
  enrollment or profile transfer begins.

## Storage guarantees

- Creates no character data until ModSentry and the Valheim peer handshake
  accept the client.
- Commits a new character only after its first complete profile is validated
  and written durably.
- Keeps the existing `character_vault.drp` graceful shutdown protocol.
- Logs every successful character profile commit with its character name and
  revision.
- Uses bounded fragmented transfers, SHA-256 validation, atomic replacement,
  and a previous revision.
- Supports the stable and public test Valheim save APIs for local and Steam
  Cloud profiles.

## Configuration

```ini
[Characters]
AllowMultipleCharactersPerSteamId = true

[New Characters]
StartingItems =
```

Starting items use comma-separated prefab and quantity pairs:

```ini
StartingItems = hammer:1,wood:10,stone:10
```

Command-line switches override BepInEx settings:

```text
--charactervault-allow-multiple-characters true
--charactervault-starting-items hammer:1,wood:10,stone:10
```

The server configuration is authoritative. Starting items are granted exactly
once during initial enrollment. When multiple characters are disabled, an
account that already has an enrolled character cannot enroll another one.

## Graceful server shutdown and restart

CharacterVault watches for `character_vault.drp` in the dedicated server's
working directory. The file must contain the Valheim process ID. When it
receives a valid request, CharacterVault saves connected characters, waits for
the transfers to be committed, and then continues with Valheim's normal world
save and shutdown. It continues the shutdown after a 90-second character-save
timeout if a client does not respond.

For a systemd-managed server, create a stop helper such as
`character-vault-stop.sh`:

```bash
#!/usr/bin/env bash
set -u

readonly valheim_pid="${1:-}"
readonly working_directory="${2:-}"

if [[ ! "$valheim_pid" =~ ^[0-9]+$ ]] || (( valheim_pid <= 1 )); then
  exit 0
fi
if [[ ! -d "$working_directory" ]] || ! kill -0 "$valheim_pid" 2>/dev/null; then
  exit 0
fi

readonly exit_file="$working_directory/character_vault.drp"
readonly temporary_exit_file="$exit_file.$$"
trap 'rm -f -- "$temporary_exit_file"' EXIT HUP INT TERM
printf '%s\n' "$valheim_pid" >"$temporary_exit_file"
mv -f -- "$temporary_exit_file" "$exit_file"
trap - EXIT HUP INT TERM

while kill -0 "$valheim_pid" 2>/dev/null; do
  sleep 1
done
```

Make the helper executable, then configure the service with the helper path and
the same working directory used to start Valheim:

```ini
[Service]
WorkingDirectory=/path/to/server-working-directory
ExecStop=/path/to/character-vault-stop.sh $MAINPID /path/to/server-working-directory
TimeoutStopSec=120
```

The helper writes the request through an atomic rename so CharacterVault never
reads a partial process ID. `TimeoutStopSec=120` gives the plugin's 90-second
save timeout enough time to finish the vanilla shutdown. After reloading the
unit, both `systemctl stop` and `systemctl restart` use the graceful path.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Install matching CharacterVault and ModSentry versions on every client and server.

## Save API compatibility

CharacterVault supports both the stable and public test save APIs. Valheim
changes the signatures of its public save helpers between these releases, so a
narrow runtime adapter selects only the available file writer, atomic replace,
character path, cache invalidation signatures, and named local save source. An
old-format profile is migrated by Valheim when it is saved on the public test
version. The migrated profile is not downgraded to the old format. This
preserves local and Steam Cloud behavior without inspecting private game state.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

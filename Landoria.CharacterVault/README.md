# CharacterVault

CharacterVault stores authoritative Valheim character profiles on the server.
The server applies its saved profile before a player enters the world, preventing
local backups or characters from other servers from replacing trusted state.

## Behavior

| Event | Character save | Completion rule |
|---|---|---|
| Initial enrollment | Yes | The profile must be validated and committed before admission. |
| World or manual save | Yes | The server acknowledges after the durable commit. |
| Voluntary logout | Yes | The server acknowledges after complete receipt and validation, then commits asynchronously. |
| In-game Quit action | Yes | After entering the world, `Menu.QuitGame` waits at most 10 seconds for the voluntary-save acknowledgement. |
| Window close or Alt+F4 | Yes | After entering the world, `Application.wantsToQuit` uses the same bounded fallback flow. |
| Server kick | Yes | The kick waits for the durable final commit. |
| Graceful server stop or restart | Yes | Shutdown waits for connected-character commits before the vanilla world save. |
| Client crash or network loss | No final request | The connection is already unavailable. |

Client and server logs include the request identifier and revision. Voluntary
disconnect logs distinguish profile acceptance from the later durable commit.

## Guarantees

- Validates bounded fragmented transfers with SHA-256 and profile identity checks.
- Writes every save in the instance `characters_local` directory as
  `Steam_<id>_<character>.fch` and simultaneously archives it under
  `characters_local/backups/Steam_<id>_<character>_<UTC timestamp>.fch`.
- Accepts a new character only when it was created during the current game session.
- Requires matching CharacterVault DLLs on client and server through ModSentry.
- Supports local and Steam Cloud profiles on stable and public-test Valheim.
- Grants configured starting items once, during initial enrollment.

An acknowledgement for a voluntary disconnect means the complete profile was
received and validated. The durable write follows asynchronously; a server crash
in that brief interval can lose the accepted revision. Enrollment, ordinary saves,
server kicks, and shutdowns retain durable-commit acknowledgement semantics.

## Compatibility

| Valheim channel | Version | Status |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Installation

| Client | Server |
|---|---|
| Required | Required |

Install matching CharacterVault and ModSentry versions on every client and server.
When CharacterVault refuses admission, Valheim returns to the main menu and displays the server's reason.

## Configuration

The server creates `BepInEx/config/Landoria.CharacterVault.cfg`:

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

Command-line switches override the configuration file:

| Switch | Example |
|---|---|
| `--charactervault-allow-multiple-characters` | `--charactervault-allow-multiple-characters true` |
| `--charactervault-starting-items` | `--charactervault-starting-items hammer:1,wood:10,stone:10` |

When multiple characters are disabled, an account with an enrolled character
cannot enroll another one.

## Graceful server stop and restart

CharacterVault watches for `character_vault.drp` in the server working directory.
The file must contain the current Valheim process ID. A valid request saves
connected characters, waits up to 90 seconds for their commits, then continues
with the vanilla world save and shutdown.

Example systemd stop helper:

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

Configure the service with the same working directory used to start Valheim:

```ini
[Service]
WorkingDirectory=/path/to/server-working-directory
ExecStop=/path/to/character-vault-stop.sh $MAINPID /path/to/server-working-directory
TimeoutStopSec=120
```

Make the helper executable and reload systemd. Atomic rename prevents the plugin
from reading a partial request, and the 120-second service timeout accommodates
the 90-second character-save limit plus vanilla shutdown.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).

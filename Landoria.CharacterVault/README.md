# CharacterVault

With this mod [ServerCharacters](https://thunderstore.io/c/valheim/p/Smoothbrain/ServerCharacters/) will automatically save all connected characters when the server stops or restarts.

## Features

- Requests a fresh profile save from every connected client.
- Waits for ServerCharacters to confirm each profile transfer.
- Processes up to four profile saves at a time to limit shutdown load.
- Disconnects saved players before the application exits.
- Performs Valheim's normal world save and shutdown afterward.
- Continues shutdown after a 90-second timeout and logs unconfirmed profiles.
- Intercepts a private dedicated-server exit request without requiring a separate supervisor.
- Detects its private exit request immediately through filesystem notifications.

## Using the valheim start script

The same shutdown protocol can be used from a regular terminal. Create `start_server.sh` in Valheim's working directory:

```bash
#!/usr/bin/env bash
set -u

readonly working_directory="$(cd -- "$(dirname -- "$0")" && pwd)"
readonly exit_file="$working_directory/character_vault.drp"
cd "$working_directory"

./start_server_bepinex.sh \
  -name "My Server Name" \
  -port 2456 \
  -world "Dedicated" \
  -password "My Password" \
  -public 1 > /dev/null 2>&1 &
readonly valheim_pid=$!

echo "Server started"
echo
read -r -p "Press RETURN to stop server"

readonly temporary_exit_file="$exit_file.$$"
printf '%s\n' "$valheim_pid" >"$temporary_exit_file"
mv -f "$temporary_exit_file" "$exit_file"

echo "Server exit signal set"
echo "Waiting for character and world saves to finish"
wait "$valheim_pid"
echo "Server stopped; you can now close this terminal"
```

Make the script executable and run it:

```bash
chmod +x start_server.sh
./start_server.sh
```

## Running with a service

Create `/home/debian/Valheim/character-vault.sh`:

```bash
#!/usr/bin/env bash
set -u

readonly valheim_pid="${1:-}"
readonly working_directory="$(cd -- "$(dirname -- "$0")" && pwd)"
readonly exit_file="$working_directory/character_vault.drp"

if [[ ! "$valheim_pid" =~ ^[0-9]+$ ]] || (( valheim_pid <= 1 )); then
  exit 0
fi
if ! kill -0 "$valheim_pid" 2>/dev/null; then
  exit 0
fi

readonly temporary_exit_file="$exit_file.$$"
printf '%s\n' "$valheim_pid" >"$temporary_exit_file"
mv -f "$temporary_exit_file" "$exit_file"
while kill -0 "$valheim_pid" 2>/dev/null; do
  sleep 1
done
```

Make the stop script executable:

```bash
chmod +x /home/debian/Valheim/character-vault.sh
```

Create `/etc/systemd/system/valheim.service`. Adjust the description, user, paths, and Valheim launch arguments for your server:

```ini
[Unit]
Description=Valheim dedicated server
Wants=network.target
After=syslog.target network-online.target

[Service]
Type=simple
User=debian
WorkingDirectory=/home/debian/Valheim
ExecStart=/home/debian/Valheim/start_server_bepinex.sh
ExecStop=/home/debian/Valheim/character-vault.sh $MAINPID
KillMode=process
KillSignal=SIGINT
SendSIGKILL=yes
TimeoutStopSec=120
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start the service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now valheim.service
```

Use `sudo systemctl stop valheim.service` or `sudo systemctl restart valheim.service` for a graceful character and world save. An explicit `systemctl stop` remains stopped, and the 120-second systemd timeout is the final fallback.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Install matching versions together with ServerCharacters on every client and the dedicated server.

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

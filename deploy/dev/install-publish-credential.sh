#!/usr/bin/env bash
set -euo pipefail

[[ "$(id -u)" -eq 0 ]] || { echo "This installer must run as root." >&2; exit 1; }
script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
agent_directory=/etc/vault-agent.d
secret_file=/var/lib/landoria-secrets/thunderstore-publish.env

install -d -o root -g vault -m 0750 "$agent_directory/templates"
install -o root -g root -m 0644 \
    "$script_directory/thunderstore-publish.env.ctmpl" \
    "$agent_directory/templates/thunderstore-publish.env.ctmpl"
if ! grep --quiet 'thunderstore-publish.env' "$agent_directory/agent.hcl"; then
    printf '\n' >>"$agent_directory/agent.hcl"
    cat "$script_directory/vault-agent-thunderstore-publish.hcl" >>"$agent_directory/agent.hcl"
fi
if [[ ! -e "$secret_file" ]]; then
    install -o vault -g debian -m 0640 /dev/null "$secret_file"
else
    chown vault:debian "$secret_file"
    chmod 0640 "$secret_file"
fi

systemctl restart vault-agent-landoria.service
for _ in {1..30}; do
    [[ -s "$secret_file" ]] && break
    sleep 1
done
[[ -s "$secret_file" ]] || { echo "Vault Agent did not render the Thunderstore credential." >&2; exit 1; }
sudo -u debian test -r "$secret_file"
echo "The Thunderstore publish credential is available on dev."

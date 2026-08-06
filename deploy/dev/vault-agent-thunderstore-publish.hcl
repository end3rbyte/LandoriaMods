template {
  source               = "/etc/vault-agent.d/templates/thunderstore-publish.env.ctmpl"
  destination          = "/var/lib/landoria-secrets/thunderstore-publish.env"
  perms                = "0640"
  error_on_missing_key = true
}

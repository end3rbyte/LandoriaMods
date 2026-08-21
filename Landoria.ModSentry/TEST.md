# ModSentry tests

Use Valheim `0.221.12` and `start_server_hammer.bat`.

## Client without ModSentry

| Test | Action | Expected result |
| --- | --- | --- |
| Guest admission | Join without ModSentry. | Player enters the Guest Lobby. |
| Banned account | Join with a banned account. | Connection is rejected. |
| Confinement | Leave the Guest Lobby. | Player is returned to the lobby. |
| Welcome | Leave the lobby and wait for teleportation. | Welcome message appears after every return. |
| Signs | Edit each sign. | Original text is restored. |
| Lighting | Use the torch and brazier. | No interaction is available; both stay lit. |
| Protection | Damage, remove, or place pieces. | Lobby pieces are protected; new pieces are removed. |
| Session timeout | Stay connected for 15 minutes. | Client disconnects; server closes any remaining connection. |
| Disconnect | Disconnect, then reconnect. | Guest state is cleared and recreated. |

## Client with ModSentry

| Test | Action | Expected result |
| --- | --- | --- |
| Matching plugins | Join with matching required plugins. | Player joins normally. |
| Guest Lobby exclusion | Enter the Guest Lobby after joining normally. | Player returns to the last position outside the lobby. |
| Saved return position | Save outside the lobby, reconnect once as a Guest, then reconnect with ModSentry. | Player returns to the position stored in the character. |
| Missing plugin | Remove a required plugin and join. | Connection is rejected; missing plugin is named. |
| Outdated plugin | Use a different required-plugin version. | Connection is rejected; outdated plugin is named. |
| Unexpected plugin | Add a plugin not listed as required or optional. | Connection is rejected; unexpected plugin is named. |
| Optional plugin | Join with and without an optional plugin. | Both connections are accepted. |
| Disconnect | Disconnect, then reconnect. | Inventory is validated again. |

# Changelog

## 1.0.5

- Read the authorization switch only on dedicated servers and fail closed elsewhere.
- Use the shared dedicated-server role check.
- Request authorization once after the local player spawns.
- Limit free-camera movement to 20 metres per second.
- Use a one-metre-radius collision sphere to prevent the free camera from passing through terrain and solid objects.

## 1.0.4

- Document Valheim version compatibility

## 1.0.3

- Update thunderstore readme

## 1.0.2

- Document the free-camera FOV command (#51)

## 1.0.1

- Updated the package icon.

## 1.0.0

- Add server-authorized access to Valheim's native `freefly` and `ffsmooth` commands.
- Preserve native smoothing controls and limit the free camera to 50 metres from the player.

# Changelog

## 1.0.10

- Keep groups only for the current server session.
- Remove players from their group when they disconnect.
- Clear stale memberships defensively when players reconnect.
- Promote the longest-standing remaining member when the leader disconnects.

## 1.0.9

- Move player-built piece permissions and offline protection to Parked.
- Add configurable public-position, public-ping, shout-distance, and speech-distance server switches.
- Add an optional one-shot `/all` command using vanilla server-wide shout delivery.
- Synchronize only client-relevant Socialize settings after player spawn.
- Read server command-line settings only on dedicated servers.
- Use the shared dedicated-server role check.

## 1.0.8

- Add unit tests.

## 1.0.7

- Document Valheim version compatibility
- Support Socialize hash API variants

## 1.0.6

- Update thunderstore readme

## 1.0.5

- Preserve player-built structures and fuel while their creator and the creator's group are offline.
- Restrict player-built piece interactions, repairs, removal, and player damage to the creator and their group.
- Prevent creatures from targeting or damaging pieces while their creator and group are offline.
- Show the creator on inaccessible pieces instead of displaying an access-denied message.

## 1.0.4

- Log when the mod is unloaded.


## 1.0.3

- minor fix

## 1.0.0

- Initial Version

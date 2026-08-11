# SlowDecay

Slows structural rain wear and fireplace fuel consumption while preserving vanilla behavior.

## Features

- Divides vanilla rain damage by one global slowdown multiplier.
- Divides fuel consumption for torches, fires, braziers, and hearths by the same multiplier.
- Preserves the vanilla 50% rain-wear health floor.
- Does not alter combat, support-collapse, lava, or Ashlands damage.
- Supports dedicated servers and peer-hosted worlds.

## Configuration

`General.SlowdownMultiplier` defaults to `10`. A value of `10` makes rain wear and fuel consumption ten times slower. Values below `1` are clamped to `1`.

The command-line switch `--slowdecay VALUE` overrides the BepInEx configuration. For example:

```text
--slowdecay 10
```

Install the mod on the server and every client so the effective setting follows network ownership.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

# SlowDecay

Slows structural rain wear and fireplace fuel consumption while preserving vanilla behavior.

## Features

- Divides vanilla rain damage by one global slowdown multiplier.
- Divides fuel consumption for torches, fires, braziers, and hearths by the same multiplier.
- Preserves the vanilla 50% rain-wear health floor.
- Does not alter combat, support-collapse, lava, or Ashlands damage.
- Supports dedicated servers and peer-hosted worlds.

## Configuration

`General.SlowdownMultiplier` defaults to `10`. A value of `10` makes rain wear and fuel consumption ten times slower. The command-line switch `--slowdecay VALUE` overrides the BepInEx configuration.

Install the mod on the server and every client so the effective setting follows network ownership.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Read the [full documentation](https://github.com/end3rbyte/LandoriaMods/blob/main/Landoria.SlowDecay/README.md) on GitHub.

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).

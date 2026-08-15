using BepInEx.Configuration;
using Landoria.SharedLib;

namespace Landoria.Socialize
{
    internal sealed class SocializeSettings
    {
        private const string PositionArgument = "--socialize-restrict-public-positions";
        private const string PingArgument = "--socialize-restrict-public-pings";
        private readonly bool configuredPositions;
        private readonly bool configuredPings;

        private SocializeSettings(bool positions, bool pings)
        {
            configuredPositions = positions;
            configuredPings = pings;
            RestrictPublicPositions = positions;
            RestrictPublicPings = pings;
        }

        internal bool RestrictPublicPositions { get; private set; }
        internal bool RestrictPublicPings { get; private set; }

        internal static SocializeSettings Load(
            ConfigFile config, string[] arguments, ModLog logger)
        {
            bool positions = config.Bind("Map", "RestrictPublicPositions", true).Value;
            bool pings = config.Bind("Map", "RestrictPublicPings", true).Value;
            positions = SocializeArgumentPolicy.Resolve(arguments, PositionArgument, positions);
            pings = SocializeArgumentPolicy.Resolve(arguments, PingArgument, pings);
            logger.LogInfo($"Effective map settings: restrictPublicPositions={positions}, " +
                $"restrictPublicPings={pings}.");
            return new SocializeSettings(positions, pings);
        }

        internal void WriteState(ZPackage package)
        {
            package.Write(RestrictPublicPositions);
            package.Write(RestrictPublicPings);
        }

        internal void ReadState(ZPackage package)
        {
            RestrictPublicPositions = package.ReadBool();
            RestrictPublicPings = package.ReadBool();
        }

        internal void ResetState()
        {
            RestrictPublicPositions = configuredPositions;
            RestrictPublicPings = configuredPings;
        }
    }
}

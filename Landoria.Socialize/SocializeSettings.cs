using BepInEx.Configuration;
using Landoria.SharedLib;

namespace Landoria.Socialize
{
    internal sealed class SocializeSettings
    {
        private const string PositionArgument = "--socialize-restrict-public-positions";
        private const string PingArgument = "--socialize-restrict-public-pings";
        private const string ShoutDistanceArgument = "--socialize-shout-distance";
        private const string SayDistanceArgument = "--socialize-say-distance";
        private readonly bool configuredPositions;
        private readonly bool configuredPings;
        private readonly float configuredShoutDistance;
        private readonly float configuredSayDistance;

        private SocializeSettings(bool positions, bool pings, float shoutDistance, float sayDistance)
        {
            configuredPositions = positions;
            configuredPings = pings;
            RestrictPublicPositions = positions;
            RestrictPublicPings = pings;
            configuredShoutDistance = shoutDistance;
            configuredSayDistance = sayDistance;
            ShoutDistance = shoutDistance;
            SayDistance = sayDistance;
        }

        internal bool RestrictPublicPositions { get; private set; }
        internal bool RestrictPublicPings { get; private set; }
        internal float ShoutDistance { get; private set; }
        internal float SayDistance { get; private set; }

        internal static SocializeSettings Load(
            ConfigFile config, string[] arguments, ModLog logger)
        {
            bool positions = config.Bind("Map", "RestrictPublicPositions", true).Value;
            bool pings = config.Bind("Map", "RestrictPublicPings", true).Value;
            float shoutDistance = config.Bind("Chat", "ShoutDistance", 30f).Value;
            float sayDistance = config.Bind("Chat", "SayDistance", 15f).Value;
            positions = SocializeArgumentPolicy.Resolve(arguments, PositionArgument, positions);
            pings = SocializeArgumentPolicy.Resolve(arguments, PingArgument, pings);
            shoutDistance = SocializeArgumentPolicy.ResolvePositiveFloat(
                arguments, ShoutDistanceArgument, shoutDistance);
            sayDistance = SocializeArgumentPolicy.ResolvePositiveFloat(
                arguments, SayDistanceArgument, sayDistance);
            logger.LogInfo($"Effective map settings: restrictPublicPositions={positions}, " +
                $"restrictPublicPings={pings}.");
            logger.LogInfo($"Effective chat distances: shout={shoutDistance}, say={sayDistance}.");
            return new SocializeSettings(positions, pings, shoutDistance, sayDistance);
        }

        internal void WriteState(ZPackage package)
        {
            package.Write(RestrictPublicPositions);
            package.Write(RestrictPublicPings);
            package.Write(ShoutDistance);
            package.Write(SayDistance);
        }

        internal void ReadState(ZPackage package)
        {
            RestrictPublicPositions = package.ReadBool();
            RestrictPublicPings = package.ReadBool();
            ShoutDistance = package.ReadSingle();
            SayDistance = package.ReadSingle();
        }

        internal void ResetState()
        {
            RestrictPublicPositions = configuredPositions;
            RestrictPublicPings = configuredPings;
            ShoutDistance = configuredShoutDistance;
            SayDistance = configuredSayDistance;
        }
    }
}

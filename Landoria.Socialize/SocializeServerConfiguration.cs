namespace Landoria.Socialize
{
    internal sealed class SocializeServerConfiguration
    {
        internal bool RestrictPublicPositions { get; private set; }
        internal bool RestrictPublicPings { get; private set; }
        internal float ShoutDistance { get; private set; }
        internal float SayDistance { get; private set; }
        internal bool AllChannelEnabled { get; private set; }

        internal static SocializeServerConfiguration FromArguments(string[] arguments)
        {
            return new SocializeServerConfiguration
            {
                RestrictPublicPositions = SocializeArgumentPolicy.Resolve(
                    arguments, "--socialize-restrict-public-positions", true),
                RestrictPublicPings = SocializeArgumentPolicy.Resolve(
                    arguments, "--socialize-restrict-public-pings", true),
                ShoutDistance = SocializeArgumentPolicy.ResolvePositiveFloat(
                    arguments, "--socialize-shout-distance", 30f),
                SayDistance = SocializeArgumentPolicy.ResolvePositiveFloat(
                    arguments, "--socialize-say-distance", 15f),
                AllChannelEnabled = SocializeArgumentPolicy.Resolve(
                    arguments, "--socialize-all-channel-enabled", false)
            };
        }
    }
}

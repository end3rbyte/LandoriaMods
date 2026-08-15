namespace Landoria.Socialize
{
    internal sealed class SocializeServerConfiguration
    {
        internal bool RestrictPublicPositions { get; private set; }
        internal bool RestrictPublicPings { get; private set; }

        internal static SocializeServerConfiguration FromArguments(string[] arguments)
        {
            return new SocializeServerConfiguration
            {
                RestrictPublicPositions = SocializeArgumentPolicy.Resolve(
                    arguments, "--socialize-restrict-public-positions", true),
                RestrictPublicPings = SocializeArgumentPolicy.Resolve(
                    arguments, "--socialize-restrict-public-pings", true)
            };
        }
    }
}

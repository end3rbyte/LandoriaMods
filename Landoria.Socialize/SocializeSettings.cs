using Landoria.SharedLib;

namespace Landoria.Socialize
{
    internal sealed class SocializeSettings
    {
        private bool serverInitialized;

        internal SocializeSettings()
        {
            ResetState();
        }

        internal bool RestrictPublicPositions { get; private set; }
        internal bool RestrictPublicPings { get; private set; }

        internal void InitializeServer(ModLog logger)
        {
            if (serverInitialized || !ServerRole.IsDedicatedServer) return;
            SocializeServerConfiguration configuration =
                SocializeServerConfiguration.FromArguments(
                    System.Environment.GetCommandLineArgs());
            RestrictPublicPositions = configuration.RestrictPublicPositions;
            RestrictPublicPings = configuration.RestrictPublicPings;
            serverInitialized = true;
            LogSettings(logger);
        }

        private void LogSettings(ModLog logger)
        {
            logger.LogInfo($"Effective map settings: restrictPublicPositions=" +
                $"{RestrictPublicPositions}, restrictPublicPings={RestrictPublicPings}.");
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
            if (serverInitialized) return;
            RestrictPublicPositions = true;
            RestrictPublicPings = true;
        }
    }
}

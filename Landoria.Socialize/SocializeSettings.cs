using Landoria.SharedLib;

namespace Landoria.Socialize
{
    internal sealed class SocializeSettings
    {
        private readonly string[] arguments;
        private bool serverInitialized;

        internal SocializeSettings(string[] arguments)
        {
            this.arguments = arguments;
            ResetState();
        }

        internal bool RestrictPublicPositions { get; private set; }
        internal bool RestrictPublicPings { get; private set; }
        internal float ShoutDistance { get; private set; }
        internal float SayDistance { get; private set; }
        internal bool AllChannelEnabled { get; private set; }

        internal void InitializeServer(ModLog logger)
        {
            if (serverInitialized) return;
            SocializeServerConfiguration configuration =
                SocializeServerConfiguration.FromArguments(arguments);
            RestrictPublicPositions = configuration.RestrictPublicPositions;
            RestrictPublicPings = configuration.RestrictPublicPings;
            ShoutDistance = configuration.ShoutDistance;
            SayDistance = configuration.SayDistance;
            AllChannelEnabled = configuration.AllChannelEnabled;
            serverInitialized = true;
            LogSettings(logger);
        }

        private void LogSettings(ModLog logger)
        {
            logger.LogInfo($"Effective map settings: restrictPublicPositions=" +
                $"{RestrictPublicPositions}, restrictPublicPings={RestrictPublicPings}.");
            logger.LogInfo($"Effective chat settings: shoutDistance={ShoutDistance}, " +
                $"sayDistance={SayDistance}, allChannelEnabled={AllChannelEnabled}.");
        }

        internal void WriteState(ZPackage package)
        {
            package.Write(RestrictPublicPositions);
            package.Write(RestrictPublicPings);
            package.Write(ShoutDistance);
            package.Write(SayDistance);
            package.Write(AllChannelEnabled);
        }

        internal void ReadState(ZPackage package)
        {
            RestrictPublicPositions = package.ReadBool();
            RestrictPublicPings = package.ReadBool();
            ShoutDistance = package.ReadSingle();
            SayDistance = package.ReadSingle();
            AllChannelEnabled = package.ReadBool();
        }

        internal void ResetState()
        {
            if (serverInitialized) return;
            RestrictPublicPositions = true;
            RestrictPublicPings = true;
            ShoutDistance = 30f;
            SayDistance = 15f;
            AllChannelEnabled = false;
        }
    }
}

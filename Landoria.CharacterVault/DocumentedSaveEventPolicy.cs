using System;

namespace Landoria.CharacterVault
{
    internal enum DocumentedSaveEvent
    {
        FirstEnrollment,
        AutomaticWorldSave,
        ManualSaveCommand,
        PauseMenuSave,
        Logout,
        MenuQuit,
        ServerKick,
        GracefulShutdown,
        ConnectionLost
    }

    internal enum SaveTarget { None, CurrentCharacter, AllConnectedCharacters }
    internal enum SaveConfirmation { None, Receipt, DurableCommit }

    internal sealed class DocumentedSaveBehavior
    {
        internal DocumentedSaveBehavior(SaveTarget target, SaveConfirmation confirmation,
            bool requiresSpawn, bool preservesVanilla, int timeoutSeconds = 0)
        {
            Target = target;
            Confirmation = confirmation;
            RequiresSpawn = requiresSpawn;
            PreservesVanilla = preservesVanilla;
            TimeoutSeconds = timeoutSeconds;
        }

        internal SaveConfirmation Confirmation { get; }
        internal bool PreservesVanilla { get; }
        internal bool RequiresSpawn { get; }
        internal SaveTarget Target { get; }
        internal int TimeoutSeconds { get; }
    }

    internal static class DocumentedSaveEventPolicy
    {
        internal const int GracefulShutdownTimeoutSeconds = 90;
        internal const int VoluntaryExitTimeoutSeconds = 10;

        internal static DocumentedSaveBehavior Get(DocumentedSaveEvent saveEvent)
        {
            switch (saveEvent)
            {
                case DocumentedSaveEvent.FirstEnrollment:
                    return Behavior(SaveTarget.CurrentCharacter, SaveConfirmation.DurableCommit, true);
                case DocumentedSaveEvent.AutomaticWorldSave:
                case DocumentedSaveEvent.ManualSaveCommand:
                    return Behavior(SaveTarget.AllConnectedCharacters, SaveConfirmation.Receipt, true, true);
                case DocumentedSaveEvent.PauseMenuSave:
                    return Behavior(SaveTarget.CurrentCharacter, SaveConfirmation.Receipt, true, true);
                case DocumentedSaveEvent.Logout:
                    return Behavior(SaveTarget.CurrentCharacter, SaveConfirmation.Receipt, true,
                        timeoutSeconds: VoluntaryExitTimeoutSeconds);
                case DocumentedSaveEvent.MenuQuit:
                    return Behavior(SaveTarget.CurrentCharacter, SaveConfirmation.Receipt, true,
                        timeoutSeconds: VoluntaryExitTimeoutSeconds);
                case DocumentedSaveEvent.ServerKick:
                    return Behavior(SaveTarget.CurrentCharacter,
                        SaveConfirmation.DurableCommit, true);
                case DocumentedSaveEvent.GracefulShutdown:
                    return Behavior(SaveTarget.AllConnectedCharacters,
                        SaveConfirmation.DurableCommit, true,
                        timeoutSeconds: GracefulShutdownTimeoutSeconds);
                case DocumentedSaveEvent.ConnectionLost:
                    return Behavior(SaveTarget.None, SaveConfirmation.None, false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(saveEvent));
            }
        }

        private static DocumentedSaveBehavior Behavior(SaveTarget target,
            SaveConfirmation confirmation, bool requiresSpawn, bool preservesVanilla = false,
            int timeoutSeconds = 0)
        {
            return new DocumentedSaveBehavior(target, confirmation, requiresSpawn,
                preservesVanilla, timeoutSeconds);
        }
    }
}

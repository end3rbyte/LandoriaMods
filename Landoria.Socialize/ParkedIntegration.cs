using System;

namespace Landoria.Socialize
{
    internal static class ParkedIntegration
    {
        private static Action reset;
        private static Action<ZPackage> writeSnapshot;
        private static Action<ZPackage> readSnapshot;

        internal static void Register(
            Action resetHandler, Action<ZPackage> writeHandler,
            Action<ZPackage> readHandler)
        {
            reset = resetHandler;
            writeSnapshot = writeHandler;
            readSnapshot = readHandler;
        }

        internal static void Unregister()
        {
            reset = null;
            writeSnapshot = null;
            readSnapshot = null;
        }

        internal static void Reset()
        {
            reset?.Invoke();
        }

        internal static void WriteSnapshot(ZPackage package)
        {
            writeSnapshot?.Invoke(package);
        }

        internal static void ReadSnapshot(ZPackage package)
        {
            readSnapshot?.Invoke(package);
        }
    }
}

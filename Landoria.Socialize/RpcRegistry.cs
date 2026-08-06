using System;

namespace Landoria.Socialize
{
    internal static class RpcRegistry
    {
        internal static bool RegisterIfChanged(
            ref ZRoutedRpc registered,
            Action<ZRoutedRpc> register)
        {
            ZRoutedRpc current = ZRoutedRpc.instance;
            if (current == null || current == registered)
            {
                return false;
            }

            registered = current;
            register(current);
            return true;
        }
    }
}

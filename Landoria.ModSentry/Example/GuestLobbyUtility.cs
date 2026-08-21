namespace GuestLobbyExample
{
    /// <summary>Provides shared version-compatible helpers.</summary>
    internal static class GuestLobbyUtility
    {
        /// <summary>Computes a stable hash compatible with Valheim 0.221.12.</summary>
        internal static int StableHash(string value)
        {
            int first = 5381;
            int second = first;
            for (int index = 0; index < value.Length; index += 2)
            {
                first = ((first << 5) + first) ^ value[index];
                if (index == value.Length - 1 || value[index + 1] == 0)
                {
                    break;
                }
                second = ((second << 5) + second) ^ value[index + 1];
            }
            return first + (second * 1566083941);
        }
    }
}

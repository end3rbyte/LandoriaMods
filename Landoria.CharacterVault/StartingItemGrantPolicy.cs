using System;

namespace Landoria.CharacterVault
{
    internal static class StartingItemGrantPolicy
    {
        internal static bool Grant<TItem>(string prefabName, int quantity,
            Func<string, TItem> findItem, Func<TItem, int, bool> addItem)
            where TItem : class
        {
            TItem item = findItem(prefabName);
            return item != null && addItem(item, quantity);
        }
    }
}

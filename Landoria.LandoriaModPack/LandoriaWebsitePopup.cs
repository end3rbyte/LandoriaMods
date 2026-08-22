using System;
using System.Collections;
using UnityEngine;

namespace Landoria.LandoriaModPack
{
    internal static class LandoriaWebsitePopup
    {
        private const string WebsiteUrl = "https://valheim.landoria-gaming.com/";

        internal static void Show()
        {
            string content = "Open the " +
                WarningPopupExtended.Link("Landoria Website", WebsiteUrl);
            UnifiedPopup.Push(new WarningPopupExtended(
                "Landoria", content, UnifiedPopup.Pop));
        }

        internal static IEnumerator ShowWhenAvailable(Func<bool> shouldCancel)
        {
            yield return new WaitUntil(() =>
                UnifiedPopup.IsAvailable() || shouldCancel());
            if (!shouldCancel())
            {
                Show();
            }
        }
    }
}

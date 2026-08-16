using System.Collections.Generic;
using UnityEngine;

namespace Landoria.FirstPerson
{
    internal static class LocalPlayerVisibility
    {
        private static readonly string[] HandKeywordHints =
        {
            "hand",
        };

        private static readonly Dictionary<Renderer, bool> OriginalStates =
            new Dictionary<Renderer, bool>();

        private static Player trackedPlayer;
        private static bool hidden;

        internal static void Update(Player player, bool shouldHide)
        {
            if (player != trackedPlayer)
            {
                Restore();
                trackedPlayer = player;
            }

            if (shouldHide == hidden)
            {
                return;
            }

            if (shouldHide)
            {
                HideRenderers();
            }
            else
            {
                Restore();
            }
        }

        internal static void Refresh()
        {
            if (hidden)
            {
                HideRenderers();
            }
        }

        internal static void Reset()
        {
            Restore();
            trackedPlayer = null;
        }

        private static void HideRenderers()
        {
            if (!trackedPlayer)
            {
                return;
            }

            foreach (Renderer renderer in trackedPlayer.GetComponentsInChildren<Renderer>(true))
            {
                if (ShouldKeepRendererVisible(renderer))
                {
                    if (!OriginalStates.ContainsKey(renderer))
                    {
                        OriginalStates.Add(renderer, renderer.forceRenderingOff);
                    }

                    renderer.forceRenderingOff = false;
                    continue;
                }

                if (!OriginalStates.ContainsKey(renderer))
                {
                    OriginalStates.Add(renderer, renderer.forceRenderingOff);
                }

                renderer.forceRenderingOff = true;
            }

            hidden = true;
        }

        private static bool ShouldKeepRendererVisible(Renderer renderer)
        {
            if (!renderer)
            {
                return false;
            }

            for (Transform current = renderer.transform; current != null; current = current.parent)
            {
                string transformName = current.name.ToLowerInvariant();

                foreach (string hint in HandKeywordHints)
                {
                    if (transformName.Contains(hint))
                    {
                        return true;
                    }
                }

                if (current == trackedPlayer.transform)
                {
                    break;
                }
            }

            return false;
        }

        private static void Restore()
        {
            foreach (KeyValuePair<Renderer, bool> state in OriginalStates)
            {
                if (state.Key)
                {
                    state.Key.forceRenderingOff = state.Value;
                }
            }

            OriginalStates.Clear();
            hidden = false;
        }
    }
}

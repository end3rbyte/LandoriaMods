using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.Socialize
{
    [HarmonyPatch(typeof(Minimap), "Update")]
    internal static class UpdateMapPingVisibilityPatch
    {
        private static void Postfix(Minimap __instance)
        {
            HidePublicPositionTogglePatch.UpdatePingVisibility(__instance);
        }
    }

    [HarmonyPatch(typeof(ZNet), "SetPublicReferencePosition")]
    internal static class DisablePublicPositionPatch
    {
        private static void Prefix(ref bool pub)
        {
            pub = false;
        }
    }

    [HarmonyPatch(typeof(ZNet), "GetOtherPublicPlayers")]
    internal static class ShowGroupMembersOnMapPatch
    {
        private static void Postfix(List<ZNet.PlayerInfo> playerList)
        {
            GroupMapSharing.AddGroupMembers(playerList);
        }
    }

    [HarmonyPatch(typeof(Minimap), "Start")]
    internal static class HidePublicPositionTogglePatch
    {
        private static void Postfix(Minimap __instance)
        {
            Toggle toggle = __instance.m_publicPosition;
            if (toggle != null)
            {
                toggle.isOn = false;
                toggle.gameObject.SetActive(false);
                HideDedicatedContainer(__instance, toggle);
            }
            UpdatePingVisibility(__instance);
        }

        internal static void UpdatePingVisibility(Minimap minimap)
        {
            if (minimap.m_pingImageObject == null) return;
            GetPingButton(minimap).SetActive(GroupService.IsLocalPlayerInGroup());
        }

        private static GameObject GetPingButton(Minimap minimap)
        {
            Button button = minimap.m_pingImageObject.GetComponentInParent<Button>(true);
            if (button != null && !IsMapRoot(minimap, button.gameObject))
            {
                return button.gameObject;
            }
            Transform parent = minimap.m_pingImageObject.transform.parent;
            if (parent != null && !IsMapRoot(minimap, parent.gameObject)
                && parent.GetComponentsInChildren<RawImage>(true).Length == 0)
            {
                return parent.gameObject;
            }
            return minimap.m_pingImageObject.gameObject;
        }

        private static void HideDedicatedContainer(Minimap minimap, Toggle toggle)
        {
            Transform parent = toggle.transform.parent;
            if (parent == null || IsMapRoot(minimap, parent.gameObject)) return;
            bool onlyToggle = parent.GetComponentsInChildren<Toggle>(true).Length == 1;
            bool containsMap = parent.GetComponentsInChildren<RawImage>(true).Length > 0;
            if (onlyToggle && !containsMap) parent.gameObject.SetActive(false);
        }

        private static bool IsMapRoot(Minimap minimap, GameObject target)
        {
            return target == minimap.m_largeRoot || target == minimap.m_smallRoot
                   || target == minimap.m_mapLarge || target == minimap.m_mapSmall;
        }
    }
}

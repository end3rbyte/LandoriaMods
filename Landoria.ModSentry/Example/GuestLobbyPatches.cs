using System.Collections.Generic;
using HarmonyLib;

namespace GuestLobbyExample
{
    /// <summary>Starts lobby generation during world location setup.</summary>
    [HarmonyPatch(typeof(ZoneSystem), "GenerateLocationsIfNeeded")]
    internal static class StartGuestLobbyGenerationPatch
    {
        private static void Prefix()
        {
            GuestLobbyGenerator.Attach();
        }
    }

    /// <summary>Restores protection when a lobby network view loads.</summary>
    [HarmonyPatch(typeof(ZNetView), "Awake")]
    internal static class RestoreGuestLobbyProtectionPatch
    {
        private static void Postfix(ZNetView __instance)
        {
            GuestLobbyProtection.Apply(__instance);
        }
    }

    /// <summary>Keeps every protected lobby object owned by the server.</summary>
    [HarmonyPatch(typeof(ZDO), nameof(ZDO.SetOwner))]
    internal static class KeepGuestLobbyServerOwnedPatch
    {
        private static void Prefix(ZDO __instance, ref long uid)
        {
            if (ZNet.instance?.IsServer() == true &&
                GuestLobbyProtection.IsProtected(__instance))
            {
                uid = ZDOMan.GetSessionID();
            }
        }
    }

    /// <summary>Prevents the removal of protected lobby pieces.</summary>
    [HarmonyPatch(typeof(WearNTear), "RPC_Remove")]
    internal static class PreventGuestLobbyRemovalPatch
    {
        private static bool Prefix(WearNTear __instance)
        {
            return !GuestLobbyProtection.IsProtected(__instance);
        }
    }

    /// <summary>Prevents RPC damage to protected lobby pieces.</summary>
    [HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
    internal static class PreventGuestLobbyDamagePatch
    {
        private static bool Prefix(WearNTear __instance)
        {
            return !GuestLobbyProtection.IsProtected(__instance);
        }
    }

    /// <summary>Prevents structural damage to protected lobby pieces.</summary>
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.ApplyDamage))]
    internal static class PreventGuestLobbyStructuralDamagePatch
    {
        private static bool Prefix(WearNTear __instance, ref bool __result)
        {
            if (!GuestLobbyProtection.IsProtected(__instance))
            {
                return true;
            }
            __result = false;
            return false;
        }
    }

    /// <summary>Detects and removes pieces built by guests in the lobby.</summary>
    [HarmonyPatch(typeof(ZDOMan), "RPC_ZDOData")]
    internal static class PreventGuestBuildingPatch
    {
        private static void Prefix(ZRpc rpc, ZPackage pkg,
            out List<ZDOID> __state)
        {
            __state = GuestLobbyProtection.FindNewIds(rpc, pkg);
        }

        private static void Postfix(ZRpc rpc, List<ZDOID> __state)
        {
            GuestLobbyProtection.RemoveNewGuestPieces(rpc, __state);
        }
    }

    /// <summary>Prevents adding fuel to the protected lobby brazier.</summary>
    [HarmonyPatch(typeof(Fireplace), "RPC_AddFuel")]
    internal static class PreventLobbyBrazierAddFuelPatch
    {
        private static bool Prefix(Fireplace __instance) =>
            GuestLobbyProtection.AllowFireplaceMutation(__instance);
    }

    /// <summary>Prevents changing the protected lobby brazier fuel.</summary>
    [HarmonyPatch(typeof(Fireplace), "RPC_AddFuelAmount")]
    internal static class PreventLobbyBrazierAddFuelAmountPatch
    {
        private static bool Prefix(Fireplace __instance) =>
            GuestLobbyProtection.AllowFireplaceMutation(__instance);
    }

    /// <summary>Prevents setting the protected lobby brazier fuel.</summary>
    [HarmonyPatch(typeof(Fireplace), "RPC_SetFuelAmount")]
    internal static class PreventLobbyBrazierSetFuelPatch
    {
        private static bool Prefix(Fireplace __instance) =>
            GuestLobbyProtection.AllowFireplaceMutation(__instance);
    }

    /// <summary>Prevents switching off the protected lobby brazier.</summary>
    [HarmonyPatch(typeof(Fireplace), "RPC_ToggleOn")]
    internal static class PreventLobbyBrazierTogglePatch
    {
        private static bool Prefix(Fireplace __instance) =>
            GuestLobbyProtection.AllowFireplaceMutation(__instance);
    }

    /// <summary>Prevents ownership of the protected lobby rug.</summary>
    [HarmonyPatch(typeof(ItemDrop), "RPC_RequestOwn")]
    internal static class PreventLobbyRugOwnershipPatch
    {
        private static bool Prefix(ItemDrop __instance) =>
            GuestLobbyProtection.AllowItemOwnershipRequest(__instance);
    }
}

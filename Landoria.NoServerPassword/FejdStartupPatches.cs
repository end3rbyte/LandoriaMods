using GUIFramework;
using HarmonyLib;
using TMPro;

namespace Landoria.NoServerPassword
{
    [HarmonyPatch(typeof(FejdStartup), "GetPublicPasswordError")]
    internal static class PublicPasswordErrorPatch
    {
        private static bool Prefix(string password, ref string __result)
        {
            if (!string.IsNullOrEmpty(password)) return true;
            __result = string.Empty;
            return false;
        }
    }

    [HarmonyPatch(typeof(FejdStartup), "IsPublicPasswordValid")]
    internal static class PublicPasswordValidationPatch
    {
        private static bool Prefix(string password, ref bool __result)
        {
            if (!string.IsNullOrEmpty(password)) return true;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(FejdStartup), "CanStartServer")]
    internal static class CanStartServerPatch
    {
        private static void Postfix(GuiInputField ___m_serverPassword,
            World ___m_world, ref bool __result)
        {
            if (!__result && string.IsNullOrEmpty(___m_serverPassword?.text) &&
                ___m_world != null && ___m_world.m_dataError == World.SaveDataError.None)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(FejdStartup), "UpdatePasswordError")]
    internal static class PasswordErrorDisplayPatch
    {
        private static void Postfix(GuiInputField ___m_serverPassword,
            TMP_Text ___m_passwordError)
        {
            if (string.IsNullOrEmpty(___m_serverPassword?.text) && ___m_passwordError != null)
            {
                ___m_passwordError.text = string.Empty;
            }
        }
    }
}

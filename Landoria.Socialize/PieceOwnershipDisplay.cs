using HarmonyLib;
using TMPro;

namespace Landoria.Socialize
{
    internal static class PieceOwnershipDisplay
    {
        private const string UnknownCreator = "Unknown creator";

        private static string GetCreatorName(Piece piece)
        {
            long creator = piece.GetCreator();
            ZDO zdo = piece.GetComponent<ZNetView>()?.GetZDO();
            string name = zdo?.GetString(ZDOVars.s_creatorName) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = PiecePermissions.GetKnownPlayerName(creator);
            }
            return string.IsNullOrWhiteSpace(name)
                ? UnknownCreator
                : CensorShittyWords.FilterUGC(name, UGCType.CharacterName, creator);
        }

        [HarmonyPatch(typeof(Piece), nameof(Piece.SetCreator))]
        private static class StoreCreatorNamePatch
        {
            private static void Postfix(Piece __instance, long uid)
            {
                PlayerProfile profile = Game.instance?.GetPlayerProfile();
                ZNetView view = __instance.GetComponent<ZNetView>();
                if (profile == null || profile.GetPlayerID() != uid || view == null ||
                    !view.IsValid() || !view.IsOwner())
                {
                    return;
                }
                view.GetZDO().Set(ZDOVars.s_creatorName, profile.GetName());
            }
        }

        [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
        private static class CreatorHoverPatch
        {
            private static void Postfix(Player player, TMP_Text ___m_hoverName)
            {
                Piece piece = player.GetHoverObject()?.GetComponentInParent<Piece>();
                if (piece == null || PiecePermissions.CanAccess(player.GetPlayerID(), piece))
                {
                    return;
                }
                string creator = GetCreatorName(piece);
                string prefix = string.IsNullOrEmpty(___m_hoverName.text) ? "" : "\n";
                ___m_hoverName.text += prefix + "<color=orange>Created by " + creator + "</color>";
            }
        }
    }
}

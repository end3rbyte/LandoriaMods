using HarmonyLib;
using TMPro;

namespace Landoria.Socialize
{
    internal static class PieceOwnershipDisplay
    {
        private static string GetCreatorName(Piece piece)
        {
            long creator = piece.GetCreator();
            ZDO zdo = piece.GetComponent<ZNetView>()?.GetZDO();
            string name = zdo?.GetString(ZDOVars.s_creatorName) ?? string.Empty;
            name = CreatorDisplayPolicy.ResolveName(
                name, PiecePermissions.GetKnownPlayerName(creator));
            return name == CreatorDisplayPolicy.UnknownCreator
                ? name
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
                ___m_hoverName.text = CreatorDisplayPolicy.AppendCreator(
                    ___m_hoverName.text, creator);
            }
        }
    }
}

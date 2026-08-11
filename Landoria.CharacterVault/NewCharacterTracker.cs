using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Landoria.CharacterVault
{
    internal static class NewCharacterTracker
    {
        private static readonly HashSet<long> CreatedThisSession = new HashSet<long>();

        internal static bool WasCreatedThisSession(long characterId)
        {
            return CreatedThisSession.Contains(characterId);
        }

        internal static HashSet<long> CaptureExistingCharacters()
        {
            return new HashSet<long>(SaveSystem.GetAllPlayerProfiles().Select(profile => profile.GetPlayerID()));
        }

        internal static void RecordNewCharacters(HashSet<long> existingCharacters)
        {
            foreach (PlayerProfile profile in SaveSystem.GetAllPlayerProfiles())
            {
                if (!existingCharacters.Contains(profile.GetPlayerID()))
                {
                    CreatedThisSession.Add(profile.GetPlayerID());
                }
            }
        }
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.OnNewCharacterDone))]
    internal static class NewCharacterCreationPatch
    {
        private static void Prefix(out HashSet<long> __state)
        {
            __state = NewCharacterTracker.CaptureExistingCharacters();
        }

        private static void Postfix(HashSet<long> __state)
        {
            NewCharacterTracker.RecordNewCharacters(__state);
        }
    }
}

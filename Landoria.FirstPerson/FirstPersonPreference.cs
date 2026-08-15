namespace Landoria.FirstPerson
{
    internal static class FirstPersonPreference
    {
        internal const string CharacterDataKey = "Landoria.FirstPerson.Enabled";

        internal static bool Load(Player player)
        {
            if (!player || !player.m_customData.TryGetValue(CharacterDataKey, out string value))
            {
                return false;
            }

            return FirstPersonPolicy.IsPreferenceEnabled(value);
        }

        internal static void Save(Player player, bool enabled)
        {
            if (player)
            {
                player.m_customData[CharacterDataKey] = enabled.ToString();
            }
        }
    }
}

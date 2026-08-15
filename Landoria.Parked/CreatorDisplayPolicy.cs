namespace Landoria.Parked
{
    internal static class CreatorDisplayPolicy
    {
        internal const string UnknownCreator = "Unknown creator";

        internal static string ResolveName(string storedName, string knownName)
        {
            if (!string.IsNullOrWhiteSpace(storedName)) return storedName;
            return string.IsNullOrWhiteSpace(knownName) ? UnknownCreator : knownName;
        }

        internal static string AppendCreator(string hoverText, string creatorName)
        {
            string prefix = string.IsNullOrEmpty(hoverText) ? "" : "\n";
            return hoverText + prefix + "<color=orange>Created by " + creatorName + "</color>";
        }
    }
}

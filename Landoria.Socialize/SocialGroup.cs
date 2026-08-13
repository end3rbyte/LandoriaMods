using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal sealed class SocialGroup
    {
        internal const int MaximumSize = 5;
        internal int Id;
        internal long Leader;
        internal readonly Dictionary<long, string> Members =
            new Dictionary<long, string>();
    }
}

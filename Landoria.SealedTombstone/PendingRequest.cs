using System;

namespace Landoria.SealedTombstone
{
    internal sealed class PendingRequest
    {
        internal long RequesterPlayerId { get; set; }
        internal string RequesterName { get; set; }
        internal ZDOID TombstoneId { get; set; }
        internal DateTime CreatedAt { get; set; }
    }
}

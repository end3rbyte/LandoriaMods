using Xunit;

namespace Landoria.ModSentry.Tests;

public sealed class GuestPermissionPolicyTests
{
    [Theory]
    [InlineData(false, true, false, true)]
    [InlineData(false, true, true, false)]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, true)]
    public void TemporaryGuestsBypassOnlyThePermittedList(bool vanillaAllowed,
        bool temporaryGuest, bool banned, bool expected)
    {
        Assert.Equal(expected,
            GuestPermissionPolicy.Resolve(vanillaAllowed, temporaryGuest, banned));
    }
}

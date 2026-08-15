using Xunit;

namespace Landoria.StructureProtection;

public sealed class WardProtectionPolicyTests
{
    [Fact]
    public void CreatorOnlineKeepsVanillaDamage()
    {
        Assert.True(WardProtectionPolicy.HasOnlineAuthorizedPlayer(
            1, Array.Empty<long>(), new HashSet<long> { 1 }));
    }

    [Fact]
    public void PermittedPlayerOnlineKeepsVanillaDamage()
    {
        Assert.True(WardProtectionPolicy.HasOnlineAuthorizedPlayer(
            1, new[] { 2L, 3L }, new HashSet<long> { 3 }));
    }

    [Fact]
    public void EveryoneOfflineEnablesWardDamageProtection()
    {
        Assert.False(WardProtectionPolicy.HasOnlineAuthorizedPlayer(
            1, new[] { 2L, 3L }, new HashSet<long> { 9 }));
    }
}

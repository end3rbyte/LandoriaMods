using Xunit;

namespace Landoria.DecayControl;

public sealed class CreatorActivityPolicyTests
{
    [Fact]
    public void OnlyOnlinePlayersAreActiveCreators()
    {
        HashSet<long> active = CreatorActivityPolicy.GetActiveCreators(
            new[] { 2L, 9L });

        Assert.Equal(new HashSet<long> { 2, 9 }, active);
    }

    [Fact]
    public void OnlineCreatorIsActive()
    {
        Assert.True(CreatorActivityPolicy.IsCreatorActive(
            1, new HashSet<long> { 1 }));
    }

    [Fact]
    public void OfflineCreatorIsInactive()
    {
        Assert.False(CreatorActivityPolicy.IsCreatorActive(
            1, new HashSet<long> { 2 }));
    }
}

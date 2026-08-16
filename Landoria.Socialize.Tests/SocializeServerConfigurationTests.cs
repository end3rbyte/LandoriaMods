using Xunit;

namespace Landoria.Socialize;

public sealed class SocializeServerConfigurationTests
{
    [Fact]
    public void DefaultsMatchDedicatedServerPolicy()
    {
        SocializeServerConfiguration configuration =
            SocializeServerConfiguration.FromArguments(System.Array.Empty<string>());

        Assert.True(configuration.RestrictPublicPositions);
        Assert.True(configuration.RestrictPublicPings);
        Assert.Equal(30f, configuration.ShoutDistance);
        Assert.Equal(15f, configuration.SayDistance);
    }
}

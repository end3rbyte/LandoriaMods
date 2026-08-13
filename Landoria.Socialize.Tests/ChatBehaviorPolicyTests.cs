using Xunit;

namespace Landoria.Socialize;

public sealed class ChatBehaviorPolicyTests
{
    // Verifies the prompt displayed for every persistent chat channel.
    [Theory]
    [InlineData(0, "", "Speaking...")]
    [InlineData(1, "", "Shouting...")]
    [InlineData(2, "Alice", "Talking to Alice...")]
    [InlineData(3, "", "Speaking to the group...")]
    public void ChannelPromptMatchesSelectedChannel(
        int channel, string target, string expected)
    {
        Assert.Equal(expected, ChatBehaviorPolicy.GetPrompt(
            (PersistentChatChannel)channel, target));
    }

    // Verifies that non-normal channels redirect non-empty follow-up messages.
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void PersistentChannelRedirectsMessage(int channel)
    {
        Assert.True(ChatBehaviorPolicy.ShouldRedirect(
            (PersistentChatChannel)channel, false, "hello"));
    }

    // Verifies that normal, empty, and recursively redirected messages keep vanilla handling.
    [Theory]
    [InlineData(0, false, "hello")]
    [InlineData(3, false, "  ")]
    [InlineData(3, true, "hello")]
    public void IneligibleMessageIsNotRedirected(
        int channel, bool redirecting, string text)
    {
        Assert.False(ChatBehaviorPolicy.ShouldRedirect(
            (PersistentChatChannel)channel, redirecting, text));
    }

    // Verifies that Socialize doubles Valheim's normal chat range for shouts.
    [Fact]
    public void ShoutRangeIsTwiceNormalRange()
    {
        Assert.Equal(30f, ChatBehaviorPolicy.GetShoutDistance(15f));
    }

    // Verifies the error returned when a whisper target is not connected.
    [Fact]
    public void WhisperToMissingPlayerIsRejected()
    {
        GroupDecision result = PrivateChatPolicy.CanSend(false, false, "Alice");

        Assert.False(result.Allowed);
        Assert.Equal("No connected player named \"Alice\" was found.", result.Message);
    }

    // Verifies the error returned when a player tries to whisper themselves.
    [Fact]
    public void WhisperToSelfIsRejected()
    {
        GroupDecision result = PrivateChatPolicy.CanSend(true, true, "Alice");

        Assert.False(result.Allowed);
        Assert.Equal("You cannot whisper yourself.", result.Message);
    }

    // Verifies that a connected remote player can receive a whisper.
    [Fact]
    public void WhisperToConnectedRemotePlayerIsAllowed()
    {
        Assert.True(PrivateChatPolicy.CanSend(true, false, "Alice").Allowed);
    }

    // Verifies that a private ping requires both a connected target and a ready client RPC path.
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void PrivatePingWithoutReadyTargetIsRejected(bool targetFound, bool clientReady)
    {
        GroupDecision result = TargetPingPolicy.CanSend(targetFound, clientReady, "Alice");

        Assert.False(result.Allowed);
        Assert.Equal("No connected player named \"Alice\" was found.", result.Message);
    }

    // Verifies that a connected target receives a private ping when the RPC path is ready.
    [Fact]
    public void PrivatePingToReadyTargetIsAllowed()
    {
        Assert.True(TargetPingPolicy.CanSend(true, true, "Alice").Allowed);
    }
}

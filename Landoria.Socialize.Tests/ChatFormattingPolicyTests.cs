using Xunit;

namespace Landoria.Socialize;

public sealed class ChatFormattingPolicyTests
{
    // Verifies the documented blue formatting used for group chat.
    [Fact]
    public void GroupChatUsesBlueFormatting()
    {
        Assert.Equal("<color=#4A90E2>Alice: hello</color>",
            ChatFormattingPolicy.FormatGroup("Alice", "hello"));
    }

    // Verifies incoming and outgoing private messages use the documented green formatting.
    [Theory]
    [InlineData("hello", "<color=#2FAE5F>Alice: hello</color>")]
    [InlineData("to Bob: hello", "<color=#2FAE5F>Alice to Bob: hello</color>")]
    public void PrivateChatUsesGreenFormatting(string text, string expected)
    {
        Assert.Equal(expected, ChatFormattingPolicy.FormatPrivate("Alice", text));
    }

    // Verifies shout sender and message colors match the documented presentation.
    [Fact]
    public void ShoutUsesOrangeSenderAndYellowText()
    {
        Assert.Equal("<color=orange>Alice</color>: <color=#FFFF00>hello</color>",
            ChatFormattingPolicy.FormatShout("Alice", "hello"));
    }

    // Verifies private pings identify the recipient and retain green/yellow formatting.
    [Fact]
    public void PrivatePingIncludesRecipientAndLabel()
    {
        Assert.Equal("<color=#2FAE5F>Alice to Bob: </color>" +
                     "<color=#FFFF00>((Ping))</color><color=#2FAE5F> here</color>",
            ChatFormattingPolicy.FormatPing("Alice", "Bob", "here"));
    }
}

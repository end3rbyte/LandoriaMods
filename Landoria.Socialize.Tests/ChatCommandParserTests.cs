using Xunit;

namespace Landoria.Socialize;

public sealed class ChatCommandParserTests
{
    // Verifies target and message parsing for whisper and targeted-ping commands.
    [Theory]
    [InlineData("/w Alice hello there", "Alice", "hello there")]
    [InlineData("/w @Alice hello", "Alice", "hello")]
    [InlineData("/wping Bob look here", "Bob", "look here")]
    public void TargetCommandIsParsed(string input, string target, string message)
    {
        bool parsed = ChatCommandParser.TryParseTarget(input, out string actualTarget,
            out string actualMessage);

        Assert.True(parsed);
        Assert.Equal(target, actualTarget);
        Assert.Equal(message, actualMessage);
    }

    // Verifies that incomplete target commands are rejected.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/w")]
    [InlineData("/w Alice")]
    [InlineData("/w  hello")]
    [InlineData("/w Alice   ")]
    public void IncompleteTargetCommandIsRejected(string input)
    {
        Assert.False(ChatCommandParser.TryParseTarget(input, out _, out _));
    }

    // Verifies which group commands require an argument.
    [Theory]
    [InlineData("invite", "Alice", true)]
    [InlineData("remove", "Alice", true)]
    [InlineData("promote", "Alice", true)]
    [InlineData("leave", "", true)]
    [InlineData("info", "", true)]
    [InlineData("invite", "", false)]
    [InlineData("leave", "Alice", false)]
    [InlineData("unknown", "", false)]
    public void GroupActionArgumentsAreValidated(string action, string argument, bool expected)
    {
        Assert.Equal(expected, ChatCommandParser.IsValidGroupAction(action, argument));
    }
}

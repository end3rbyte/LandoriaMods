using Moq;
using Xunit;

namespace Landoria.Socialize;

public sealed class GroupChatPolicyTests
{
    // Verifies group chat reports when the sender does not belong to a group.
    [Fact]
    public void PlayerOutsideGroupCannotSendGroupChat()
    {
        Mock<Func<long, bool>> isOnline = new(MockBehavior.Strict);
        Mock<Func<string, string, string>> format = new(MockBehavior.Strict);

        GroupChatResult result = GroupChatPolicy.Prepare(
            null, 1, "Hello", isOnline.Object, format.Object);

        Assert.False(result.Broadcast);
        Assert.Equal("You are not in a group.", result.Message);
        isOnline.VerifyNoOtherCalls();
        format.VerifyNoOtherCalls();
    }

    // Verifies group chat reports when every other group member is disconnected.
    [Fact]
    public void PlayerCannotSendGroupChatWhenNobodyElseIsConnected()
    {
        SocialGroup group = Group();
        Mock<Func<long, bool>> isOnline = new(MockBehavior.Strict);
        Mock<Func<string, string, string>> format = new(MockBehavior.Strict);
        isOnline.Setup(check => check(2)).Returns(false);
        isOnline.Setup(check => check(3)).Returns(false);

        GroupChatResult result = GroupChatPolicy.Prepare(
            group, 1, "Hello", isOnline.Object, format.Object);

        Assert.False(result.Broadcast);
        Assert.Equal("No other group member is connected.", result.Message);
        isOnline.Verify(check => check(2), Times.Once);
        isOnline.Verify(check => check(3), Times.Once);
        format.VerifyNoOtherCalls();
    }

    // Verifies group chat broadcasts a formatted message when another member is connected.
    [Fact]
    public void ConnectedMemberReceivesFormattedGroupChat()
    {
        SocialGroup group = Group();
        Mock<Func<long, bool>> isOnline = new(MockBehavior.Strict);
        Mock<Func<string, string, string>> format = new(MockBehavior.Strict);
        isOnline.Setup(check => check(2)).Returns(true);
        format.Setup(apply => apply("Sender", "Hello")).Returns("formatted");

        GroupChatResult result = GroupChatPolicy.Prepare(
            group, 1, "Hello", isOnline.Object, format.Object);

        Assert.True(result.Broadcast);
        Assert.Equal("formatted", result.Message);
        isOnline.Verify(check => check(2), Times.Once);
        format.Verify(apply => apply("Sender", "Hello"), Times.Once);
    }

    private static SocialGroup Group()
    {
        SocialGroup group = new() { Id = 1, Leader = 1 };
        group.Members[1] = "Sender";
        group.Members[2] = "Online";
        group.Members[3] = "Offline";
        return group;
    }
}

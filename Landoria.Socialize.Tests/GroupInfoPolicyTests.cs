using Moq;
using Xunit;

namespace Landoria.Socialize;

public sealed class GroupInfoPolicyTests
{
    // Verifies group info reports when the requesting player has no group.
    [Fact]
    public void PlayerOutsideGroupReceivesNotInGroupMessage()
    {
        Mock<Func<long, bool>> isOnline = new(MockBehavior.Strict);

        string message = GroupInfoPolicy.Build(null, isOnline.Object);

        Assert.Equal("You are not in a group.", message);
        isOnline.VerifyNoOtherCalls();
    }

    // Verifies group info lists members, connection state, and the leader marker.
    [Fact]
    public void GroupInfoListsConnectedLeaderAndDisconnectedMember()
    {
        SocialGroup group = new() { Id = 1, Leader = 1 };
        group.Members[1] = "Leader";
        group.Members[2] = "Member";
        Mock<Func<long, bool>> isOnline = new(MockBehavior.Strict);
        isOnline.Setup(check => check(1)).Returns(true);
        isOnline.Setup(check => check(2)).Returns(false);

        string message = GroupInfoPolicy.Build(group, isOnline.Object);

        Assert.Equal("Group members:\nLeader - Connected - Group Leader\n" +
                     "Member - Disconnected", message);
        isOnline.Verify(check => check(1), Times.Once);
        isOnline.Verify(check => check(2), Times.Once);
    }
}

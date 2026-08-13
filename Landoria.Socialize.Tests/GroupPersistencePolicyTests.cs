using System.Collections.Generic;
using Xunit;

namespace Landoria.Socialize;

public sealed class GroupPersistencePolicyTests
{
    // Verifies that persistent group data restores identifiers, leaders, names, and membership lookup.
    [Fact]
    public void CapturedGroupsRoundTripWithoutLosingState()
    {
        SocialGroup first = Group(4, 10, (10, "Alice"), (11, "Bob"));
        SocialGroup second = Group(8, 20, (20, "Cora"), (21, "Dane"));
        List<PersistedGroup> snapshot = GroupPersistencePolicy.Capture([first, second]);
        Dictionary<int, SocialGroup> groups = new();
        Dictionary<long, int> memberships = new();

        GroupPersistencePolicy.Restore(snapshot, groups, memberships);

        Assert.Equal(2, groups.Count);
        Assert.Equal(10, groups[4].Leader);
        Assert.Equal("Bob", groups[4].Members[11]);
        Assert.Equal(8, memberships[21]);
    }

    // Verifies that a captured snapshot is independent from later live-state changes.
    [Fact]
    public void CapturedGroupIsADeepSnapshot()
    {
        SocialGroup group = Group(4, 10, (10, "Alice"), (11, "Bob"));

        List<PersistedGroup> snapshot = GroupPersistencePolicy.Capture([group]);
        group.Members[11] = "Changed";

        Assert.Equal("Bob", snapshot[0].Members[11]);
    }

    // Verifies that restoring empty persistent data clears stale server state.
    [Fact]
    public void EmptySnapshotClearsStaleGroupsAndMemberships()
    {
        Dictionary<int, SocialGroup> groups = new() { [1] = Group(1, 1, (1, "Old")) };
        Dictionary<long, int> memberships = new() { [1] = 1 };

        GroupPersistencePolicy.Restore([], groups, memberships);

        Assert.Empty(groups);
        Assert.Empty(memberships);
    }

    private static SocialGroup Group(
        int id, long leader, params (long Id, string Name)[] members)
    {
        SocialGroup group = new() { Id = id, Leader = leader };
        foreach ((long memberId, string name) in members) group.Members[memberId] = name;
        return group;
    }
}

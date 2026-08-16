using Xunit;

namespace Landoria.ModSentry;

public sealed class PolicyValidatorTests
{
    // Verifies that exact required and optional inventories are admitted.
    [Fact]
    public void ExactInventoryIsAccepted()
    {
        PluginPolicy policy = Policy(Required("Core"), Optional("Map"));

        ValidationResult result = PolicyValidator.Validate(policy,
            new[] { Required("Core"), Optional("Map") });

        Assert.True(result.Accepted);
        Assert.Equal(string.Empty, result.PlayerMessage);
        Assert.Equal("Client plugin inventory accepted.", result.TechnicalMessage);
    }

    // Verifies that an absent optional mod does not reject the client.
    [Fact]
    public void MissingOptionalPluginIsAccepted()
    {
        ValidationResult result = PolicyValidator.Validate(
            Policy(Required("Core"), Optional("Map")), new[] { Required("Core") });

        Assert.True(result.Accepted);
    }

    // Verifies that optional libraries that are not plugins do not reject the client.
    [Fact]
    public void LibraryPluginInOptionalPolicyIsAccepted()
    {
        ValidationResult result = PolicyValidator.Validate(
            Policy(optional: Library("SharedLib")), Array.Empty<PluginDescriptor>());

        Assert.True(result.Accepted);
        Assert.Equal(string.Empty, result.PlayerMessage);
    }

    // Verifies that required libraries without plugin metadata do not reject the client.
    [Fact]
    public void LibraryPluginInRequiredPolicyIsIgnored()
    {
        ValidationResult result = PolicyValidator.Validate(
            Policy(required: Library("SharedLib")), Array.Empty<PluginDescriptor>());

        Assert.True(result.Accepted);
        Assert.Equal(string.Empty, result.PlayerMessage);
    }

    // Verifies the player and technical messages for a missing required mod.
    [Fact]
    public void MissingRequiredPluginIsRejected()
    {
        ValidationResult result = PolicyValidator.Validate(
            Policy(Required("Core")), Array.Empty<PluginDescriptor>());

        Assert.False(result.Accepted);
        Assert.Equal("Required mod missing: Core 1.2.3.", result.PlayerMessage);
        Assert.Equal("Required plugin mod.core 1.2.3 is missing.", result.TechnicalMessage);
    }

    // Verifies that required and optional version mismatches use their specific messages.
    [Theory]
    [InlineData(false, "Mod update required: Core 1.2.3.", "required")]
    [InlineData(true, "Optional mod mismatch: Core 1.2.3.", "optional")]
    public void VersionMismatchIsRejected(bool optional, string playerMessage, string kind)
    {
        PluginDescriptor expected = Required("Core");
        PluginPolicy policy = optional ? Policy(optional: expected) : Policy(expected);

        ValidationResult result = PolicyValidator.Validate(policy,
            new[] { Descriptor("mod.core", "Core", "9.9.9", HashA) });

        Assert.False(result.Accepted);
        Assert.Equal(playerMessage, result.PlayerMessage);
        Assert.Contains($"{kind} plugin mod.core version mismatch", result.TechnicalMessage);
    }

    // Verifies that hashes are compared without case sensitivity.
    [Fact]
    public void HashComparisonIsCaseInsensitive()
    {
        PluginDescriptor expected = Descriptor("mod.core", "Core", "1.2.3", HashA.ToLowerInvariant());

        ValidationResult result = PolicyValidator.Validate(
            Policy(expected), new[] { Required("Core") });

        Assert.True(result.Accepted);
    }

    // Verifies that a changed binary hash rejects an otherwise matching mod.
    [Theory]
    [InlineData(false, "required")]
    [InlineData(true, "optional")]
    public void HashMismatchIsRejected(bool optional, string kind)
    {
        PluginDescriptor expected = Required("Core");
        PluginPolicy policy = optional ? Policy(optional: expected) : Policy(expected);

        ValidationResult result = PolicyValidator.Validate(policy,
            new[] { Descriptor("mod.core", "Core", "1.2.3", HashB) });

        Assert.False(result.Accepted);
        Assert.Equal("Mod mismatch: Core 1.2.3.", result.PlayerMessage);
        Assert.Contains($"{kind} plugin mod.core SHA-256 mismatch", result.TechnicalMessage);
    }

    // Verifies that a client mod outside both policy lists is rejected.
    [Fact]
    public void UnexpectedPluginIsRejected()
    {
        PluginDescriptor unexpected = Descriptor("mod.cheat", "Cheat", "1.0.0", HashB);

        ValidationResult result = PolicyValidator.Validate(
            Policy(), new[] { unexpected });

        Assert.False(result.Accepted);
        Assert.Equal("Unsupported mod detected: Cheat. Remove it before reconnecting.",
            result.PlayerMessage);
        Assert.Contains("Unexpected plugin mod.cheat 1.0.0", result.TechnicalMessage);
    }

    // Verifies that duplicate GUIDs are rejected in every inventory source.
    [Theory]
    [InlineData("client")]
    [InlineData("required")]
    [InlineData("optional")]
    public void DuplicateGuidIsRejected(string source)
    {
        PluginDescriptor plugin = Required("Core");
        PluginPolicy policy = source switch
        {
            "required" => new PluginPolicy(new[] { plugin, plugin }, Array.Empty<PluginDescriptor>()),
            "optional" => new PluginPolicy(Array.Empty<PluginDescriptor>(), new[] { plugin, plugin }),
            _ => Policy()
        };
        IReadOnlyList<PluginDescriptor> actual = source == "client"
            ? new[] { plugin, plugin }
            : Array.Empty<PluginDescriptor>();

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => PolicyValidator.Validate(policy, actual));

        Assert.Contains($"Duplicate plugin GUID in {source} inventory.", error.Message);
    }

    private const string HashA = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private const string HashB = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";

    private static PluginPolicy Policy(PluginDescriptor required = null,
        PluginDescriptor optional = null)
    {
        return new PluginPolicy(required == null ? Array.Empty<PluginDescriptor>() : new[] { required },
            optional == null ? Array.Empty<PluginDescriptor>() : new[] { optional });
    }

    private static PluginDescriptor Required(string name)
    {
        return Descriptor("mod." + name.ToLowerInvariant(), name, "1.2.3", HashA);
    }

    private static PluginDescriptor Optional(string name)
    {
        return Descriptor("mod." + name.ToLowerInvariant(), name, "2.0.0", HashB);
    }

    private static PluginDescriptor Library(string name)
    {
        return Descriptor("library." + name.ToLowerInvariant(), name, "3.0.0", HashA, false);
    }

    private static PluginDescriptor Descriptor(string guid, string name, string version, string hash,
        bool isBepInPlugin = true)
    {
        return new PluginDescriptor(guid, name, version, hash, isBepInPlugin);
    }
}

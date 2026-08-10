using System;
using System.Collections.Generic;
using System.Linq;

namespace Landoria.ModSentry
{
    internal static class PolicyValidator
    {
        internal static ValidationResult Validate(PluginPolicy policy,
            IReadOnlyList<PluginDescriptor> actual)
        {
            Dictionary<string, PluginDescriptor> installed = ToDictionary(actual, "client");
            Dictionary<string, PluginDescriptor> required = ToDictionary(policy.Required, "required");
            Dictionary<string, PluginDescriptor> optional = ToDictionary(policy.Optional, "optional");

            foreach (PluginDescriptor expected in policy.Required)
            {
                if (!installed.TryGetValue(expected.Guid, out PluginDescriptor found))
                {
                    return Missing(expected);
                }

                ValidationResult mismatch = Compare(expected, found, false);
                if (mismatch != null)
                {
                    return mismatch;
                }
            }

            foreach (PluginDescriptor found in actual.OrderBy(item => item.Guid, StringComparer.Ordinal))
            {
                if (required.ContainsKey(found.Guid))
                {
                    continue;
                }

                if (!optional.TryGetValue(found.Guid, out PluginDescriptor expected))
                {
                    return Unexpected(found);
                }

                ValidationResult mismatch = Compare(expected, found, true);
                if (mismatch != null)
                {
                    return mismatch;
                }
            }

            return ValidationResult.Accept();
        }

        private static Dictionary<string, PluginDescriptor> ToDictionary(
            IEnumerable<PluginDescriptor> plugins, string source)
        {
            try
            {
                return plugins.ToDictionary(item => item.Guid, StringComparer.Ordinal);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"Duplicate plugin GUID in {source} inventory.", exception);
            }
        }

        private static ValidationResult Compare(PluginDescriptor expected,
            PluginDescriptor actual, bool optional)
        {
            string kind = optional ? "optional" : "required";
            if (!string.Equals(expected.Version, actual.Version, StringComparison.Ordinal))
            {
                return ValidationResult.Reject(
                    optional
                        ? $"An optional mod is not compatible: {expected.Name} (expected {expected.Version})."
                        : $"A mod needs to be updated: {expected.Name} (expected {expected.Version}).",
                    $"{kind} plugin {expected.Guid} version mismatch: expected {expected.Version}, received {actual.Version}.");
            }

            if (!string.Equals(expected.Hash, actual.Hash, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Reject(
                    $"A mod installation is not compatible: {expected.Name} {expected.Version}.",
                    $"{kind} plugin {expected.Guid} SHA-256 mismatch: expected {expected.Hash}, received {actual.Hash}.");
            }

            return null;
        }

        private static ValidationResult Missing(PluginDescriptor expected)
        {
            return ValidationResult.Reject(
                $"A required mod is missing: {expected.Name} {expected.Version}.",
                $"Required plugin {expected.Guid} {expected.Version} is missing.");
        }

        private static ValidationResult Unexpected(PluginDescriptor actual)
        {
            return ValidationResult.Reject(
                $"An unexpected mod must be removed: {actual.Name}.",
                $"Unexpected plugin {actual.Guid} {actual.Version} with SHA-256 {actual.Hash}.");
        }
    }
}

using System.Globalization;
using System.Resources;

namespace Landoria.ModSentry
{
    internal static class GuestAdmissionMessages
    {
        private static readonly ResourceManager Resources = new ResourceManager(
            "Landoria.ModSentry.GuestAdmissionMessages",
            typeof(GuestAdmissionMessages).Assembly);

        internal static string DefaultRegistration => Get("DefaultRegistration");
        internal static string Sender => Get("Sender");

        internal static string Countdown(string message, int seconds)
        {
            return string.Format(CultureInfo.CurrentCulture, Get("Countdown"), message, seconds);
        }

        private static string Get(string key)
        {
            return Resources.GetString(key, CultureInfo.CurrentUICulture);
        }
    }
}

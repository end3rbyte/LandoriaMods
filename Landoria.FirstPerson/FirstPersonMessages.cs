using System.Globalization;
using System.Resources;

namespace Landoria.FirstPerson
{
    internal static class FirstPersonMessages
    {
        private static readonly ResourceManager Resources = new ResourceManager(
            "Landoria.FirstPerson.FirstPersonMessages",
            typeof(FirstPersonMessages).Assembly);

        internal static string FieldOfViewAboveMaximum => string.Format(
            CultureInfo.CurrentCulture,
            Resources.GetString("FieldOfViewAboveMaximum", CultureInfo.CurrentUICulture),
            FirstPersonPolicy.MaximumFieldOfView);
    }
}

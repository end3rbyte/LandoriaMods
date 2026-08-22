using System;
using System.Security;

namespace Landoria.LandoriaModPack
{
    internal sealed class WarningPopupExtended : WarningPopup
    {
        private const string LinkColor = "#5AA9FF";

        internal WarningPopupExtended(string header, string content,
            PopupButtonCallback okCallback)
            : base(header, content, okCallback, localizeText: false)
        {
        }

        internal static string Link(string label, string url)
        {
            if (!IsSupportedUrl(url))
            {
                throw new ArgumentException("Popup links must use HTTP or HTTPS.", nameof(url));
            }
            return "<link=\"" + SecurityElement.Escape(url) + "\"><color=" +
                LinkColor + "><u>" + SecurityElement.Escape(label) +
                "</u></color></link>";
        }

        internal static bool IsSupportedUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri parsed) &&
                (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
        }
    }
}

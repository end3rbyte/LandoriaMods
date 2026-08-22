using HarmonyLib;
using Splatform;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.LandoriaModPack
{
    [HarmonyPatch(typeof(UnifiedPopup), "ShowWarning")]
    internal static class ConfigureWarningPopupLinkPatch
    {
        private static void Postfix(WarningPopup popup, TextMeshProUGUI ___bodyText)
        {
            PopupLinkHandler handler =
                ___bodyText.GetComponent<PopupLinkHandler>() ??
                ___bodyText.gameObject.AddComponent<PopupLinkHandler>();
            handler.Configure(popup is WarningPopupExtended);
        }
    }

    internal sealed class PopupLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        private TMP_Text _text;
        private bool _defaultRaycastTarget;
        private bool _initialized;

        private void Awake()
        {
            InitializeText();
        }

        internal void Configure(bool acceptsLinks)
        {
            InitializeText();
            _text.raycastTarget = acceptsLinks || _defaultRaycastTarget;
            enabled = acceptsLinks;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(
                _text, eventData.position, eventData.pressEventCamera);
            if (linkIndex < 0)
            {
                return;
            }
            string url = _text.textInfo.linkInfo[linkIndex].GetLinkID();
            if (WarningPopupExtended.IsSupportedUrl(url))
            {
                OpenBrowser(url);
            }
        }

        private static void OpenBrowser(string url)
        {
            if (PlatformManager.DistributionPlatform.UIProvider.WebBrowser != null)
            {
                PlatformManager.DistributionPlatform.UIProvider.WebBrowser.Open(url);
                return;
            }
            Application.OpenURL(url);
        }

        private void InitializeText()
        {
            if (_initialized)
            {
                return;
            }
            _text = GetComponent<TMP_Text>();
            _defaultRaycastTarget = _text.raycastTarget;
            _initialized = true;
        }
    }
}

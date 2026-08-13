using System.Collections;
using TMPro;
using UnityEngine;

namespace Landoria.CharacterVault
{
    internal sealed class CharacterSaveStatusDisplay
    {
        private const float StatusDisplaySeconds = 3f;
        private TextMeshProUGUI _label;
        private int _stateVersion;

        internal void Attach(Minimap minimap)
        {
            if (_label != null || minimap?.m_mapImageSmall == null ||
                minimap.m_biomeNameSmall == null)
            {
                return;
            }

            GameObject target = new GameObject("CharacterVaultSaveStatus", typeof(RectTransform));
            target.transform.SetParent(minimap.m_smallRoot.transform, false);
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(480f, 30f);
            RectTransform mapRect = minimap.m_mapImageSmall.rectTransform;
            rect.position = mapRect.TransformPoint(
                new Vector3(mapRect.rect.center.x, mapRect.rect.yMin - 8f, 0f));
            rect.SetAsLastSibling();

            _label = target.AddComponent<TextMeshProUGUI>();
            _label.font = minimap.m_biomeNameSmall.font;
            _label.fontSharedMaterial = minimap.m_biomeNameSmall.fontSharedMaterial;
            _label.fontSize = minimap.m_biomeNameSmall.fontSize;
            _label.fontSizeMax = minimap.m_biomeNameSmall.fontSize;
            _label.fontSizeMin = 8f;
            _label.enableAutoSizing = true;
            _label.textWrappingMode = TextWrappingModes.NoWrap;
            _label.fontStyle = FontStyles.Bold;
            _label.alignment = TextAlignmentOptions.Center;
            _label.color = Color.white;
            _label.raycastTarget = false;
            _label.text = string.Empty;
            target.SetActive(false);
        }

        internal void ShowSaving()
        {
            ShowForThreeSeconds("Saving character...");
        }

        internal void ShowAccepted()
        {
            ShowForThreeSeconds("Saving character......");
        }

        internal void ShowCommitted()
        {
            ShowForThreeSeconds("Character saved");
        }

        internal void Hide()
        {
            _stateVersion++;
            if (_label != null)
            {
                _label.gameObject.SetActive(false);
            }
        }

        internal void Dispose()
        {
            _stateVersion++;
            if (_label != null)
            {
                Object.Destroy(_label.gameObject);
                _label = null;
            }
        }

        private void ShowForThreeSeconds(string message)
        {
            _stateVersion++;
            Attach(Minimap.instance);
            if (_label == null)
            {
                return;
            }

            _label.text = message;
            _label.gameObject.SetActive(true);
            int version = _stateVersion;
            CharacterVaultPlugin.Instance?.Run(HideAfterDelay(version));
        }

        private IEnumerator HideAfterDelay(int version)
        {
            yield return new WaitForSecondsRealtime(StatusDisplaySeconds);
            if (_stateVersion == version)
            {
                Hide();
            }
        }
    }
}

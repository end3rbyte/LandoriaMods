using System.Collections;
using TMPro;
using UnityEngine;

namespace Landoria.CharacterVault
{
    internal sealed class CharacterSaveStatusDisplay
    {
        private const float ActiveDisplaySeconds = 30f;
        private const float CommitTimeoutSeconds = 20f;
        private const float ResultDisplaySeconds = 3f;
        private TextMeshProUGUI _label;
        private string _requestId;
        private bool _waitingForCommit;
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

        internal void ShowSaving(string requestId)
        {
            Show(requestId, "Saving character...", ActiveDisplaySeconds, false);
        }

        internal void ShowAccepted(string requestId)
        {
            Show(requestId, "Saving character......", ActiveDisplaySeconds, true);
            int version = _stateVersion;
            CharacterVaultPlugin.Instance?.Run(FailWithoutCommit(requestId, version));
        }

        internal void ShowCommitted(string requestId)
        {
            if (_requestId == requestId && _waitingForCommit)
            {
                Show(requestId, "Character saved", ResultDisplaySeconds, false);
            }
        }

        internal void Hide()
        {
            _stateVersion++;
            _requestId = null;
            _waitingForCommit = false;
            if (_label != null)
            {
                _label.gameObject.SetActive(false);
            }
        }

        internal void Dispose()
        {
            _stateVersion++;
            _requestId = null;
            _waitingForCommit = false;
            if (_label != null)
            {
                Object.Destroy(_label.gameObject);
                _label = null;
            }
        }

        private void Show(string requestId, string message, float duration, bool waitingForCommit)
        {
            _stateVersion++;
            _requestId = requestId;
            _waitingForCommit = waitingForCommit;
            Attach(Minimap.instance);
            if (_label == null)
            {
                return;
            }

            _label.text = message;
            _label.gameObject.SetActive(true);
            int version = _stateVersion;
            CharacterVaultPlugin.Instance?.Run(HideAfterDelay(version, duration));
        }

        private IEnumerator FailWithoutCommit(string requestId, int version)
        {
            yield return new WaitForSecondsRealtime(CommitTimeoutSeconds);
            if (_stateVersion == version && _requestId == requestId && _waitingForCommit)
            {
                Show(requestId, "Failed", ResultDisplaySeconds, false);
            }
        }

        private IEnumerator HideAfterDelay(int version, float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            if (_stateVersion == version)
            {
                Hide();
            }
        }
    }
}

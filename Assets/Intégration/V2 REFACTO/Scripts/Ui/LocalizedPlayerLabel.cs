using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Intégration.V2_REFACTO.Scripts
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedPlayerLabel : MonoBehaviour
    {
        private const string TableName = "Translate";
        private const string PlayerPrefixKey = "Player Prefix";
        private const string AILabelKey = "AI Label";

        private TMP_Text _text;
        private int _playerIndex;
        private bool _initialized;
        private int _requestId;

        public static void Apply(TMP_Text text, int playerIndex)
        {
            if (text == null) return;
            if (!text.TryGetComponent(out LocalizedPlayerLabel label))
                label = text.gameObject.AddComponent<LocalizedPlayerLabel>();

            label._text = text;
            label._playerIndex = playerIndex;
            label._initialized = true;
            label.Refresh();
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            if (_initialized) Refresh();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale locale) => Refresh();

        private void Refresh()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
            bool isAI = _playerIndex < 0;
            int request = ++_requestId;

            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TableName, isAI ? AILabelKey : PlayerPrefixKey);
            if (handle.IsDone)
            {
                SetText(handle.Result);
                return;
            }

            handle.Completed += operation =>
            {
                if (this != null && request == _requestId) SetText(operation.Result);
            };
        }

        private void SetText(string localized)
        {
            if (_text == null || string.IsNullOrEmpty(localized)) return;
            _text.text = _playerIndex < 0 ? localized : localized + (_playerIndex + 1);
        }
    }
}

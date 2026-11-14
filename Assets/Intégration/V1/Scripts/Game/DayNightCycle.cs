using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Intégration.V1.Scripts.Game
{
    public class DayNightCycle : MonoBehaviour
    {
        public float DayDuration = 120f;
        public float SunsetDuration = 30f;
        public float NightTransitionDuration = 8f;
        public Color SunsetColor = new Color(1f, 0.6f, 0.35f);
        public Color NightColor = new Color(0.45f, 0.55f, 1f);
        [Range(0f, 1f)] public float NightIntensityRatio = 0.3f;
        public float MessageDuration = 3f;

        public event Action OnNightFall;
        public bool IsNight { get; private set; }

        private float _elapsed;
        private bool _sunsetStarted;
        private NightTransition _nightTransition;
        private readonly List<Light> _sunLights = new();
        private readonly List<Color> _baseColors = new();
        private readonly List<float> _baseIntensities = new();
        private string _nightText;

        private void Start()
        {
            _nightTransition = FindFirstObjectByType<NightTransition>();
            Light nightLight = _nightTransition != null ? _nightTransition.NightLight : null;

            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type != LightType.Directional || !light.enabled || light == nightLight) continue;
                _sunLights.Add(light);
                _baseColors.Add(light.color);
                _baseIntensities.Add(light.intensity);
            }

            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Translate", "Night Falls");
            handle.Completed += operation =>
            {
                if (this != null && !string.IsNullOrEmpty(operation.Result)) _nightText = operation.Result;
            };
        }

        private void Update()
        {
            GameManager gameManager = GameManager.Instance;
            if (IsNight || gameManager == null || gameManager.GameFinished) return;

            _elapsed += Time.deltaTime;

            if (!_sunsetStarted && _elapsed >= DayDuration - SunsetDuration) StartSunset();
            if (_elapsed >= DayDuration) StartNight();
        }

        private void StartSunset()
        {
            _sunsetStarted = true;
            float duration = Mathf.Max(0.1f, DayDuration - _elapsed);

            for (int i = 0; i < _sunLights.Count; i++)
            {
                Light light = _sunLights[i];
                Color target = Color.Lerp(_baseColors[i], SunsetColor, 0.7f);
                DOTween.To(() => light.color, c => light.color = c, target, duration).SetTarget(light);
            }
        }

        private void StartNight()
        {
            IsNight = true;
            OnNightFall?.Invoke();
            ShowNightMessage();

            if (_nightTransition != null)
            {
                _nightTransition.transitionDuration = NightTransitionDuration;
                _nightTransition.Transition();
                return;
            }

            for (int i = 0; i < _sunLights.Count; i++)
            {
                Light light = _sunLights[i];
                DOTween.To(() => light.color, c => light.color = c, NightColor, NightTransitionDuration).SetTarget(light);
                DOTween.To(() => light.intensity, v => light.intensity = v, _baseIntensities[i] * NightIntensityRatio,
                    NightTransitionDuration).SetTarget(light);
            }

            float ambient = RenderSettings.ambientIntensity;
            DOTween.To(() => RenderSettings.ambientIntensity, v => RenderSettings.ambientIntensity = v,
                ambient * 0.5f, NightTransitionDuration);
        }

        private void ShowNightMessage()
        {
            CountDown countDown = FindFirstObjectByType<CountDown>(FindObjectsInactive.Include);
            TextMeshProUGUI text = countDown != null ? countDown.Text : null;
            if (text == null) return;

            string fallback = LocalizationSettings.SelectedLocale != null &&
                              LocalizationSettings.SelectedLocale.Identifier.Code == "fr"
                ? "LA NUIT TOMBE..."
                : "NIGHT IS FALLING...";

            text.gameObject.SetActive(true);
            text.text = string.IsNullOrEmpty(_nightText) ? fallback : _nightText;
            text.alpha = 1f;
            text.transform.localScale = Vector3.one * 0.6f;

            DOTween.Sequence()
                .Append(text.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack))
                .AppendInterval(MessageDuration)
                .Append(DOTween.To(() => text.alpha, a => text.alpha = a, 0f, 1f))
                .OnComplete(() =>
                {
                    if (text == null) return;
                    text.gameObject.SetActive(false);
                    text.alpha = 1f;
                });
        }
    }
}

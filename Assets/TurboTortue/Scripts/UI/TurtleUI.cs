using System.Collections.Generic;
using DG.Tweening;
using Michael.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurtleUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider batteryBar;
    [SerializeField] private Image batteryFillImage;
    [SerializeField] private Slider easeBatteryBar;

    [Header("UI Elements")]
    [SerializeField] private Image turtleIcon;
    [SerializeField] private Image turtleOutline;
    [SerializeField] private TextMeshProUGUI batteryText;
    [SerializeField] private List<Sprite> _turtlesIcons;
    [SerializeField] private List<Color> _turtleStateColors;

    [Header("Battery Logic")]
    [SerializeField] private BatteryManager batteryManager;
    [SerializeField] private float easeSliderRate = 3f;
    [SerializeField] private float ColorBarRate = 3f;

    private float delayedBatteryTime;
    private Color currentTargetColor;
    private Tween blinkTween;
    private Tween fillColorTween;
    private Color fillTargetColor;
    private bool hasFillTarget;
    private bool isBatteryCritical;

    private void Start()
    {
        delayedBatteryTime = batteryManager.CurrentBatteryTime;
    }

    private void Update()
    {
        float current = batteryManager.CurrentBatteryTime;
        float max = batteryManager.MaxBatteryTime;
        float percent = current / max * 100f;

        bool wasCritical = isBatteryCritical;
        isBatteryCritical = percent < 25f;

        batteryBar.value = current / max;

        delayedBatteryTime = Mathf.Lerp(delayedBatteryTime, current, Time.deltaTime * easeSliderRate);
        easeBatteryBar.value = delayedBatteryTime / max;

        batteryText.text = Mathf.RoundToInt(percent) + "%";

        if (percent <= 25)
        {
            turtleIcon.sprite = _turtlesIcons[2];
            currentTargetColor = _turtleStateColors[2];
        }
        else if (percent is <= 50 and > 25)
        {
            turtleIcon.sprite = _turtlesIcons[1];
            currentTargetColor = _turtleStateColors[1];
        }
        else
        {
            turtleIcon.sprite = _turtlesIcons[0];
            currentTargetColor = _turtleStateColors[0];
        }

        if (!hasFillTarget || fillTargetColor != currentTargetColor)
        {
            hasFillTarget = true;
            fillTargetColor = currentTargetColor;
            fillColorTween?.Kill();
            fillColorTween = AnimateUIColor(batteryFillImage, currentTargetColor, ColorBarRate);
        }

        if (isBatteryCritical && !wasCritical && GameManager.Instance.GameisStarted && !GameManager.Instance.GameFinished)
        {
            StartBlinkingIcon();
        }
        else if (!isBatteryCritical && wasCritical)
        {
            StopBlinkingIcon();
        }
    }

    private Tween AnimateUIColor(Image targetImage, Color toColor, float duration)
    {
        return targetImage.DOColor(toColor, duration).SetEase(Ease.InOutSine);
    }

    private void StartBlinkingIcon()
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleBatteryWarning);
        AnimateUIColor(turtleOutline, currentTargetColor,ColorBarRate);
        blinkTween?.Kill();
        blinkTween = turtleOutline.DOFade(0f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopBlinkingIcon()
    {
        turtleOutline.color = Color.white;
        blinkTween?.Kill();
        turtleOutline.DOFade(1f, 0.2f);
    }
}

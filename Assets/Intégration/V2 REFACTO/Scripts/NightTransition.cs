using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NightTransition : MonoBehaviour
{

    public float transitionDuration = 120f;
    UnityEngine.Rendering.ProbeReferenceVolume probeRefVolume;
    [SerializeField] Light _dayLight;
    [SerializeField] Light _nightLight;
    public string scenario01 = "Day";
    public string scenario02 = "Night";
    [Range(0, 1)] public float blendingFactor = 0.5f;
    [Min(1)] public int numberOfCellsBlendedPerFrame = 10;

    private Tween currentTween;

    [Header("Intensity Settings")] public float dayLightIntensityDay = 1.5f;
    public float dayLightIntensityNight = 0f;
    public float nightLightIntensityDay = 0f;
    public float nightLightIntensityNight = 3f;

    void Start()
    {
        probeRefVolume = UnityEngine.Rendering.ProbeReferenceVolume.instance;
        probeRefVolume.lightingScenario = scenario01;
        probeRefVolume.numberOfCellsBlendedPerFrame = numberOfCellsBlendedPerFrame;
    }

    void Update()
    {
        probeRefVolume.BlendLightingScenario(scenario02, blendingFactor);

        probeRefVolume.BlendLightingScenario(scenario02, blendingFactor);

        if (_dayLight != null && _nightLight != null)
        {
            _dayLight.intensity = Mathf.Lerp(dayLightIntensityDay, dayLightIntensityNight, blendingFactor);
            _nightLight.intensity = Mathf.Lerp(nightLightIntensityDay, nightLightIntensityNight, blendingFactor);
        }
    }

    [ContextMenu("start transition")]
    public void Transition()
    {
        currentTween = DOTween.To(() => blendingFactor, x => blendingFactor = x, 1, transitionDuration)
            .SetEase(Ease.InOutSine);
    }

}

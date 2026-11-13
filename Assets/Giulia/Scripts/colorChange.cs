using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Michael.Scripts.Manager;

public class colorChange : MonoBehaviour
{
    public Color emissiveColor;
    public float emissiveIntensity = 1.0f;
    public float fadeDuration = 1.0f;
    public List<Material> materialsToUpdate;
    public List<ParticleSystem> particlesToUpdate;

    public void FadeEmissiveColor()
    {
        StartCoroutine(FadeEmissiveCoroutine());
    }

    private IEnumerator FadeEmissiveCoroutine()
    {
        Color finalEmissiveColor = emissiveColor * emissiveIntensity;

        float step = 0.0f;
        while (step < 1.0f)
        {
            foreach (Material material in materialsToUpdate)
            {
                if (material != null && material.HasProperty("_EmissionColor"))
                {
                    Color currentEmissiveColor = material.GetColor("_EmissionColor");
                    Color lerpedColor = Color.Lerp(currentEmissiveColor, finalEmissiveColor, step);
                    material.SetColor("_EmissionColor", lerpedColor);
                }
            }

            step += Time.deltaTime / fadeDuration;

            yield return null;
        }

        foreach (Material material in materialsToUpdate)
        {
            if (material != null && material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", finalEmissiveColor);
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Michael.Scripts.Manager;

public class Dissolve : MonoBehaviour
{
    public string propertyName = "_MyProperty";
    public float startValue = -0.2f;
    public float endValue = 1f;
    public float fadeDuration = 1.0f;
    public List<Material> materialsToUpdate;

    private void OnEnable()
    {
        StartCoroutine(FadePropertyCoroutine());
    }

    private IEnumerator FadePropertyCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / fadeDuration);
            foreach (Material material in materialsToUpdate)
            {
                if (material != null && material.HasProperty(propertyName))
                {
                    material.SetFloat(propertyName, currentValue);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (Material material in materialsToUpdate)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, endValue);
            }
        }
    }
}

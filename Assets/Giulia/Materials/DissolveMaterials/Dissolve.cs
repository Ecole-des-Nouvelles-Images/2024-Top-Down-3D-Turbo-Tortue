using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Michael.Scripts.Manager;

public class Dissolve : MonoBehaviour
{
    public string propertyName = "_MyProperty"; // Nom de la propriété du shader à modifier
    public float startValue = -0.2f; // Valeur de départ
    public float endValue = 1f; // Valeur de fin
    public float fadeDuration = 1.0f; // Durée du fondu en secondes
    public List<Material> materialsToUpdate;

    private void OnEnable()
    {
        // Démarre une coroutine pour le fondu de la propriété du shader
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

        // Assure que la valeur finale est définie après la boucle
        foreach (Material material in materialsToUpdate)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, endValue);
            }
        }
    }
}
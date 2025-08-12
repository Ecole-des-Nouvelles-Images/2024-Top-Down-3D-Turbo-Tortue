using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Michael.Scripts.Manager;

public class colorChange : MonoBehaviour
{
    public Color emissiveColor;
    public float emissiveIntensity = 1.0f; // Intensité de la couleur émissive
    public float fadeDuration = 1.0f; // Durée du fondu en secondes
    public List<Material> materialsToUpdate;
    public List<ParticleSystem> particlesToUpdate;

    // Méthode pour déclencher le fondu de la couleur émissive
    public void FadeEmissiveColor()
    {
        // Démarre une coroutine pour le fondu de couleur
        StartCoroutine(FadeEmissiveCoroutine());
    }

    // Coroutine pour le fondu de couleur émissive
    private IEnumerator FadeEmissiveCoroutine()
    {
        // Calcule la couleur émissive finale avec l'intensité spécifiée
        Color finalEmissiveColor = emissiveColor * emissiveIntensity;

        // Calcule le pas de changement de couleur par étape
        float step = 0.0f;
        while (step < 1.0f)
        {
            // Interpole progressivement entre la couleur actuelle et la couleur finale
            foreach (Material material in materialsToUpdate)
            {
                if (material != null && material.HasProperty("_EmissionColor"))
                {
                    Color currentEmissiveColor = material.GetColor("_EmissionColor");
                    Color lerpedColor = Color.Lerp(currentEmissiveColor, finalEmissiveColor, step);
                    material.SetColor("_EmissionColor", lerpedColor);
                }
            }

            // Incrémente le pas de changement de couleur
            step += Time.deltaTime / fadeDuration;

            // Attend la fin du frame avant la prochaine itération
            yield return null;
        }

        // Assure que la couleur finale est définie après la boucle
        foreach (Material material in materialsToUpdate)
        {
            if (material != null && material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", finalEmissiveColor);
            }
        }
    }
}

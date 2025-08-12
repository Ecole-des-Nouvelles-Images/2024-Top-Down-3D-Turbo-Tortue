using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    void Start()
    {
        // Boucle à travers tous les enfants de l'objet auquel ce script est attaché
        foreach (Transform child in transform)
        {
            // Génère une rotation aléatoire autour de l'axe Y
            float randomYRotation = Random.Range(0f, 360f);
            child.rotation = Quaternion.Euler(0, randomYRotation, 0);
        }
    }
}

using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    void Start()
    {
        foreach (Transform child in transform)
        {
            float randomYRotation = Random.Range(0f, 360f);
            child.rotation = Quaternion.Euler(0, randomYRotation, 0);
        }
    }
}

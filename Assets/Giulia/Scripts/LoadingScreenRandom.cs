using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenRandom : MonoBehaviour
{
    public Sprite[] sprites;
    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        if (sprites.Length > 0)
        {
            img.sprite = sprites[Random.Range(0, sprites.Length)];
        }
    }
}

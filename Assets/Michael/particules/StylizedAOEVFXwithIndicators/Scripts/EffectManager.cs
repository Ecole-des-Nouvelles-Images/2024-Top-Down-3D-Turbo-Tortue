using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EffectSceneManager
{
public class EffectManager : MonoBehaviour
{
    public List<GameObject> effects;
    public List<KeyCode> keys;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                Instantiate<GameObject>(effects[i]);
            }
        }
    }
}
}

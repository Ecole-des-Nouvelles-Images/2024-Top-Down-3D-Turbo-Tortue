using System.Collections;
using System.Collections.Generic;
using Michael.Scripts.Controller;
using UnityEngine;
using UnityEngine.VFX;

namespace VFXSelfDestroy 
{

public class SelfDestroyEffect : MonoBehaviour
{
    private VisualEffect effect;
    private bool effectPlayed = false;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("ShowTurtle",1.4f);
        effect = gameObject.GetComponent<VisualEffect>();
        effect.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(effect.aliveParticleCount > 0 && !effectPlayed)
        {
            effectPlayed = true;
        }

        if(effect.aliveParticleCount == 0 && effectPlayed)
        {
            Destroy(gameObject);
        }
        
    }

  
}
}

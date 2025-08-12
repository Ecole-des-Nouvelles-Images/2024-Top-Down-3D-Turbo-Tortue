using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering.Universal;

namespace PlayDecalVFX
{
public class PlayVFX : MonoBehaviour
{
    private DecalProjector decal;
    private Material mat;
    private VisualEffect effect;

    private float anticipation;
    private float dissapation;
    private AnimationCurve showIndicatorCurve;
    private AnimationCurve dissolveCurve;
    private float startTime;
    private float lifetime;

    private bool effectPlayed = false;
    // Start is called before the first frame update
    void Start()
    {
        //Assign variables
        effect = gameObject.GetComponent<VisualEffect>();
        effect.Play();
        decal = gameObject.GetComponentInChildren<DecalProjector>();
        mat = decal.material;
        anticipation = effect.GetFloat("Anticipation");
        dissapation = effect.GetFloat("Dissapation");
        startTime = Time.time;
        showIndicatorCurve = effect.GetAnimationCurve("IndicatorCurve");
        dissolveCurve = effect.GetAnimationCurve("DissolveCurve");

        //Apply properties to decal shader
        mat.SetTexture("_Texture2D", effect.GetTexture("IndicatorTexture"));
        mat.SetColor("_BrightColor", effect.GetVector4("BrightColor"));
        mat.SetColor("_DarkColor", effect.GetVector4("DarkColor"));
        mat.SetInt("_UseLUT", effect.GetBool("UseLUT") ? 1 : 0);
        mat.SetTexture("_LUT", effect.GetTexture("LUT"));

        //Apply diameter to decal
        decal.size = new Vector3(effect.GetFloat("Diameter") * 1.15f, effect.GetFloat("Diameter") * 1.15f, effect.GetFloat("Diameter") * 1.15f);

    }

    // Update is called once per frame
    void Update()
    {
        //calc lifetime depending on effect
        if(effect.name == "PlantHealDecalPrefab(Clone)")
        {
            lifetime = (Time.time - startTime) * (1 / (anticipation + dissapation));
        }
        else
        {
            lifetime = (Time.time - startTime) * (1 / anticipation);
        }
        
        
        //Animate Properties
        mat.SetFloat("_SHowIndicator", showIndicatorCurve.Evaluate(lifetime));
        mat.SetFloat("_Dissolve", dissolveCurve.Evaluate(lifetime));


        // Register PlayStart
        if (effect.aliveParticleCount > 0 && !effectPlayed)
        {
            effectPlayed = true;
        }
        //Delete Game Object after Playing
        if (effect.aliveParticleCount == 0 && effectPlayed && lifetime > anticipation + dissapation)
        {
            Destroy(gameObject);
        }
        
        
    }
}
}

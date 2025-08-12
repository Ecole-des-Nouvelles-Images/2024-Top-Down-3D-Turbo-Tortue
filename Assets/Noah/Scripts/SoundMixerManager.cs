using UnityEngine;
using UnityEngine.Audio;

namespace Noah.Scripts
{
    public class SoundMixerManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;

        public void SetMasterVolume(float level)
        {
           // audioMixer.SetFloat("masterVolume", level);
           audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
        }
        
        public void SetSoundFXVolume(float level)
        {
           // audioMixer.SetFloat("soundFX", level);
           audioMixer.SetFloat("soundFX", Mathf.Log10(level) * 20f);
        }
        
        public void SetMusicVolume(float level)
        {
           //audioMixer.SetFloat("musicVolume", level);
           audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
           
        }
    }
}

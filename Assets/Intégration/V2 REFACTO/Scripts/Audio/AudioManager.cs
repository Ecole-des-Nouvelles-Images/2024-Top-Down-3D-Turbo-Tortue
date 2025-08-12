using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviourSingleton<AudioManager>
{
    [Space]
    [SerializeField] private AudioMixer _mixerSystem;
   
    [Space]
    public AudioClipsIndex ClipsIndex;

    [Header("Initial Mix")]
    [SerializeField] [Range(0, 1)] private float _initialMasterVolume = 0.5f;
    [SerializeField] [Range(0, 1)] private float _initialAmbientVolume = 1f;
    [SerializeField] [Range(0, 1)] private float _initialSFXVolume = 1f;
   

    [Header("Mixer Groups")]
    [field:SerializeField] public AudioMixerGroup MasterMixerModule;
    [field:SerializeField] public AudioMixerGroup MusicMixerModule;
    [field:SerializeField] public AudioMixerGroup SFXMixerModule;

    [Header("Ambient Audio Sources")]
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioSource _sfxAudioSource;
    [SerializeField] private AudioSource _loopSfxAudioSource;

    private static readonly string MasterVolumeParameter = "VolumeMaster";
    private static readonly string AmbientVolumeParameter = "VolumeMusic";
    private static readonly string SFXVolumeParameter = "VolumeSFX";
    private static readonly string LowpassParameter = "LowPassFreqMusic";

    private float _masterVolume;
    public float MasterVolume {
        get => _masterVolume;

        set {
            _masterVolume = value;
            UpdateVolume(MasterVolumeParameter, value);
        }
    }
    

    private float _ambientVolume;
    public float AmbientVolume {
        get => _ambientVolume;

        set {
            _ambientVolume = value;
            UpdateVolume(AmbientVolumeParameter, value);
        }
    }

    private float _sfxVolume;
    public float SFXVolume {
        get => _sfxVolume;

        set {
            _sfxVolume = value;
            UpdateVolume(SFXVolumeParameter, value);
        }
    }

    private void Awake()
    {
        MasterVolume = _initialMasterVolume;
        AmbientVolume = _initialAmbientVolume;
        SFXVolume = _initialSFXVolume;
    }

  
    private void UpdateVolume(string mixerParameter, float value)
    {
        _mixerSystem.SetFloat(mixerParameter, Mathf.Log10(value) * 20);
    }
    
    
    public void SetLowpassFrequency(float frequency)
    {
        _mixerSystem.SetFloat(LowpassParameter,frequency);
    }
    
    
    public void ChangeMusic(AudioClip musicClip)
    {
        _musicAudioSource.clip = musicClip;
        _musicAudioSource.Play();
    }
    
    public void ReplayMusic()
    {
        _musicAudioSource.Play();
    }

    public void PlaySound(AudioClip clip,float volume = 1f)
    {
        _sfxAudioSource.PlayOneShot(clip,volume);
    }
    
    public bool IsLoopingSfxPlaying()
    {
        return _loopSfxAudioSource.isPlaying;
    }
    
    public void PlayRandomSound(List<AudioClip> clips, float volume = 1f)
    {
        _sfxAudioSource.PlayOneShot(clips[Random.Range(0,clips.Count)],volume);
    }
    
    public void PlayLoopSfx(AudioClip clip, float volume = 1f)
    {
        if (_loopSfxAudioSource.isPlaying && _loopSfxAudioSource.clip == clip) return;

        _loopSfxAudioSource.clip = clip;
        _loopSfxAudioSource.loop = true;
        _loopSfxAudioSource.volume = volume;
        _loopSfxAudioSource.Play();
    }
    
    public void StopLoopingSfx()
    {
       _loopSfxAudioSource.Stop();
        _loopSfxAudioSource.clip = null;
    }
    
}


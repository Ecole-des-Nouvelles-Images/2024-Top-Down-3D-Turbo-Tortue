using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    Pressed,
   Canceled,
   Selected,
   Navigated,
   FlowerVoices,
   TurtleMoveStart,
   
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] _soundList;
    private static SoundManager _instance;
    private AudioSource _audioSource;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = _instance._soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        _instance._audioSource.PlayOneShot(randomClip, volume);
    }

#if UNITY_EDITOR

    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref _soundList, names.Length);
        for (int i = 0; i < _soundList.Length; i++)
        {
            _soundList[i].Name = names[i];
        }
    }
#endif

    [Serializable]
    public struct SoundList
    {
        public AudioClip[] Sounds
        {
            get => _sounds;
        }

        [HideInInspector] public string Name;
        [SerializeField] private AudioClip[] _sounds;
    }
}
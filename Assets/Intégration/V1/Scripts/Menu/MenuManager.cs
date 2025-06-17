using System;
using DG.Tweening;
using Intégration.V1.Scripts.SharedScene;
using Michael.Scripts.Manager;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.Menu
{
    public class MenuManager : MonoBehaviourSingleton<MenuManager>
    {
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        
        private void Start()
        {
            _masterSlider.value = AudioManager.Instance.MasterVolume;
            _musicSlider.value = AudioManager.Instance.AmbientVolume;
            _sfxSlider.value = AudioManager.Instance.SFXVolume;
        }

        private void OnEnable()
        {
            ManageEventCallbacks(true);
        }

        private void OnDisable()
        {
            ManageEventCallbacks(false);
        }

        private void ManageEventCallbacks(bool subscribe)
        {
            if (subscribe)
            {
                _masterSlider.onValueChanged.AddListener(delegate { AudioManager.Instance.MasterVolume = _masterSlider.value; });
                _musicSlider.onValueChanged.AddListener(delegate { AudioManager.Instance.AmbientVolume = _musicSlider.value; });
                _sfxSlider.onValueChanged.AddListener(delegate { AudioManager.Instance.SFXVolume = _sfxSlider.value; });
            }
            else
            {
                _masterSlider.onValueChanged.RemoveAllListeners();
                _musicSlider.onValueChanged.RemoveAllListeners();
                _sfxSlider.onValueChanged.RemoveAllListeners();
            }
        }
        

        public void StartGame(string sceneTitle)
        {
            CustomSceneManager.Instance.LoadScene(sceneTitle);
        }
        

        
        private void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }

        
        public void FadeInPanel(GameObject panel)
        {
            panel.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        }

        public void FadeOutPanel(GameObject panel)
        {
            panel.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
        }


        public void ToggleVibration()
        {
            DataManager.CanVibrate = !DataManager.CanVibrate;
        }

        public void ToggleUIWorldSpace()
        {
            DataManager.UiInWorldSpace = !DataManager.UiInWorldSpace;
        }
        
        public void PlayPressedSound()
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIButtonPressed);
        }
        
        
    }
}
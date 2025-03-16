using System;
using DG.Tweening;
using Intégration.V1.Scripts.SharedScene;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.Menu
{
    public class MenuManager : MonoBehaviourSingleton<MenuManager>
    {
        public bool _gameStarted;
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject CharacterSelectionPanel;
        [SerializeField] private GameObject eventSystem;


        private void Start()
        {
            _musicSlider.value = DataManager.MusicVolume;
            _sfxSlider.value = DataManager.SfxVolume;
            SetMusicVolume();
            SetSfxVolume();

           /* if (CharacterSelectionPanel)
            {
                if (DataManager.CharacterSelectionScene)
                {
                    eventSystem.SetActive(false);
                    CharacterSelectionPanel.SetActive(true);
                    DataManager.CharacterSelectionScene = false;
                }
                else
                {
                    mainMenuPanel.SetActive(true);
                }
            }*/
        }

        public void GameStartAction()
        {
            if ( !_gameStarted) {
                
              Invoke("StartGame",1.1f);
                CharacterSelection.CanStart = false;
                _gameStarted = true;
            }
        }

        public void StartGame(string sceneTitle)
        {
            CustomSceneManager.Instance.LoadScene(sceneTitle);
        }

        private void Update()
        {
          /*  if (CharacterSelection.CanStart && !_gameStarted)
            {
                StartGame();
                CharacterSelection.CanStart = false;
                _gameStarted = true;
            }*/
        }

        public void QuitApplication()
        {
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


        public void SetMusicVolume()
        {
            DataManager.MusicVolume = _musicSlider.value;
            _mixer.SetFloat("musicVolume", Mathf.Log10(DataManager.MusicVolume) * 20);
        }

        public void SetSfxVolume()
        {
            DataManager.SfxVolume = _sfxSlider.value;
            _mixer.SetFloat("soundFXVolume", Mathf.Log10(DataManager.SfxVolume) * 20);
        }
    }
}
using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.SharedScene;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Manager;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.Menu
{
    public class MenuManager : MonoBehaviourSingleton<MenuManager>
    {
        [Header("Options Ui")]
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Toggle _vibrationTogle;
        
        [Header("Transition references")]
        [SerializeField] private GameObject circularTransition;
        
        
        private void Start()
        {
            _masterSlider.value = AudioManager.Instance.MasterVolume;
            _musicSlider.value = AudioManager.Instance.AmbientVolume;
            _sfxSlider.value = AudioManager.Instance.SFXVolume;
            _vibrationTogle.isOn = DataManager.Instance.CanVibrate;
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
                _vibrationTogle.onValueChanged.AddListener(ToggleVibration);
            }
            else
            {
                _masterSlider.onValueChanged.RemoveAllListeners();
                _musicSlider.onValueChanged.RemoveAllListeners();
                _sfxSlider.onValueChanged.RemoveAllListeners();
                _vibrationTogle.onValueChanged.RemoveListener(ToggleVibration);
            }
        }
        
        public void CircleTransition(GameObject transitionImage,float endScale, float duration)
        {
            transitionImage.transform.DOScale(endScale, duration).SetUpdate(true);
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


        private void ToggleVibration(bool isON)
        {
            DataManager.Instance.CanVibrate = isON;
            //RumbleManager.Instance.RumbleAllGamepad();
        }
        
        
        public void PlayPressedSound()
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIButtonPressed);
        }
        
        #region EndGamePanel Methods
        
        public void ReloadScene()
        {
            //remet le pause a zero
            Time.timeScale = 1;
            PauseControlller.IsPaused = false;
            AudioManager.Instance.SetLowpassFrequency(4000f);
            
            CircleTransition(circularTransition,1,1.2f);
            RemoveAllInputUsers();
            CustomSceneManager.Instance.Invoke(nameof(CustomSceneManager.ReloadActiveScene),1f);
            AudioManager.Instance.ReplayMusic();
        }

        public void LoadMenuScene(string sceneName)
        {
            //remet le pause a zero
            Time.timeScale = 1;
            PauseControlller.IsPaused = false;
            AudioManager.Instance.SetLowpassFrequency(4000f);
            
            
           CircleTransition(circularTransition,1,1.2f);
           RemoveAllInputUsers();
           DataManager.Instance.PlayerChoice.Clear();
           CustomSceneManager.Instance.LoadScene(sceneName);
           AudioManager.Instance.ChangeMusic(AudioManager.Instance.ClipsIndex.MenuMusic);
        }
        
        
        private void RemoveAllInputUsers()
        {
            // Copie de la liste des utilisateurs actifs pour éviter de modifier la collection en itérant
            List<InputUser> users = new List<InputUser>(InputUser.all);

            foreach (var user in users)
            {
                user.UnpairDevicesAndRemoveUser();
            }
            Debug.Log("Tous les InputUser ont été supprimés.");
        }


       /* private void OnDestroy()
        {
            RemoveAllInputUsers();
        }*/
        
        #endregion
    }
    
}
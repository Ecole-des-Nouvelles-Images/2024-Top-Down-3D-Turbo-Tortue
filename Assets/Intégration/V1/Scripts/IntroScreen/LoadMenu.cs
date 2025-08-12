using System;
using DG.Tweening;
using Intégration.V1.Scripts.SharedScene;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.Video;

namespace Intégration.V1.Scripts.IntroScreen
{
    public class LoadMenu : MonoBehaviour
    {
        [Header("Videos")]
        [SerializeField] private VideoPlayer _splashScreenVideo;
        [SerializeField] private VideoPlayer _introVideo;
        
        [Header("UI")]
        [SerializeField] private MultiplayerEventSystem _multiplayerEventSystem;
        [SerializeField] private CanvasGroup _skipBarCanvasGroup;
        [SerializeField] private GameObject _skipButton;
        
        [SerializeField] private string _menuSceneTitle;
       
     

        private void OnEnable()
        {
            _introVideo.loopPointReached += LoadMenuScene;
            _splashScreenVideo.loopPointReached += PlayIntro;
        }

        private void OnDisable()
        {
            _introVideo.loopPointReached -= LoadMenuScene;
            _splashScreenVideo.loopPointReached -= PlayIntro;
        }
        
        
        public void LoadMenuScene(VideoPlayer videoPlayer)
        {
            CustomSceneManager.Instance.LoadScene(_menuSceneTitle);
            AudioManager.Instance.ChangeMusic(AudioManager.Instance.ClipsIndex.MenuMusic);
        }

        private void PlayIntro(VideoPlayer videoPlayer)
        {
            _introVideo.Play();
            _skipBarCanvasGroup.DOFade(1f,1f).SetDelay(3f).OnComplete(() => 
            {
                _multiplayerEventSystem.SetSelectedGameObject(_skipButton);
            }); 
           
        }
        
        
        
  
    }
}
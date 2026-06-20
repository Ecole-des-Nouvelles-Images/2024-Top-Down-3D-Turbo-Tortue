using System;
using DG.Tweening;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;

namespace Intégration.V1.Scripts.UI
{
    public class PauseControlller : MonoBehaviour
    {
        public static Action OnGamePaused;
        public static bool IsPaused;
        
        [Header("Animation References")]
        [SerializeField] private CanvasGroup _pausePanelCG;
        [SerializeField] private RectTransform _gameTitle;
        [SerializeField] private RectTransform _buttonsBackground;
        [SerializeField] private float _pauseIntroDuration = 0.5f;

        [Header("Pause logic")]
        [SerializeField] private MultiplayerEventSystem _eventSystem;
        [SerializeField] private GameObject _playButton;
        private Sequence _openPauseSeq;
        private Sequence _closePauseSeq;

        private void Start()
        {
            IsPaused = false;
            Time.timeScale = 1;
        }

        private void OnEnable()
        {
            OnGamePaused += OpenPausePanel;
        }
        private void OnDisable()
        {
            OnGamePaused -= OpenPausePanel;
        }

        private void OpenPausePanel()
        {
            if (!(Time.timeScale > 0) || !GameManager.Instance.GameisStarted) return;
            
            Debug.Log("GAME PAUSEDDDD ?");
            _openPauseSeq = DOTween.Sequence();
            _openPauseSeq.SetUpdate(true);
            //RumbleManager.Instance.StopAllVibrations();

            _openPauseSeq.Append(_pausePanelCG.DOFade(1, _pauseIntroDuration));
           _openPauseSeq.Join(_buttonsBackground.DOAnchorPosX(0,_pauseIntroDuration)).SetEase(Ease.OutBack);
           _openPauseSeq.Join(_gameTitle.DOAnchorPosX(-350,_pauseIntroDuration)).SetEase(Ease.OutBack);
           _openPauseSeq.OnComplete(() =>
           {
               Time.timeScale = 0;
               IsPaused = true;
               _eventSystem.SetSelectedGameObject(_playButton);
               AudioManager.Instance.SetLowpassFrequency(500f);
           });
            
            if (AudioManager.Instance.IsLoopingSfxPlaying())
            {
                AudioManager.Instance.StopLoopingSfx();
            }
        }


        public void ClosePausePanel()
        {
            if (!(Time.timeScale <= 0) || !GameManager.Instance.GameisStarted) return;
            
            _eventSystem.SetSelectedGameObject(null);
            _closePauseSeq = DOTween.Sequence();
            _closePauseSeq.SetUpdate(true);

            _closePauseSeq.Append(_pausePanelCG.DOFade(0, _pauseIntroDuration));
            _closePauseSeq.Join(_buttonsBackground.DOAnchorPosX(-350,_pauseIntroDuration)).SetEase(Ease.OutBack);
            _closePauseSeq.Join(_gameTitle.DOAnchorPosX(0,_pauseIntroDuration)).SetEase(Ease.OutBack);
            _closePauseSeq.OnComplete(() =>
            {
                Time.timeScale = 1;
                IsPaused = false;
                AudioManager.Instance.SetLowpassFrequency(4000f);
            });
            
            
            
        }
    }
}
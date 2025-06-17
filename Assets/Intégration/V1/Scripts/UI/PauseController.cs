using DG.Tweening;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

namespace Intégration.V1.Scripts.UI
{
    public class PauseControlller : MonoBehaviourSingleton<PauseControlller>
    {
        public static bool IsPaused;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _eventSystem;
        [SerializeField] private GameObject _playButton;
        [SerializeField] private AudioMixer _musicMixer;
        private void Start()
        {
            IsPaused = false;
            Time.timeScale = 1;
        }


        public void OpenPausePanel()
        {
            if (!(Time.timeScale > 0) || GameManager.Instance.GameisStarted) return;
            
            Time.timeScale = 0;
            IsPaused = true;
            _pausePanel.SetActive(true);
            _eventSystem.SetActive(true);
            _eventSystem.GetComponent<EventSystem>().SetSelectedGameObject(_playButton);
            AudioManager.Instance.SetLowpassFrequency(500f);
        }


        public void ClosePausePanel()
        {
            if (!(Time.timeScale <= 0) || GameManager.Instance.GameisStarted) return;
            
            Time.timeScale = 1;
            IsPaused = false;
            _pausePanel.SetActive(false);
            AudioManager.Instance.SetLowpassFrequency(5000f);
        }
    }
}
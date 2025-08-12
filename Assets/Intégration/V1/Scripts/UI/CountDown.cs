using DG.Tweening;
using Michael.Scripts.Manager;
using TMPro;
using UnityEngine;

namespace Intégration.V1.Scripts.UI
{
    public class CountDown : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);
        [SerializeField] private float bounceDuration = 0.5f;
        [SerializeField] private float delayBetweenNumbers = 1f;
        [SerializeField] private float delayBeforeCountdown = 2f;
        [SerializeField] private int number = 4;
        [SerializeField] private string StartMessage;

        private void Start()
        {
            Invoke("Countdown",1.6f);
        }

        private void Update()
        {
           /* if (GameManager.Instance.GameisStarted)
            {
                Countdown();
                GameManager.Instance.GameisStarted = false;
            }*/
        }

        private void Countdown()
        {
            if (number > 0)
            {
                if (number == 4)
                {
                    countdownText.text = StartMessage;
                }
                else
                {
                    countdownText.text = number.ToString();
                    countdownText.transform.DOScale(targetScale, bounceDuration)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(() => countdownText.transform.DOScale(Vector3.one, bounceDuration)
                            .SetEase(Ease.InOutSine));
                }

                if (number == 4)
                {
                    Invoke("StartCountDown", delayBeforeCountdown);
                }
                else
                {
                    Invoke("Countdown", delayBetweenNumbers);
                    number--;
                }
                
                if (number == 1)
                {
                    AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleExplosion);
                }
            }
            else
            {
                countdownText.gameObject.SetActive(false);
                //countdownText.text = "GO!";
                GameManager.Instance.StartGame();
               
            }
        }


        private void StartCountDown()
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UICountDown);
            number--;
            Countdown();
        }
    }
}
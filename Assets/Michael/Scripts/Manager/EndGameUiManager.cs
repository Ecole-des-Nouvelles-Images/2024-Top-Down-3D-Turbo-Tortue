using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.SharedScene;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace Michael.Scripts.Manager
{
    public class EndGameUiManager : MonoBehaviour
    {
        [Header("Players stats Elements ")] [SerializeField]
        private RectTransform _playerStatsLayout;

        [SerializeField] private GameObject _playerStatsPrefabs;

        [Header("Buttons")] [SerializeField] private Button _confirmButon;
        [SerializeField] private Button _restartButton;
        [SerializeField] private RectTransform _buttonsTransform;


        [SerializeField] private MultiplayerEventSystem eventSystem;
        [SerializeField] private TextMeshProUGUI _winnerTitle;
        [SerializeField] private Image _happyScientistImage;
        [SerializeField] private Image _angryScientistImage;
        [SerializeField] private RectTransform _scientistTransform;

        private CanvasGroup _canvasGroup;
        private Sequence _winnerTitleSeq;

        private void OnEnable()
        {
            _confirmButon.onClick.AddListener(EnableRestartButtons);
        }

        private void OnDisable()
        {
            _confirmButon.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ShowWinnerPanel()
        {
            _winnerTitle.text = GameManager.Instance.FlowersIsDead ? "Victoire de\nTurbo Tortue" : "Victoire\ndes fleurs";
            
            if (GameManager.Instance.FlowersIsDead)
            {
                _happyScientistImage.gameObject.SetActive(true);
            }
            else
            {
                _angryScientistImage.gameObject.SetActive(true);
            }


            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIWinner);
            _winnerTitleSeq = DOTween.Sequence();
            _winnerTitleSeq.AppendInterval(0.5f);
            _winnerTitleSeq.Append(_canvasGroup.DOFade(1, 0.5f));
            _winnerTitleSeq.Join(_winnerTitle.rectTransform.DOScale(1.3f, 0.8f).SetEase(Ease.InOutSine));
            _winnerTitleSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIWinner));
            _winnerTitleSeq.Join(_winnerTitle.rectTransform.DOPunchRotation(new Vector3(10, 10, 10), 0.5f, 10))
                .OnComplete(() =>
                {
                    eventSystem.SetSelectedGameObject(_confirmButon.gameObject);
                });


        }

        private void EnableRestartButtons()
        {
            GameManager.Instance.firstCamera.SetActive(true);
            _scientistTransform.gameObject.SetActive(false);
            _confirmButon.gameObject.SetActive(false);
            _winnerTitle.rectTransform.DOScale(1f, 0.6f).SetEase(Ease.OutBack);
            _winnerTitle.rectTransform.DOAnchorPosY(400f, 0.5f).SetEase(Ease.OutCubic);
            _buttonsTransform.DOAnchorPosY(200f, 0.7f).SetEase(Ease.Linear).OnComplete(ShowPlayersStats);
        }

        private void ShowPlayersStats()
        {
            float delay = 0;

            foreach (var player in GameManager.Instance.Players)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                GameObject Playerstats = Instantiate(_playerStatsPrefabs, _playerStatsLayout);
                PlayerStatsUI statsUI = Playerstats.GetComponent<PlayerStatsUI>();

                statsUI.Setup(stats);

                Sequence seq = DOTween.Sequence();

                seq.AppendInterval(delay);
                seq.Append(Playerstats.transform.DOScale(1f, 0.6f).SetEase(Ease.OutBounce));
                seq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIPopPanel));
                seq.Join(Playerstats.transform.DOPunchRotation(new Vector3(10, 10, 10), 0.5f, 10)).OnComplete(() =>
                    {
                        eventSystem.SetSelectedGameObject(_restartButton.gameObject);
                    });

                delay += 1f;
            }
        }
        
    }
}  
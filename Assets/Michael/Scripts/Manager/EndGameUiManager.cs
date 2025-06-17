using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.SharedScene;
using Michael.Scripts.Ui;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Michael.Scripts.Manager
{
    public class EndGameUiManager : MonoBehaviour
    {
        [Header("Players stats Elements ")]
        [SerializeField] private RectTransform _playerStatsLayout;
        [SerializeField] private GameObject _playerStatsPrefabs;
        
        [Header("Buttons")]
        [SerializeField] private GameObject _confirmButon ;
        [SerializeField] private GameObject _restartButton;
        [SerializeField] private RectTransform _buttonsTransform;
        
        
        [SerializeField] private MultiplayerEventSystem eventSystem;
        [SerializeField] private TextMeshProUGUI _winnerTitle;
        private CanvasGroup _canvasGroup;
        
        
        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ShowWinnerPanel()
        {
            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(0.5f);
            seq.Append(_canvasGroup.DOFade(1, 0.3f))
                .Append(_winnerTitle.rectTransform.DOScale(2f, 0.6f).SetEase(Ease.OutBack))
                .Append(_winnerTitle.rectTransform.DOPunchRotation(new Vector3(0, 0, 10), 0.4f, 6))
                .OnComplete(() => eventSystem.SetSelectedGameObject(_confirmButon));
        }

        public void EnableButtons()
        {
            //Sequence seq = DOTween.Sequence();
            
            
            _confirmButon.SetActive(false);
            _winnerTitle.rectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
            _winnerTitle.rectTransform.DOAnchorPosY(400f, 0.5f).SetEase(Ease.OutCubic);
            _buttonsTransform.DOAnchorPosY(200f, 0.7f).SetEase(Ease.Linear).OnComplete(() =>
            {
               //instancier les player stats
                eventSystem.SetSelectedGameObject(_restartButton);
            });
        }

        public void ShowPlayersStats()
        {
            foreach (var player in GameManager.Instance.Players)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                GameObject Playerstats = Instantiate(_playerStatsPrefabs, _playerStatsLayout);
                PlayerStatsUI statsUI = Playerstats.GetComponent<PlayerStatsUI>();
                
                statsUI.Setup(stats);
            }
        }
        
        
        
        #region LoadScene Methods
        
        public void ReloadScene()
        {
           GameManager.Instance.CircleTransition(1,1.2f);
            RemoveAllInputUsers();
            CustomSceneManager.Instance.Invoke(nameof(CustomSceneManager.ReloadActiveScene),1f);
        }

        public void LoadScene(string sceneName)
        {
            GameManager.Instance.CircleTransition(1,1.2f);
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


        private void OnDestroy()
        {
            RemoveAllInputUsers();
        }
        
        #endregion
    }
}  
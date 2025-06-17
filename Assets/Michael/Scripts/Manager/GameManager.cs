using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Intégration.V1.Scripts.Game;
using Intégration.V1.Scripts.SharedScene;
using Michael.Scripts.Controller;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Michael.Scripts.Manager
{
    [System.Serializable]

    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public Action OnFlowersWin;
        [Header("Booleen")]
        public bool TurtleIsDead = false;
        public bool GameFinished = false;
        public bool GameisStarted = false;

        [Header("Players References")] 
        public GameObject Turtle;
        public List<GameObject> FlowersAlive;
        public List<GameObject> Players;
        public List<GameObject> TurtleTrap;
        
        [Header("Sun System references")] 
        public Dictionary<GameObject, Transform> _sunOccupiedSpawns = new Dictionary<GameObject, Transform>();
        [SerializeField] private Transform[] _sunSpawnPoints;
        [SerializeField] private GameObject _sunPrefabs;
        [SerializeField] private GameObject SunSpawnsParent;
        
        [Header("Ui References")]
        [SerializeField] private GameObject circularTransition;
        [SerializeField] private GameObject PlayersUi;
        [SerializeField] private EndGameUiManager endGameUiManager;
        [Header("others")]
        [SerializeField] private GameObject CrashVfx;
        [SerializeField] private GameObject firstCamera;
        [SerializeField] private CinemachineTargetGroup _targetGroup;
        
       
       
        
        void Start()
        {
            GameisStarted = true;
            //GameisStarted = false;
            // Invoke(nameof(ShowRulesPanels), 3f);
          
            CircleTransition(15,1f);
        }

        public void CircleTransition(float endScale, float duration)
        {
            circularTransition.transform.DOScale(endScale, duration);
        }
        
        public void StartGame()
        {
            CrashVfx.SetActive(true);
            InvokeRepeating(nameof(SpawnSun), 2, 8);
            Invoke("TurtleEntrance", 1.45f);
            
        }
        
        private void FinishGame()
        {
            foreach (GameObject player in Players)
            {
                player.GetComponent<PlayerInput>().enabled = false;
            }
            
            PlayersUi.SetActive(false);
            GameFinished = true;
            endGameUiManager.ShowWinnerPanel();
            ChangeCameraWeight(Turtle,10);// zoomer sur la tortu

        }

        

        public void WinVerification()
        {
            if (FlowersAlive.Count <= 0 && !GameFinished) {
               
                
               FinishGame();
            }
            else if (TurtleIsDead && !GameFinished) {
               
                OnFlowersWin.Invoke();
                FinishGame();
            }
        }
      

        /*public void ShowRulesPanels()
        {
            Time.timeScale = 0;
            eventSystem.GetComponent<EventSystem>().SetSelectedGameObject(okButton);

        }
        
        public void closeRulesPanels()
        {
            Time.timeScale = 1;
            GameisStarted = true; 
        }*/
        
        
        private void SpawnSun()
        {
            if (_sunSpawnPoints.Length <= 0) return;
            
            int sunTospawn = Random.Range(3, 5);

            for (int i = 0; i < sunTospawn; i++)
            {
                Transform randomSpawnPoint = _sunSpawnPoints[Random.Range(0, _sunSpawnPoints.Length)];
                if (!_sunOccupiedSpawns.ContainsValue(randomSpawnPoint))
                {

                    GameObject Sun = Instantiate(_sunPrefabs, randomSpawnPoint.position, randomSpawnPoint.rotation,
                        SunSpawnsParent.transform);
                    _sunOccupiedSpawns.Add(Sun, randomSpawnPoint);
                }
            }
        }

        private void TurtleEntrance()
        {
            Turtle.GetComponent<TurtleController>().EnableTurtle();
            CameraShake(1, 0.5f, 10);
            firstCamera.SetActive(false);
            PlayersUi.SetActive(true);
            PlayersUi.transform.DOShakePosition(0.5f, 0.1f, 10);
        }


        public void CameraShake(float duration, float strength, int vibrato)
        {
            firstCamera.transform.DOShakePosition(duration, strength, vibrato);
        }
        
        private void ChangeCameraWeight(GameObject target,float weight)
        {
            for (int i = 0; i < _targetGroup.m_Targets.Length; i++)
            {
                if (_targetGroup.m_Targets[i].target.gameObject == target)
                {
                    _targetGroup.m_Targets[i].weight = weight;
                    break;
                }
            }
        }
        
    }
}

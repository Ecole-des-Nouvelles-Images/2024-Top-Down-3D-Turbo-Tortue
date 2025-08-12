using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Intégration.V1.Scripts.Menu;
using Michael.Scripts.Controller;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Michael.Scripts.Manager
{
    [System.Serializable]

    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public Action OnIntroStarted;
        public Action OnFlowersWin;
        public Action OnTurtleDead;
        [Header("Booleen")]
        public bool TurtleIsDead = false;
        public bool FlowersIsDead = false;
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
        [SerializeField] private GameObject PlayersUi;
        [SerializeField] private EndGameUiManager endGameUiManager;
        
        [Header("Cameras")]
        public GameObject firstCamera;
        [SerializeField] private CinemachineTargetGroup _targetGroup;
        
        [Header("Effects")]
        [SerializeField] private GameObject MeteorVfx;
        [SerializeField] private GameObject CrashVfx;
       // [SerializeField] private ParticleSystem _dandelionDeathVfx;
        
        [Header("Transition references")]
        [SerializeField] private GameObject circularTransition;

        private QteManager _qteManager;

        void Start()
        {
            MenuManager.Instance.CircleTransition(circularTransition,15,1.5f);
        }

       
        
        public void StartGame()
        {
            MeteorVfx.SetActive(true);
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

        /*public void DandelionDeath()
        {
            _dandelionDeathVfx.Play();
        }*/


        public void WinVerification()
        {
            if (FlowersAlive.Count <= 0 && !GameFinished)
            {
                FlowersIsDead = true;
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
            RumbleManager.Instance.RumbleAllGamepad();
            Turtle.transform.position = MeteorVfx.transform.position;
            _qteManager = Turtle.GetComponent<QteManager>();
            _qteManager.StartQTE();
            CrashVfx.SetActive(true);
            CameraShake(1, 0.5f, 10);
            
            firstCamera.SetActive(false);
            PlayersUi.SetActive(true);
            PlayersUi.transform.DOShakePosition(0.5f, 0.2f, 10);
            GameisStarted = true;
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

using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Michael.Scripts.Manager
{
    public class QteManager : MonoBehaviourSingleton<QteManager>
    {
        public Action OnQteFinished;
        
        [SerializeField] private float qteTimePerButton = 1f;
        [SerializeField] private PlayerInput _turtlePlayerInput;
        [SerializeField] private InputAction[] _qteActions;
        [SerializeField] private GameObject _currentQTeImage;
        [SerializeField] private Image _FailQTeImage;
        [SerializeField] private List<InputAction> qteSequence;
        [SerializeField] private List<Sprite> qteImages;
        [SerializeField] private int TouchQteCount;
        [SerializeField] private ParticleSystem _electricParticleSystem;
        
        private bool qteActive;
        private float qteTimer;
        private int currentButtonIndex =0;
        private CanvasGroup failImageCG;
        private Sequence _failQteSequence;
        
        private void Start()
        {
            failImageCG = _FailQTeImage.GetComponent<CanvasGroup>();
            OnQteFinished += QTESuccess;

            _qteActions[0] = _turtlePlayerInput.actions["UpArrow"];
            _qteActions[1] = _turtlePlayerInput.actions["DownArrow"];
            _qteActions[2] = _turtlePlayerInput.actions["RightArrow"];
            _qteActions[3] = _turtlePlayerInput.actions["LeftArrow"];
            _qteActions[4] = _turtlePlayerInput.actions["LeftShoulder"];
            _qteActions[5] = _turtlePlayerInput.actions["RightShoulder"];
            
            //StartQTE();
        }

        void UpdteQTEUi()
        {
           
            if (currentButtonIndex < TouchQteCount)
            {
                InputAction currentAction = qteSequence[currentButtonIndex];
                int index = System.Array.IndexOf(_qteActions, currentAction);
                _currentQTeImage.GetComponent<Image>().sprite = qteImages[index];

                // Réinitialiser le temps restant pour le bouton actuel
                qteTimer = qteTimePerButton;
                
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.QTEKey);
            }
        }

        void GenerateQTESequence()
        {
            qteSequence.Clear();

            for (int i = 0; i < TouchQteCount; i++)
            {
                InputAction randomAction = _qteActions[Random.Range(0, _qteActions.Length)];
                while (qteSequence.Contains(randomAction))
                {
                    randomAction = _qteActions[Random.Range(0, _qteActions.Length)];
                }

                qteSequence.Add(randomAction);
            }
        }

        [ContextMenu("StartQTE")]
        public void StartQTE()
        {
            if (!qteActive)
            {
                failImageCG.alpha = 0;
                _FailQTeImage.rectTransform.DOAnchorPosY(0, 0f);
                
                qteActive = true;
                GenerateQTESequence();
                currentButtonIndex = 0;
                _currentQTeImage.SetActive(true);
                UpdteQTEUi();
            }
        }

        private void Update()
        {
            if (qteActive)
            {
                // Mettre à jour le temps restant pour le bouton actuel
                qteTimer -= Time.deltaTime;

                if (qteTimer <= 0f)
                {
                    // Le temps est écoulé, échec de la séquence QTE
                    QTEFailure();
                }
                else
                {
                    // Vérifier l'entrée du joueur pour le bouton actuel
                    CheckQTEInput();
                }
            }
        }

        void CheckQTEInput()
        {
            if (PauseControlller.IsPaused) return;
            
            foreach (var action in _qteActions)
            {
                if (action.WasPressedThisFrame() && action == qteSequence[currentButtonIndex])
                {
                    // Le joueur a appuyé sur le bon bouton, passer au bouton suivant
                    currentButtonIndex++;
                    UpdteQTEUi();

                    if (currentButtonIndex >= TouchQteCount)
                    {
                        // Tous les boutons ont été pressés avec succès, fin de la séquence QTE
                        OnQteFinished.Invoke();
                        
                    }

                    break;
                }
                else if (action.WasPressedThisFrame())
                {
                    // Le joueur a appuyé sur le mauvais bouton, échec de la séquence QTE
                    QTEFailure();
                    

                    break;
                }
            }
        }

        void QTESuccess()
        {
            qteActive = false;
            qteSequence.Clear();
            _currentQTeImage.SetActive(false);
            _turtlePlayerInput.currentActionMap = _turtlePlayerInput.actions.FindActionMap("Character");
            GetComponent<Rigidbody>().isKinematic = false;
            
            _electricParticleSystem.Play();
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.QTESuccess);
        }

        void QTEFailure()
        {
            qteActive = false;
            qteSequence.Clear();
            _currentQTeImage.SetActive(false);
         
           
           _failQteSequence =  DOTween.Sequence();
           
           _failQteSequence.Append(failImageCG.DOFade(1,0f));
           _failQteSequence.Join(_FailQTeImage.rectTransform.DOAnchorPosY(1, 0.5f));
           _failQteSequence.Append(failImageCG.DOFade(0,0.5f));
            
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.QTEFailed);
            Invoke("StartQTE", 1f);
        }
    }
}
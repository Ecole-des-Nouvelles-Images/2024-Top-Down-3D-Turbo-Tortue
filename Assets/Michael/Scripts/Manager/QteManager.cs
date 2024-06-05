﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace Michael.Scripts.Manager
{
    public class QteManager : MonoBehaviourSingleton<QteManager>
    {
        public bool QteSucces = false;
        public float qteTimePerButton = 1f; // Temps maximal pour chaque bouton
        public Action OnQteFinished;
        [SerializeField] private PlayerInput _turtlePlayerInput;
        [SerializeField] private InputAction[] _qteActions;
        [SerializeField] private GameObject _currentQTeImage;
        [SerializeField] private GameObject _FailQTeImage;
        [SerializeField] private List<InputAction> qteSequence;
        [SerializeField] private List<Sprite> qteImages;
        [SerializeField] private int TouchQteCount;
        private bool qteActive;
        private float qteTimer;
        private int currentButtonIndex =0;

        private void Start()
        {
            OnQteFinished += QTESuccess;

            _qteActions[0] = _turtlePlayerInput.actions["UpArrow"];
            _qteActions[1] = _turtlePlayerInput.actions["DownArrow"];
            _qteActions[2] = _turtlePlayerInput.actions["RightArrow"];
            _qteActions[3] = _turtlePlayerInput.actions["LeftArrow"];
            _qteActions[4] = _turtlePlayerInput.actions["LeftShoulder"];
            _qteActions[5] = _turtlePlayerInput.actions["RightShoulder"];
            
            StartQTE();
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
                _FailQTeImage.SetActive(false);
                QteSucces = false;
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
                qteTimer -= TimeManager.Instance.deltaTime;

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
            QteSucces = true;
            qteSequence.Clear();
            _currentQTeImage.SetActive(false);
            _turtlePlayerInput.currentActionMap = _turtlePlayerInput.actions.FindActionMap("Character");
        }

        void QTEFailure()
        {
            qteActive = false;
            QteSucces = false;
            qteSequence.Clear();
            _currentQTeImage.SetActive(false);
            _FailQTeImage.SetActive(true);

            Invoke("StartQTE", 1f);
        }
    }
}
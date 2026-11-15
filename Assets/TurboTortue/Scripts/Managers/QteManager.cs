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

        [Tooltip("Tortue jouée par l'IA : elle fait le QTE elle-même, à un rythme humain.")]
        public bool IsAIControlled;
        [SerializeField] private Vector2 _aiReactionTime = new Vector2(0.35f, 0.75f);
        [Tooltip("Chance qu'elle réagisse trop tard sur une touche (= QTE raté, elle recommence).")]
        [SerializeField, Range(0f, 1f)] private float _aiMistakeChance = 0.05f;
        private float _aiNextPressTime;

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
        }

        void UpdteQTEUi()
        {
            if (currentButtonIndex < TouchQteCount)
            {
                InputAction currentAction = qteSequence[currentButtonIndex];
                int index = System.Array.IndexOf(_qteActions, currentAction);
                _currentQTeImage.GetComponent<Image>().sprite = qteImages[index];

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
                if (IsAIControlled) ScheduleAIPress();
            }
        }

        private void Update()
        {
            if (qteActive)
            {
                qteTimer -= Time.deltaTime;

                if (qteTimer <= 0f)
                {
                    QTEFailure();
                }
                else
                {
                    if (IsAIControlled) UpdateAIQte();
                    else
                    {
                        CheckPauseInput();
                        CheckQTEInput();
                    }
                }
            }
        }

        private void CheckPauseInput()
        {
            GameManager gameManager = GameManager.Instance;
            if (PauseControlller.IsPaused || _turtlePlayerInput == null || gameManager == null
                || !gameManager.GameisStarted || gameManager.GameFinished) return;

            foreach (InputDevice device in _turtlePlayerInput.devices)
            {
                if (device is Gamepad gamepad && gamepad.startButton.wasPressedThisFrame)
                {
                    PauseControlller.OnGamePaused?.Invoke();
                    return;
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
                    currentButtonIndex++;
                    UpdteQTEUi();

                    if (currentButtonIndex >= TouchQteCount)
                    {
                        OnQteFinished.Invoke();
                    }

                    break;
                }
                else if (action.WasPressedThisFrame())
                {
                    QTEFailure();

                    break;
                }
            }
        }

        private void ScheduleAIPress()
        {
            bool tooSlow = Random.value < _aiMistakeChance;
            float delay = tooSlow
                ? qteTimePerButton + 0.2f
                : Mathf.Min(Random.Range(_aiReactionTime.x, _aiReactionTime.y), qteTimePerButton * 0.85f);
            _aiNextPressTime = Time.time + delay;
        }

        private void UpdateAIQte()
        {
            if (PauseControlller.IsPaused || Time.time < _aiNextPressTime) return;

            currentButtonIndex++;
            UpdteQTEUi();

            if (currentButtonIndex >= TouchQteCount)
                OnQteFinished.Invoke();
            else
                ScheduleAIPress();
        }

        void QTESuccess()
        {
            qteActive = false;
            qteSequence.Clear();
            _currentQTeImage.SetActive(false);
            if (_turtlePlayerInput != null && _turtlePlayerInput.enabled)
            {
                _turtlePlayerInput.currentActionMap = _turtlePlayerInput.actions.FindActionMap("Character");
            }
            GetComponent<Rigidbody>().isKinematic = false;
            GameManager.Instance?.firstCamera.SetActive(false);
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

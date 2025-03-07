using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Intégration.V1.Scripts.SharedScene;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.Menu
{
    public class CharacterSelection : MonoBehaviour
    {
        public static Action OnGameStart;
        
        public static bool[] PlayerIsReady;
        public static bool[] PlayerIsJoined;
        public static bool CanHold;
        public static bool CanStart;
        public static bool TurtleIsSelected;
        public static bool CanJoin;
        public bool IsCharging;
        public int PlayerIndex;
        public static int _minPlayers = 2;
        [SerializeField] private List<Button> _characterButtons;
        [SerializeField] private List<Sprite> _characterSprites;
        [SerializeField] private List<Sprite> _characterCapacitiesSprites;
        [SerializeField] private Button _characterSelected;
        [SerializeField] private Button _joinButton;
        [SerializeField] private int _characterIndex; // l'index du personnage selectionné
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private GameObject Selector;
        [SerializeField] private GameObject joinedText;
        [SerializeField] private Image CapacityImage;
        [SerializeField] private GameObject readyText;
        [SerializeField] private GameObject readyCofirmText;
        [SerializeField] private GameObject circleTransition;
        [SerializeField] private GameObject canvas;
        [SerializeField] private Button _backButton;
        [SerializeField] private Image startButton;
        [SerializeField] private GameObject startText;
        [SerializeField] private GameObject textInfo;
        [SerializeField] private GameObject turtleInfo;
        [SerializeField] private float startTimerDuration = 2f;
        [SerializeField] private float startTimer = 0;
        private Vector3 _initialTransform;
        [SerializeField] private InputAction startGameAction;
        [SerializeField] private AudioSource pressedSound;
        [SerializeField] private AudioSource cancelSound;

        private void Start()
        {
            startGameAction = GetComponent<PlayerInput>().currentActionMap.FindAction("StartGame");
            startGameAction.started += context => StartHold();
            startGameAction.canceled += context => StartRelease();
            MenuManager.Instance._gameStarted = false;


            PlayerIndex = GetComponent<PlayerInput>().user.index;
            _initialTransform = transform.localScale;
            Selector.SetActive(false);
            CanHold = false;
            CanStart = false;
            TurtleIsSelected = false;
            PlayerIsReady = new bool[5] { false, false, false, false, false };
            PlayerIsJoined = new bool[5] { false, false, false, false, false };

            if (readyText)
            {
                readyText.transform.DOScale(1.1f, 0.5f).SetEase(Ease.OutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        private void OnEnable()
        {
            OnGameStart += MenuManager.Instance.GameStartAction;
        }
        
        private void OnDisable()
        {
            OnGameStart -= MenuManager.Instance.GameStartAction;
        }

        public void CanStartGame()
        {
            CanStart = true;
        }

        private void Update()
        {
            
            PlayerSelector();
            if (CanHold)
            {
                if (IsCharging)
                {
                    startButton.fillAmount = 0;
                    startTimer += Time.deltaTime;
                    startButton.fillAmount = startTimer / startTimerDuration;
                    if (startTimer >= startTimerDuration + 0.1f)
                    {
                        circleTransition.transform.DOScale(15, 1);
                        OnGameStart.Invoke();
                    }
                }
                else
                {
                    DOTween.To(() => startButton.fillAmount, value => startButton.fillAmount = value, 0f, 0.5f);
                }
            }
        }

        /*public void SelectionScreen(bool canJoin)
        {
            CanJoin = canJoin;
        }*/


        public void OnNavigate()
        {
            //Bouger le cursor du player 
            //
            //PlayerSelector();
        }

        void PlayerSelector()
        {
            if (_eventSystem.currentSelectedGameObject && 
                _eventSystem.currentSelectedGameObject.GetComponentInChildren<HorizontalLayoutGroup>())
            {
                transform.SetParent(_eventSystem.currentSelectedGameObject
                    .GetComponentInChildren<HorizontalLayoutGroup>().transform);
                _characterSelected = GetComponentInParent<Button>();
                _joinButton.image.sprite = _characterSprites[_characterButtons.IndexOf(_characterSelected)];

                if (_characterSelected.name == "TurtleButton")
                {
                    CapacityImage.gameObject.SetActive(false);
                    _joinButton.transform.DOScale(1.2f, 0.5f);
                    transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
                }
                else if (_characterSelected.name != "TurtleButton")
                {
                    CapacityImage.gameObject.SetActive(true);
                    CapacityImage.sprite = _characterCapacitiesSprites[_characterButtons.IndexOf(_characterSelected)];
                    _joinButton.transform.DOScale(1f, 0.5f);
                    transform.localScale = _initialTransform;
                }
            }
        }
        
        public void PlayerJoined()
        {
            if (CanJoin)
            {
                PlayerIsJoined[PlayerIndex] = true;
                MooveSelectorPosition();
                Debug.Log("un joueur " + PlayerIndex + " a rejoin");
                Selector.SetActive(true);
                joinedText.SetActive(false);
                readyText.SetActive(true);
                CapacityImage.gameObject.SetActive(true);
            }
        }


        public void OnSubmit()
        {
            //Valider la selection d'un personnage 

            if (CanJoin)
            {
                if (PlayerIsJoined[PlayerIndex] == false)
                {
                    PlayerJoined();
                    pressedSound.Play();
                }
                else if (!PlayerIsReady[PlayerIndex])
                {
                    if (GetComponentInParent<Button>())
                    {
                        //_characterSelected = GetComponentInParent<Button>();
                        _characterIndex = _characterButtons.IndexOf(_characterSelected);
                        _characterSelected.enabled = false;
                        PlayerReady();
                        Debug.Log("l'index du personnage selectionné est " + _characterIndex);
                        MooveOtherSelectorPosition();
                        pressedSound.Play();
                    }
                }
            }
        }

        void MooveSelectorPosition()
        {
            Button buttonWithNoChildren = FindButtonWithNoChildren(_characterButtons);
            _eventSystem.SetSelectedGameObject(buttonWithNoChildren.gameObject);
            PlayerSelector();
        }

        void MooveOtherSelectorPosition()
        {
            var buttonGrid = _characterSelected.GetComponentInChildren<HorizontalLayoutGroup>();
            if (buttonGrid.transform.childCount >= 2)
            {
                Debug.Log("il a plus de 1 joueur sur ce personnage");

                for (int i = 0; i < buttonGrid.transform.childCount; i++)
                {
                    Transform childTransform = buttonGrid.transform.GetChild(i);

                    if (childTransform != this.transform)
                    {
                        Button buttonWithNoChildren = FindButtonWithNoChildren(_characterButtons);
                        childTransform.GetComponentInChildren<EventSystem>()
                            .SetSelectedGameObject(buttonWithNoChildren.gameObject);
                        childTransform.GetComponent<CharacterSelection>().PlayerSelector();
                    }
                }
            }
        }

        Button FindButtonWithNoChildren(List<Button> buttonList)
        {
            foreach (Button button in _characterButtons)
            {
                if (button.GetComponentInChildren<HorizontalLayoutGroup>().transform.childCount == 0)
                {
                    return button;
                }
            }

            return null;
        }

        public void OnDeviceLost()
        {
            OnCancel();
        }
        
        


        public void OnCancel()
        {
            // Annulé la la selection d'un personnage
            if (CanJoin)
            {
                if (PlayerIsReady[PlayerIndex])
                {
                    Debug.Log("retour");
                    PlayerIsReady[PlayerIndex] = false;
                    _characterSelected.enabled = true;
                    Debug.Log("le joueur nest plus pret");
                    RemoveChoice(PlayerIndex);
                    joinedText.SetActive(false);
                    readyText.SetActive(true);
                    readyCofirmText.SetActive(false);
                    startText.SetActive(false);
                    textInfo.SetActive(true);

                    if (_characterSelected.name == "TurtleButton")
                    {
                        TurtleIsSelected = false;
                        Debug.Log("turtle deselectionnée");
                    }

                    cancelSound.Play();
                }
                else if (PlayerIsJoined[PlayerIndex])
                {
                    cancelSound.Play();
                    _joinButton.transform.DOScale(1f, 0.5f);
                    _joinButton.image.sprite = _characterSprites[7];
                    CapacityImage.gameObject.SetActive(false);
                    joinedText.SetActive(true);
                    readyText.SetActive(false);
                    _eventSystem.SetSelectedGameObject(_joinButton.gameObject);
                    if (_characterSelected)
                    {
                        _characterSelected.enabled = true;
                    }

                    PlayerIsJoined[PlayerIndex] = false;
                    Debug.Log("le jooueur est parti");
                    Selector.SetActive(false);
                    transform.SetParent(canvas.transform);
                }
                else if (PlayerIsJoined.All(element => !element))
                {
                    if (_backButton != null)
                    {
                        _backButton.onClick?.Invoke();
                    }
                }
            }
        }


      

        public void StartHold()
        {
            if (CanHold)
            {
                Debug.Log("charge");
                IsCharging = true;
            }
        }

        public void StartRelease()
        {
            IsCharging = false;
            startTimer = 0;
        }


        public void PlayerReady()
        {
            if (CanJoin)
            {
                readyCofirmText.SetActive(true);
                readyText.SetActive(false);
                PlayerIsReady[PlayerIndex] = true;
                //bool allPlayersReady = true;
                int readyCount = 0;
                Debug.Log(PlayerIsReady[PlayerIndex]);

                for (int i = 0; i < PlayerIsJoined.Length; i++)
                {
                    if (PlayerIsJoined[i] == true) {
                        if (PlayerIsReady[i] == false)
                        {
                            //  allPlayersReady = false;
                        }
                        else {
                            readyCount++;
                            ConfirmChoice(PlayerIndex, _characterIndex);
                            

                            if (_characterSelected.name == "TurtleButton") {
                                TurtleIsSelected = true;
                                Debug.Log("turtle selectionnée");
                            }

                            Debug.Log("le joueur " + PlayerIndex + "est pret");
                            Debug.Log(readyCount + " joueurs pret");
                        }
                    }
                }
                
                
                if ( /*allPlayersReady == true && readyCount > _maxPlayers*/ readyCount >= _minPlayers && TurtleIsSelected)
                { 
                    CanHold = true;
                   turtleInfo.SetActive(false);
                    
                }
                else if (readyCount >= _minPlayers && !TurtleIsSelected)
                {
                    CanHold = false;
                    turtleInfo.SetActive(true); // show error message 
                }
                else
                {
                    CanHold = false;
                }

                if (CanHold)
                {
                    textInfo.SetActive(false);
                    startText.SetActive(true);
                }
            }
        }


        public void ConfirmChoice(int playerIndex, int characterIndex)
        {
            if (!DataManager.Instance.PlayerChoice.ContainsKey(PlayerIndex))
            {
                //DataManager.Instance.PlayerCharacter.Add(playerIndex, characterIndex);
                DataManager.Instance.PlayerChoice[playerIndex] = characterIndex;
            }
        }

        public void RemoveChoice(int playerIndex)
        {
            DataManager.Instance.PlayerChoice.Remove(playerIndex);
        }
    }
}
using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.SharedScene;
using Intégration.V2_REFACTO.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    public static Action OnPlayerStateChanged;
    public static bool PlayersSelectionConfirmed;
    public static Action OnPlayersCanceledSelection;

    [Header("Player manager")]
    [SerializeField] private PlayerInputManager _playerInputManager;
    [SerializeField] private Transform _playerUIParent;
    [SerializeField] private int _minPlayers;
    private bool AllPlayersReady; 
    
    [Header("Start Game Ui References")]
    [SerializeField] private GameObject _startgamePopUp;
    [SerializeField] private GameObject _choseCharacterPopUp;
    [SerializeField] private Image holdFillImage;  // Image de remplissage (UI)
    [SerializeField] private float holdTime = 1.5f;
    [SerializeField] private GameObject _choseMapPanel;
    private Tween fillTween;
    
    private bool isStartingGame = false;

    
    private Dictionary<InputDevice, PlayerInput> deviceToPlayerInput = new();
    private HashSet<PlayerInput> playerInputsList = new() ;
    private HashSet<GameObject> playersUiList = new();
    private bool _turtleIsSelected;
    private int _playersReadyCount;
    private HashSet<PlayerInput> holdingPlayers = new(); //Stocke les joueurs qui maintiennent "Start"

    private void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        //Détection des manettes déjà branchées au lancement
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad) { CheckCurrentGamepads(device); Debug.Log("manettes check");}
        }
        InputSystem.onDeviceChange += OnDeviceChange;
        OnPlayerStateChanged += CheckPlayersReady;
        OnPlayersCanceledSelection += OnCancelSelection;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        OnPlayerStateChanged -= CheckPlayersReady;
        OnPlayersCanceledSelection -= OnCancelSelection;
        
        foreach (var player in playerInputsList)
        {
            UnRegisterPlayer(player); 
        }
    }

    private void OnDestroy()
    {
        
    }


    private void CheckPlayersReady()
    {
        _playersReadyCount = 0;
        _turtleIsSelected = false;
       
        foreach (var player in playerInputsList)
        {
            PlayerSelection playerSelection = player.GetComponent<PlayerSelection>();

            if (playerSelection.CurrentState == PlayerState.Joined) //si un joueur n'est pas prêt
            {
                _startgamePopUp.SetActive(false);
                _choseCharacterPopUp.SetActive(true);
                AllPlayersReady = false;
                UpdateHoldProgress();
                return;
            }
            else if (playerSelection.CurrentState == PlayerState.Ready)
            {
                _playersReadyCount++;
                if (playerSelection.TurtleIsSelected)
                {
                    _turtleIsSelected = true;
                }
            }
        }
        if (_playersReadyCount < _minPlayers || !_turtleIsSelected) return; //si tous les joueurs sont prêts
        _choseCharacterPopUp.SetActive(false);
        _startgamePopUp.SetActive(true);
        AllPlayersReady = true;
    }
        
    
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad gamepad) return;
        if (change == InputDeviceChange.Added)
        {
            if (!PlayersSelectionConfirmed)
            {
                RumbleManager.Instance.RumblePulse(1f, 1f, 0.1f, gamepad);
                CheckCurrentGamepads(gamepad);
            }
        }
        else if (change == InputDeviceChange.Disconnected)
        {
          
            //if (!PlayersSelectionConfirmed) return;
            ReturnSelectionPanel();
            Debug.Log("Disconnected !!!!!!!!!!");
        }
    }

    
    private void CheckCurrentGamepads(InputDevice device)
    {
        // Verifie si la manette a deja été connectée
        if (deviceToPlayerInput.ContainsKey(device) || _playerInputManager.playerCount >= _playerInputManager.maxPlayerCount) return;
        
        Debug.Log($"Manette {device.displayName} branchée !");
        _playerInputManager.JoinPlayer(-1, -1, null, device);
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        InputDevice device = playerInput.devices[0];
        GameObject playerUi = playerInput.transform.parent.gameObject;
        playersUiList.Add(playerUi);
        playerUi.transform.SetParent(_playerUIParent);
        
        if (deviceToPlayerInput.ContainsKey(device))
        {
            Debug.Log(" check failed");
            Debug.Log($"Manette {device.displayName} reconnue !");
            //Destroy(playerInput.gameObject);
            return;
        }
        
        Debug.Log($"Nouvelle manette {device.displayName} !");
        deviceToPlayerInput[device] = playerInput;
        playerInputsList.Add(playerInput);
        RegisterPlayer(playerInput);
    }

    public void LoadGameSene()
    {
        foreach (var player in FindObjectsOfType<PlayerInput>())
        {
            UnRegisterPlayer(player);
        }
        if (!isStartingGame)
        {
            CustomSceneManager.Instance.LoadScene("Game");
            Debug.Log("start game");
            isStartingGame = true;
        }
    }
    
    #region Input Actions
    private void RegisterPlayer(PlayerInput player)
    {
        player.actions["StartGame"].performed += ctx => OnStartGame(player, ctx);
        player.actions["StartGame"].canceled += ctx => OnCancelGame(player, ctx);
        //player.actions["Cancel"].performed += ctx => OnCancelSelection(player, ctx);
    }
    
    private void UnRegisterPlayer(PlayerInput player)
    {
        player.actions["StartGame"].performed -= ctx => OnStartGame(player, ctx);
        player.actions["StartGame"].canceled -= ctx => OnCancelGame(player, ctx);
        //player.actions["Cancel"].performed -= ctx => OnCancelSelection(player, ctx);
    }
    

    private void OnStartGame(PlayerInput player, InputAction.CallbackContext context)
    {
        if (!AllPlayersReady) return;
        if (PlayersSelectionConfirmed) return;
        
       //if (isStartingGame) return;
       SoundManager.PlaySound(SoundType.Pressed, 0.3f);
        holdingPlayers.Add(player);
        UpdateHoldProgress();
    }

    private void OnCancelSelection()
    {
        Debug.Log("cancel ma gueule");
        if (PlayersSelectionConfirmed)
        {
            ReturnSelectionPanel();
        }
        else
        {
            foreach (var playerInput in playerInputsList)
            {
                PlayerSelection playerSelection = playerInput.GetComponent<PlayerSelection>();

                if (playerSelection.CurrentState != PlayerState.NotJoined)
                {
                    return;
                }
            }
            Debug.Log("cancel Selection");
            PlayersSelectionConfirmed = false;
            CustomSceneManager.Instance.LoadScene("Menu");
            
        }
       
    }

    private void ReturnSelectionPanel()
    {
        PlayersSelectionConfirmed = false;
        _choseMapPanel.SetActive(false);
        OnPlayerStateChanged.Invoke();
    }

    private void OnCancelGame(PlayerInput player, InputAction.CallbackContext context)
    {
        holdingPlayers.Remove(player);
        UpdateHoldProgress();
    }
    
    private void UpdateHoldProgress()
    {
        if (holdingPlayers.Count > 0 && AllPlayersReady)
        {
            if (fillTween != null && fillTween.IsActive()) return;
            
            holdFillImage.transform.parent.DOScale(1.1f, 0.5f);
            fillTween = holdFillImage.DOFillAmount(1f, holdTime)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    SoundManager.PlaySound(SoundType.Pressed, 0.3f);
                    _choseMapPanel.SetActive(true);
                    PlayersSelectionConfirmed = true;
                    Debug.Log(PlayerManager.PlayersSelectionConfirmed);
                    Debug.Log("LOAD GAME");
                });
        }
        else
        {
            holdFillImage.transform.parent.DOScale(1f, 0.5f);
            holdingPlayers.Clear();
            fillTween?.Kill();
            holdFillImage.DOFillAmount(0f, 1f).SetEase(Ease.OutQuad);
        }
    }

    #endregion
 
}

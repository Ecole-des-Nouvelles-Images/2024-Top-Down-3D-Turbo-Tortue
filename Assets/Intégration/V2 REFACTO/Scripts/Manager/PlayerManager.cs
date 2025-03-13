using System;
using System.Collections.Generic;
using Intégration.V1.Scripts.SharedScene;
using Intégration.V2_REFACTO.Scripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    public static Action<bool> OnPlayerStateChanged;
    
    [SerializeField] private PlayerInputManager _playerInputManager;
    [SerializeField] private Transform _playerUIParent;
    [SerializeField] private int _minPlayers;
    
    private Dictionary<InputDevice, PlayerInput> deviceToPlayerInput = new();
    private List<PlayerInput> playerInputsList = new List<PlayerInput>() ;
    private List<GameObject> playersUiList = new List<GameObject>();
    private bool _turtleIsSelected;
    private int _playersReadyCount;
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
        OnPlayerStateChanged += AllPlayersReady;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        OnPlayerStateChanged -= AllPlayersReady;
        ResetAllInputDevices();
    }


    private void AllPlayersReady(bool turtleIsSelected)
    {
        _playersReadyCount = 0; 
       
        foreach (var player in playerInputsList)
        {
            PlayerSelection playerSelection = player.GetComponent<PlayerSelection>();

            if (playerSelection.CurrentState == PlayerState.Joined) //si un joueur n'est pas prêt
            {
                Debug.Log("CANT START GAME  !!!!!!!!!!");
                Debug.Log("nombre de joueurs prets ; " +_playersReadyCount);
                Debug.Log( "tortue selectionnée ? : "+_turtleIsSelected);
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
        
        if (_playersReadyCount >= _minPlayers && _turtleIsSelected)
        {
            Debug.Log("CAN START GAME");
        }
        //si tous les joueurs sont prêts
        
        
        Debug.Log("nombre de joueurs prets ; " +_playersReadyCount);
        Debug.Log( "tortue selectionnée ? : "+_turtleIsSelected);
    }
        
    

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad gamepad) return;
        if (change == InputDeviceChange.Added)
        {
            RumbleManager.Instance.RumblePulse(1f, 1f, 0.1f, gamepad);
            CheckCurrentGamepads(gamepad);
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log("Disconnected");
           // ResetAllInputDevices();
        }
    }

    private void ResetAllInputDevices()
    {
        foreach (var playerInput in playerInputsList)
        {
            Destroy(playerInput.gameObject);
        }
        foreach (var playerUi in playersUiList)
        {
            Destroy(playerUi);
        }
        deviceToPlayerInput.Clear();
        playerInputsList.Clear();
        playersUiList.Clear();
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
    }
    
 
}

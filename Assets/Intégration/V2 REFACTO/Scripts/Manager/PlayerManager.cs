using System;
using System.Collections.Generic;
using Intégration.V1.Scripts.SharedScene;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    public static Action<PlayerInput> OnPlayerJoin;
    public static Action<PlayerInput> OnPlayerReady;
    public static Action<PlayerInput> OnPlayerCanceled;
    
    
    [SerializeField] private PlayerInputManager _playerInputManager;
    [SerializeField] private Transform _playerUIParent;
    [SerializeField] private int _minPlayers;
    [SerializeField] private bool _turtleIsSelected;
    
    
    
    private Dictionary<InputDevice, PlayerInput> deviceToPlayerInput = new();
    
    [SerializeField] private List<PlayerInput> playerInputsList = new List<PlayerInput>() ;
    private List<GameObject> playersUiList = new List<GameObject>();
    
    
    [SerializeField] private List<PlayerInput> playersReadyList;
    [SerializeField] private List<PlayerInput> playerJoinedList;
    
    
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
       
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        ResetAllInputDevices();
    }

    private void CheckReadyPlayers()
    {
        
        
        if (playersReadyList.Count >= _minPlayers)
        {
            //can start game. 
        }
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

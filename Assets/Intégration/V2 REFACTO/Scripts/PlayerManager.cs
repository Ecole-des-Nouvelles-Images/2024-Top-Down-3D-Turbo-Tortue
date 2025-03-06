using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    public static Action<PlayerInput> OnPlayerReady;
    [SerializeField] private PlayerInputManager _playerInputManager;
    [SerializeField] private Transform _playerUIParent;
    
    private Dictionary<InputDevice, PlayerInput> deviceToPlayerInput = new();
    [SerializeField] private List<PlayerInput> playerInputsList ;
    [SerializeField] private List<GameObject> playersUiList;
    private void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        //Détection des manettes déjà branchées au lancement
        foreach (var device in InputSystem.devices)
        {
            Debug.Log(" check failed");
            if (device is Gamepad) { CheckCurrentGamepads(device); Debug.Log("manettes check");}
        }
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        ResetAllInputDevices();
    }

    private void Start()
    {
      
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
        playerInput.gameObject.transform.parent.transform.SetParent(_playerUIParent);
        
        if (deviceToPlayerInput.ContainsKey(device))
        {
            Debug.Log($"Manette {device.displayName} reconnue !");
            //Destroy(playerInput.gameObject);
            return;
        }
        
        Debug.Log($"Nouvelle manette {device.displayName} !");
        deviceToPlayerInput[device] = playerInput;
        playerInputsList.Add(playerInput);
        
    }
    
 
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{ 
    [SerializeField] private PlayerInputManager _playerInputManager;
    [SerializeField] private Transform _playerUIParent;
    
    private Dictionary<InputDevice, PlayerInput> deviceToPlayerInput = new();

    private void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        //Détection des manettes déjà branchées au lancement
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad) { CheckCurrentGamepads(device); }
        }
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        
        if (device is not Gamepad gamepad) return;
        switch (change)
        {
            case InputDeviceChange.Added:
                RumbleManager.Instance.RumblePulse(1f, 1f, 0.1f,gamepad);
                CheckCurrentGamepads(gamepad);
                break;
        }
    }

    private void CheckCurrentGamepads(InputDevice device)
    {
        // Verifie si la manette a deja été connectée
        if (deviceToPlayerInput.ContainsKey(device)) return;
        
        Debug.Log($"Manette {device.displayName} branchée !");
        _playerInputManager.JoinPlayer(-1, -1, null, device);
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        InputDevice device = playerInput.devices[0];
        playerInput.gameObject.transform.parent.transform.SetParent(_playerUIParent);
        
        if (deviceToPlayerInput.ContainsKey(device))
        {
            Debug.Log($"Manette {device.displayName} reconnue !");
            Destroy(playerInput.gameObject);
            return;
        }

        Debug.Log($"Nouvelle manette {device.displayName} !");
        deviceToPlayerInput[device] = playerInput;
    }
    
 
}

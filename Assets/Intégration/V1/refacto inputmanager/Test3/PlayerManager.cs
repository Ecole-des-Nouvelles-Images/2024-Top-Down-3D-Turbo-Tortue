using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{ 
    public PlayerInputManager playerInputManager;
    public Transform joinUIParent;
    
    private Dictionary<InputDevice, PlayerInput> deviceToPlayerInput = new();

    private void Awake()
    {
       // playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
       //Détection des manettes déjà branchées au lancement
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad && !deviceToPlayerInput.ContainsKey(device))
            {
                Debug.Log($"Manette {device.displayName} déjà branchée au lancement !");
                playerInputManager.JoinPlayer(-1, -1, null, device);
            }
            
        }
        
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        
        if (!(device is Gamepad gamepad)) return;
       
        switch (change)
        {
            case InputDeviceChange.Added:
                RumbleManager.Instance.RumblePulse(1f, 1f, 0.1f,(Gamepad)device);
                if (!deviceToPlayerInput.ContainsKey(device))
                {
                    Debug.Log($"Manette {device.displayName} branchée !");
                    playerInputManager.JoinPlayer(-1, -1, null, device);
                } 
                break;
        }
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        InputDevice device = playerInput.devices[0];
        playerInput.gameObject.transform.parent.transform.SetParent(joinUIParent);
        if (deviceToPlayerInput.ContainsKey(device))
        {
            Debug.Log($"Manette {device.displayName} reconnue !");
            Destroy(playerInput.gameObject);
            return;
        }

        Debug.Log($"Nouvelle manette {device.displayName} !");
        deviceToPlayerInput[device] = playerInput;
        
        Debug.Log("player instancié");
    }
    
 
}

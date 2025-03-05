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
    public GameObject joinUIPrefab;
    public Transform joinUIParent;

    private Dictionary<InputDevice, GameObject> deviceToUI = new();
    private Dictionary<InputDevice, PlayerInput> deviceToPlayerInput = new();

    private void Awake()
    {
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        // 🔥 Détection des manettes déjà branchées au lancement
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
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (!(device is Gamepad)) return;

        switch (change)
        {
            case InputDeviceChange.Added:
                if (!deviceToPlayerInput.ContainsKey(device))
                {
                    Debug.Log($"Manette {device.displayName} branchée !");
                    playerInputManager.JoinPlayer(-1, -1, null, device);
                }
                break;

            case InputDeviceChange.Disconnected:
                if (deviceToPlayerInput.ContainsKey(device))
                {
                    //OnDeviceLost(deviceToPlayerInput[device]);
                }
                break;

            case InputDeviceChange.Reconnected:
                if (deviceToPlayerInput.ContainsKey(device))
                {
                   // OnDeviceRegained(deviceToPlayerInput[device]);
                }
                break;
        }
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        InputDevice device = playerInput.devices[0];

        if (deviceToPlayerInput.ContainsKey(device))
        {
            Debug.Log($"Manette {device.displayName} reconnue !");
          //  OnDeviceRegained(playerInput);
            Destroy(playerInput.gameObject);
            return;
        }

        Debug.Log($"Nouvelle manette {device.displayName} !");
        deviceToPlayerInput[device] = playerInput;
       // playerInput.onDeviceLost += OnDeviceLost;
        //playerInput.onDeviceRegained += OnDeviceRegained;

        GameObject ui = Instantiate(joinUIPrefab, joinUIParent);
        Debug.Log("player instancié");
        deviceToUI[device] = ui;
       // ui.SetActive(false);
    }

    /* private void OnDeviceLost(PlayerInput playerInput)
     {
         InputDevice device = playerInput.devices[0];
 
         Debug.Log($"Manette {device.displayName} déconnectée !");
         if (deviceToUI.ContainsKey(device))
         {
             deviceToUI[device].SetActive(true);
         }
     }
 
     private void OnDeviceRegained(PlayerInput playerInput)
     {
         InputDevice device = playerInput.devices[0];
 
         Debug.Log($"Manette {device.displayName} reconnectée !");
         if (deviceToUI.ContainsKey(device))
         {
             deviceToUI[device].SetActive(false);
         }
     }*/
  
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputUserManager : MonoBehaviour
{
    public InputDevice AssignedDevice { get; private set; }
    private PlayerInput playerInput;
    private InputUser user;
    private bool userInitialized = false;
   

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

  

    public void Initialize(InputDevice device)
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (device != null && playerInput != null)
        {
            AssignedDevice = device;
            user = InputUser.CreateUserWithoutPairedDevices();
            InputUser.PerformPairingWithDevice(device, user);
            user.AssociateActionsWithUser(playerInput.actions);
            userInitialized = true;
        }
        else
        {
            Debug.LogWarning("[InputUserManager] Impossible d'initialiser l'utilisateur (device ou PlayerInput manquant).");
        }
    }

    private void OnEnable()
    {
        if (userInitialized && playerInput != null && !playerInput.user.valid)
        {
            if (AssignedDevice != null && !user.pairedDevices.Contains(AssignedDevice))
            {
                InputUser.PerformPairingWithDevice(AssignedDevice, user);
            }

            user.AssociateActionsWithUser(playerInput.actions);
        }
    }

  
}


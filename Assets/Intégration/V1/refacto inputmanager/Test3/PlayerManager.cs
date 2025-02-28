using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
 
    public GameObject playerUIPrefab; // Prefab de l'UI Player avec "Rejoindre"
    public Transform playerSlotsParent; // Parent qui contient les slots UI

    private Dictionary<InputDevice, GameObject> playerMap = new Dictionary<InputDevice, GameObject>();
    private Dictionary<InputDevice, int> playerCharacterMap = new Dictionary<InputDevice, int>(); // Stocke le personnage choisi

    void Start()
    {
        
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        Debug.Log($"Player Index: {playerInput.playerIndex}"); // Index logique
        Debug.Log($"InputUser ID: {playerInput.user.id}");      // User interne
    }
    private void Awake()
    {
        InputSystem.onDeviceChange += OnDeviceChanged;
        DetectConnectedControllers();
    }

    private void DetectConnectedControllers()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad /*&& !playerMap.ContainsKey(device)*/)
            {
                AddPlayer(device);
               // Debug.Log(InputUser.GetUnpairedInputDevices());
            }
        }
        
    }

    private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
       
        if (device is Gamepad)
        {
            if (change == InputDeviceChange.Added && !playerMap.ContainsKey(device))
            {
                AddPlayer(device);
                Debug.Log("unpairs devices : "+ InputUser.GetUnpairedInputDevices());
            }
            else if (change == InputDeviceChange.Removed /*&& playerMap.ContainsKey(device)*/)
            {
                RemovePlayer(device);
                Debug.Log("unpairs devices : "+ InputUser.GetUnpairedInputDevices());
            }
        }
    }

    private void AddPlayer(InputDevice device)
    {
        GameObject slot = GetFreeSlot();
        if (slot != null)
        {
            GameObject playerUI = Instantiate(playerUIPrefab, slot.transform);
            PlayerJoinUI joinUI = playerUI.GetComponent<PlayerJoinUI>();
            joinUI.SetPlayer(device);
          //  joinUI.OnCharacterSelected += StoreCharacterChoice; // Abonnement à l'événement de choix de personnage
            playerUI.SetActive(true);
            playerMap.Add(device, playerUI);
        }
    }

    private void StoreCharacterChoice(InputDevice device, int characterIndex)
    {
        if (playerCharacterMap.ContainsKey(device))
        {
            playerCharacterMap[device] = characterIndex;
        }
        else
        {
            playerCharacterMap.Add(device, characterIndex);
        }
    }

    public Dictionary<InputDevice, int> GetCharacterChoices()
    {
        return playerCharacterMap;
    }

    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void RemovePlayer(InputDevice device)
    {
        if (playerMap.TryGetValue(device, out GameObject playerUI))
        {
          //  playerUI.GetComponent<PlayerJoinUI>().ResetUI();
            playerUI.SetActive(false);
            playerMap.Remove(device);
            playerCharacterMap.Remove(device);
        }
    }

    private GameObject GetFreeSlot()
    {
        foreach (Transform slot in playerSlotsParent)
        {
            if (slot.childCount == 0)
            {
                return slot.gameObject;
            }
        }
        return null;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerUIPrefab;
    public Transform playerUIParent;

    private List<GameObject> playerUIs = new List<GameObject>();
    private Dictionary<PlayerInput, GameObject> playerMap = new Dictionary<PlayerInput, GameObject>();

    private void Awake()
    {
        PlayerInputManager.instance.onPlayerJoined += AddPlayer;
        PlayerInputManager.instance.onPlayerLeft += RemovePlayer;
    }

    void AddPlayer(PlayerInput playerInput)
    {
        GameObject slot = GetFreeSlot();
        if (slot != null)
        {
            playerMap[playerInput] = slot;
            slot.GetComponent<PlayerJoinUI>().SetPlayer(playerInput);
            slot.GetComponent<PlayerJoinUI>().Join();
            
        }
    }

    void RemovePlayer(PlayerInput playerInput)
    {
        if (playerMap.ContainsKey(playerInput))
        {
            playerMap[playerInput].GetComponent<PlayerJoinUI>().Disconnect();
            playerMap.Remove(playerInput);
        }
    }

    GameObject GetFreeSlot()
    {
        foreach (GameObject slot in playerUIs)
        {
            if (!slot.GetComponent<PlayerJoinUI>().isJoined)
            {
                return slot;
            }
        }
        GameObject newSlot = Instantiate(playerUIPrefab, playerUIParent);
        playerUIs.Add(newSlot);
        return newSlot;
    }
}

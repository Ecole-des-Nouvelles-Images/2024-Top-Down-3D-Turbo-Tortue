using System.Collections;
using System.Collections.Generic;
using Michael.Scripts.Manager;
using UnityEngine;

public class PlayerSelectionManager : MonoBehaviourSingleton<PlayerSelectionManager>
{
    private List<PlayerData> players = new List<PlayerData>();
    
    public void AddPlayer(PlayerData player)
    {
        players.Add(player);
    }

    public List<PlayerData> GetPlayerData()
    {
        return players;
    }

    public void ClearPlayers()
    {
        players.Clear();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayersSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // Points de spawn
    [SerializeField] private GameObject[] playerPrefabs; // Prefabs des personnages

    private void Start()
    {
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        List<PlayerData> players = PlayerSelectionManager.Instance.GetPlayerData();

        for (int i = 0; i < players.Count; i++)
        {
            if (i >= spawnPoints.Length)
            {
                Debug.LogWarning("Pas assez de points de spawn !");
                continue;
            }

            PlayerData player = players[i];
            GameObject prefab = playerPrefabs[player.CharacterID];

            // Instancier le joueur
            GameObject playerInstance = Instantiate(prefab, spawnPoints[i].position, spawnPoints[i].rotation);

            // Configurer l'InputUser et PlayerInput
            PlayerInput playerInput = playerInstance.GetComponent<PlayerInput>();
            InputUser.PerformPairingWithDevice(player.Device);

            //playerInput.SwitchCurrentControlScheme(user.controlScheme, user.pairedDevices.ToArray());

            Debug.Log($"Player {player.PlayerID} spawned with character {player.CharacterID} and device {player.Device.displayName}");
        }
    }
}

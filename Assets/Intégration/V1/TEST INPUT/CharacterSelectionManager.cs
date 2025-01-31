using System;
using System.Collections;
using System.Collections.Generic;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

[System.Serializable]
public class PlayerData
{
    public int PlayerID;             // ID unique du joueur (PlayerIndex)
    public int CharacterID;          // ID du personnage sélectionné
    public InputDevice Device;       // Périphérique associé au joueur
}

public class CharacterSelectionManager : MonoBehaviourSingleton<CharacterSelectionManager>
{
    public GameObject[] characterPrefabs;
   private Dictionary<int, PlayerData> playerChoices = new Dictionary<int, PlayerData>();

   

    // Lorsque le joueur rejoint
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int playerIndex = playerInput.playerIndex;

        // Créer un nouveau joueur
        var playerData = new PlayerData
        {
            PlayerID = playerIndex,
            CharacterID = 0, // Par défaut, premier personnage
            Device = playerInput.devices[0] // Associer le périphérique
        };

        // Ajouter le joueur au dictionnaire
        playerChoices[playerIndex] = playerData;

        Debug.Log($"Player {playerIndex} joined with device: {playerInput.devices[0].displayName}");
    }

    // Lorsque le joueur quitte
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        int playerIndex = playerInput.playerIndex;

        // Supprimer le joueur de la liste de sélection
        if (playerChoices.ContainsKey(playerIndex))
        {
            playerChoices.Remove(playerIndex);
            Debug.Log($"Player {playerIndex} left. Removed from selection.");
        }

      
    }
    
    // Cette méthode est appelée lorsque le joueur sélectionne un personnage
    public void OnCharacterSelected(int playerIndex, int characterID)
    {
        // Mettre à jour le personnage choisi pour ce joueur
        if (playerChoices.ContainsKey(playerIndex))
        {
            playerChoices[playerIndex].CharacterID = characterID;
            Debug.Log($"Player {playerIndex} selected character {characterID}");
        }
    }

    // Exemple pour démarrer le jeu avec les joueurs sélectionnés
    public void OnStartGame()
    {
        foreach (var player in playerChoices.Values)
        {
            // Ici, tu pourrais procéder à l'instantiation de ton joueur avec le personnage choisi
            Debug.Log($"Starting game with Player {player.PlayerID}, Character {player.CharacterID}");
        }

        // Charger la scène de jeu
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}

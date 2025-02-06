using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private int selectedCharacterIndex = 0;
    private PlayerInput _playerInput;
    private int _playerIndex;

    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerIndex = _playerInput.playerIndex;
    }

    public void SelectNextCharacter()
    {
        selectedCharacterIndex++;
        // Gérer les boucles pour les personnages
        selectedCharacterIndex %= CharacterSelectionManager.Instance.characterPrefabs.Length;

        Debug.Log($"Player {GetComponent<PlayerInput>().playerIndex} selected character {selectedCharacterIndex}");
    }

    public void SelectPreviousCharacter()
    {
        selectedCharacterIndex--;
        if (selectedCharacterIndex < 0)
            selectedCharacterIndex = CharacterSelectionManager.Instance.characterPrefabs.Length - 1;

        Debug.Log($"Player {GetComponent<PlayerInput>().playerIndex} selected character {selectedCharacterIndex}");
    }

    public void ConfirmSelection()
    {
        CharacterSelectionManager.Instance.OnCharacterSelected(_playerIndex, selectedCharacterIndex);
    }
    
    
    public void OnDeviceLost()
    {
      CharacterSelectionManager.Instance.OnPlayerLeft(_playerInput);
      Destroy(gameObject,0.5f);
    }
    
    

}

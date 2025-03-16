using System;
using System.Collections;
using System.Collections.Generic;
using Intégration.V1.Scripts.Game.FeedBack;
using Intégration.V2_REFACTO.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorPlayer : MonoBehaviour
{
    [Header("Grid UI References")]
    [SerializeField] private Button[] _characterButtons;
    
    [SerializeField] private PlayerSelection _playerSelection;
   
    private Image _cursorImage;
    private EventSystem _eventSystem;
    private GameObject _lastGameObjectSelected;
    private GameObject _currentObjectSelected;

    private void Awake()
    {
        _cursorImage = GetComponent<Image>();
        _eventSystem = transform.parent.GetComponentInChildren<EventSystem>();
        _playerSelection = transform.parent.GetComponent<PlayerSelection>();
        _characterButtons = _playerSelection.CharacterButtons;
    }

    private void Start()
    {
       
    }

    private void OnEnable()
    {
        if (!GetComponentInParent<RadialGridLayoutGroup>())
        {
            MoveSelectorPosition();
        }
        _playerSelection.OnCursorMoved += UpdateCursorPosition;
        _playerSelection.OnPlayerReady += MoveOtherSelector;
    }

    private void OnDisable()
    {
        _playerSelection.OnCursorMoved -= UpdateCursorPosition;
        _playerSelection.OnPlayerReady -= MoveOtherSelector;
    }


    private void MoveSelectorPosition()
    {
        var buttonWithNoChildren = FindButtonWithNoChildren(_characterButtons);
        _eventSystem.SetSelectedGameObject(buttonWithNoChildren.gameObject);
        UpdateCursorPosition(buttonWithNoChildren.gameObject);
    }

    private Button FindButtonWithNoChildren(Button[] buttonList)
    {
        foreach (var button in buttonList)
        {
            if (button.GetComponentInChildren<RadialGridLayoutGroup>().transform.childCount == 0)
            {
                return button;
            }
        }
        return null;
    }

    private void UpdateCursorPosition(GameObject selectedObject)
    {
        RadialGridLayoutGroup radialLayout = selectedObject?.GetComponentInChildren<RadialGridLayoutGroup?>();
        transform.SetParent(radialLayout?.transform);
        _cursorImage.color = selectedObject.GetComponentInChildren<UISelectionFeedBack>().OutlineColor;
        radialLayout.enabled = false;
        radialLayout.enabled = true;
        transform.localScale = Vector3.one;
    }
    
    
    private void MoveOtherSelector(RadialGridLayoutGroup characterSelected, GameObject cursorUi)
    {
        for (int i = 0; i < characterSelected.transform.childCount; i++)
        {
            Transform childTransform = characterSelected.transform.GetChild(i);

            if (childTransform != cursorUi.transform)
            { 
                childTransform.gameObject.GetComponent<CursorPlayer>().MoveSelectorPosition();
                childTransform.gameObject.GetComponent<CursorPlayer>()._playerSelection.UpdateIconCharacter();
            }
        }
    }
    
}
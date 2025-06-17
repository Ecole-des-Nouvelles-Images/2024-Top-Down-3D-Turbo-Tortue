using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerUI _playerUI;
    public static Action OnBackPressed;

    private void Awake()
    {
        _playerUI = new PlayerUI();
    }

    private void OnEnable()
    {
        _playerUI.UI.Enable();
        _playerUI.UI.Cancel.performed += ctx => OnBackPressed?.Invoke();
    }

    private void OnDisable()
    {
        _playerUI.UI.Cancel.performed -= ctx => OnBackPressed?.Invoke();
        _playerUI.UI.Disable();
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGamePadManager : MonoBehaviour
{
    [SerializeField] private InputAction joinAction;
    [SerializeField] private List<PlayerInput> disabledPlayerInputs;
    private int _nextPlayerinput;
    private void Awake()
    {
      joinAction.Enable();
      joinAction.performed += context => JoinAction(context);
    }

    private void Start()
    {
        List<Gamepad> gamepads = Gamepad.all.ToList();
        for (int i = 0; i < gamepads.Count; i++) {
            disabledPlayerInputs[i].gameObject.SetActive(true);
        }
    }

    void JoinAction(InputAction.CallbackContext context)
    {
        if (_nextPlayerinput < disabledPlayerInputs.Count)
        {
            while (disabledPlayerInputs[_nextPlayerinput].gameObject.activeInHierarchy)
            {
                _nextPlayerinput++;
                if (_nextPlayerinput >= disabledPlayerInputs.Count)
                {
                    _nextPlayerinput = 0;
                }
            }
            disabledPlayerInputs[_nextPlayerinput].gameObject.SetActive(true);

            _nextPlayerinput++;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DevicesManager : MonoBehaviour
{
    [SerializeField] private GameObject _joinPanel;
    [SerializeField] private Transform _joinlayout;
    private PlayerInputManager _playerInputManager;
    void Start()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();
        DetectDevices();
    }


    void DetectDevices()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad)
            {
                Debug.Log(device.name + device.deviceId );
                Instantiate(_joinPanel, _joinlayout);
               _playerInputManager.JoinPlayer(pairWithDevice: device);
            }
        }
    }
}

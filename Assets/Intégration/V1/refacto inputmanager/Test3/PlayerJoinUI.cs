using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerJoinUI : MonoBehaviour
{
    [SerializeField] private GameObject _playerUi;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private int _userIndex;
    [SerializeField] private TextMeshProUGUI _playerIndexText;
    [SerializeField] private Image _playerIndeximage;
    [SerializeField] private Gamepad _gamepad;
    private void Awake()
    {
        _playerUi = transform.parent.gameObject;
        playerInput = transform.parent.GetComponentInChildren<PlayerInput>();
        _userIndex = playerInput.user.index;
        _playerIndexText.text = (_userIndex + 1).ToString() ;
        _playerIndeximage.color = new Color(Random.value, Random.value, Random.value);
        _gamepad = playerInput.devices[0] as Gamepad;
    }

    private void Start()
    {
        playerInput.gameObject.transform.SetParent(playerInput.transform.parent.parent);
        Join();
    }
    
    public void Join()
    {
        Debug.Log("Join");
        _playerUi.transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            _playerUi.transform.DOScale(Vector3.one, 0.1f);
        });
    }
    
    public void OnDeviceLost()
    {
        Debug.Log("OnDeviceLost");
        _playerUi.SetActive(false); 
    }
    
    public void OnDeviceRegained()
    {
        Debug.Log("OndeviceRegained");
        _playerUi.SetActive(true); 
        Join();
    }

 
   
}
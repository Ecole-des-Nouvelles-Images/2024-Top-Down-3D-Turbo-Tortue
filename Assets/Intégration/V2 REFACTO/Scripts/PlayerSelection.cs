using System;
using System.IO.Enumeration;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerSelection : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _playerIndexImage;
    [SerializeField] private GameObject _playerUi;
    [SerializeField] private TextMeshProUGUI _playerIndexText;
    [SerializeField] private GameObject _joinVisuals;
    [SerializeField] private GameObject _flowerVisuals;
    
    
    private PlayerInput _playerInput;
    private int _userIndex;
    private RectTransform _globalPanel;
    private readonly Vector3 _playerUiScale = new Vector3(0.5f, 0.5f, 0.5f);

    private bool _isReady; 
    private bool _isJoining;
    
    
    private void Awake()
    {
        _globalPanel = GameObject.Find("GlobalPanel").GetComponent<RectTransform>();
        _playerUi = transform.parent.gameObject;
        _playerInput = transform.parent.GetComponentInChildren<PlayerInput>();
        _userIndex = _playerInput.user.index;
        _playerIndexText.text = (_userIndex + 1).ToString() ;
        _playerIndexImage.color = new Color(Random.value, Random.value, Random.value);
    }

    
    private void Start()
    {
        _playerInput.gameObject.transform.SetParent(_playerInput.transform.parent.parent);
        GamepadConnectionFeedback();
    }

    private void GamepadConnectionFeedback()
    {
        _playerUi.transform.localScale = _playerUiScale;
        _playerUi.transform.DOScale(Vector3.one, 0.1f);
        CameraShake();
    }

    private void GamepadResetFeedBack()
    { 
        _playerUi.transform.localScale = _playerUiScale;
    }
    
    public void OnNavigate()
    {
        
    }
    

    public void OnSubmit()
    {
        if (!_isJoining)
        {
            PlayerJoined();
        }
        else if (!_isReady)
        {
            PlayerReady();
        }
       
    }

    public void OnCancel()
    {
        Debug.Log("CANCEL");
        if (_isJoining)
        {
            PlayerJoined();
        }
        else if (_isReady)
        {
            PlayerReady();
        }
    }

    
    
    private void PlayerJoined()
    {
        Debug.Log("JOINING");
        _flowerVisuals.SetActive(true);
        _joinVisuals.SetActive(false);
    }
    
    private void PlayerReady()
    {
        Debug.Log("Ready");
    }
    
    public void OnDeviceLost()
    {
        Debug.Log("OnDeviceLost");
        _playerUi.SetActive(false); 
    }

    public void OnDeviceRegained()
    {
        Debug.Log("OndeviceRegained");
        GamepadConnectionFeedback();
        _playerUi.SetActive(true); 
    }

    private void CameraShake()
    { 
        _globalPanel.DOShakePosition(0.2f, 5f, 10, 5,false, true);
    }
 
   
}
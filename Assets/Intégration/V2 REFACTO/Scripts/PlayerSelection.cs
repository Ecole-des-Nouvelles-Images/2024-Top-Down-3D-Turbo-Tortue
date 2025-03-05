using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerSelection : MonoBehaviour
{
    [SerializeField] private RectTransform _canvas;
    [SerializeField] private GameObject _playerUi;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private int _userIndex;
    [SerializeField] private TextMeshProUGUI _playerIndexText;
    [SerializeField] private Image _playerIndeximage;
    private readonly Vector3 _playerUiScale = new Vector3(0.5f, 0.5f, 0.5f);
    private void Awake()
    {
        _playerUi = transform.parent.gameObject;
        _playerUi.transform.localScale = _playerUiScale;
        playerInput = transform.parent.GetComponentInChildren<PlayerInput>();
        _userIndex = playerInput.user.index;
        _playerIndexText.text = (_userIndex + 1).ToString() ;
        _playerIndeximage.color = new Color(Random.value, Random.value, Random.value);
    }

    private void Start()
    {
        playerInput.gameObject.transform.SetParent(playerInput.transform.parent.parent);
        GamepadConnectionFeedback();
    }

    private void GamepadConnectionFeedback()
    {
        _playerUi.transform.DOScale(Vector3.one, 0.1f);
    }

    private void GamepadResetFeedBack()
    {
        _playerUi.transform.DOScale(_playerUiScale, 0.1f);
    }
    
    public void OnDeviceLost()
    {
        Debug.Log("OnDeviceLost");
        GamepadResetFeedBack();
        _playerUi.SetActive(false); 
    }
    
    public void OnNavigate()
    {
        Debug.Log("NAVIGATE");
    }
    
    public void PlayerJoined()
    {
       
    }

    public void OnSubmit()
    {
       
        Debug.Log("JOINING");
    }

    public void OnCancel()
    {
        Debug.Log("CANCEL");
    }

    public void OnReady()
    {
        
    }

    public void OnDeviceRegained()
    {
        Debug.Log("OndeviceRegained");
        _playerUi.SetActive(true); 
        GamepadConnectionFeedback();
    }

    private void CameraShake()
    { 
        _canvas.DOShakePosition(0.5f, 5f, 10, 5,false, true);
    }
 
   
}
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class PlayerJoinUI : MonoBehaviour
{
    public TextMeshProUGUI playerText;
    public GameObject avatar;
    public GameObject joinPrompt;

    private PlayerInput playerInput;
    public bool isJoined = false;

    public void SetPlayer(PlayerInput input)
    {
        playerInput = input;
        playerText.text = $"Player {input.playerIndex + 1}";
    }

    public void Join()
    {
        isJoined = true;
        joinPrompt.SetActive(false);
        avatar.SetActive(true);
        transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.1f);
        });
        
        // Vibration manette
        if (playerInput.devices.Count > 0 && playerInput.devices[0] is Gamepad gamepad)
        {
            gamepad.SetMotorSpeeds(0.5f, 0.5f);
            Invoke(nameof(StopVibration), 0.2f);
        }
       
    }

    public void Disconnect()
    {
        isJoined = false;
        joinPrompt.SetActive(true);
        avatar.SetActive(false);
    }
    
    void StopVibration()
    {
        if (playerInput.devices.Count > 0 && playerInput.devices[0] is Gamepad gamepad)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }
}

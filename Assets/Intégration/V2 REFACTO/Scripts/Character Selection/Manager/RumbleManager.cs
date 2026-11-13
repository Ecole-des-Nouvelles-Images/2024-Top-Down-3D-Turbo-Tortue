using System.Collections;
using System.Collections.Generic;
using Intégration.V1.Scripts.SharedScene;
using Michael.Scripts.Manager;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using CharacterController = Intégration.V1.Scripts.Game.Characters.CharacterController;

public class RumbleManager : MonoBehaviourSingleton<RumbleManager>
{
    private Coroutine _stopRumbleCoroutine;

    public void RumblePulse(Gamepad gamepad, float lowFrequency = 0.6f, float highFrequency = 0.6f, float duration = 0.1f)
    {
        if (gamepad == null || !DataManager.Instance.CanVibrate) return;
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        _stopRumbleCoroutine = StartCoroutine(StopRumblePulse(duration, gamepad));
    }

    private IEnumerator StopRumblePulse(float duration, Gamepad gamepad)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (gamepad != null) gamepad.SetMotorSpeeds(0f, 0f);
    }

    public void RumbleLoop(Gamepad gamepad, float lowFrequency = 1, float highFrequency = 1)
    {
        if (gamepad == null || !DataManager.Instance.CanVibrate) return;
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
    }

    public void StopRumbleLoop(Gamepad gamepad)
    {
        if (gamepad == null) return;
        gamepad.SetMotorSpeeds(0f, 0f);
    }

    public void RumbleAllGamepad(float lowFrequency = 1f, float highFrequency = 1f, float duration = 0.1f)
    {
        if (!DataManager.Instance.CanVibrate) return;
        foreach (GameObject player in GameManager.Instance.Players)
        {
            Gamepad pad = player.GetComponent<CharacterController>().Gamepad;
            RumblePulse(pad,lowFrequency,highFrequency,duration);
        }
    }
}

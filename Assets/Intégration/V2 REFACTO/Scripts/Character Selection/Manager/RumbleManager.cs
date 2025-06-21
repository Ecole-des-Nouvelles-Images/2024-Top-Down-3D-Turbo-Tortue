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
        if (!DataManager.CanVibrate) return;
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
        gamepad.SetMotorSpeeds(0f, 0f);
    }

    
    public void RumbleLoop(Gamepad gamepad, float lowFrequency = 1, float highFrequency = 1)
    {
        if (!DataManager.CanVibrate) return;
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
    }
    
    public void StopRumbleLoop(Gamepad gamepad)
    {
        gamepad.SetMotorSpeeds(0f, 0f);
    }

    /*public void StopAllVibrations()
    {
        // Stop toutes les manettes vibrantes
        foreach (var pad in Gamepad.all)
        {
            pad.SetMotorSpeeds(0f, 0f);
        }
        
        // Si une coroutine est en cours, l'arrêter
        if (_stopRumbleCoroutine != null)
        {
            StopAllCoroutines(); // Coupe toutes les coroutines
            _stopRumbleCoroutine = null;
        }
    }*/
    
    public void RumbleAllGamepad(float lowFrequency = 1f, float highFrequency = 1f, float duration = 0.1f)
    {
        if (!DataManager.CanVibrate) return;
        foreach (GameObject player in GameManager.Instance.Players)
        {
            Gamepad pad = player.GetComponent<CharacterController>()._gamepad;
            RumblePulse(pad,lowFrequency,highFrequency,duration);
        }
    }
    
   
}

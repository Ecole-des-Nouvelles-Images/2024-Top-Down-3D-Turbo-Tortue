using System.Collections;
using Michael.Scripts.Manager;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : MonoBehaviourSingleton<RumbleManager>
{
    private Coroutine _stopRumbleCoroutine;
    
    public void RumblePulse(float lowFrequency, float highFrequency, float duration, Gamepad gamepad)
    {
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

   
    
    public void StopAllVibrations()
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
    }
}

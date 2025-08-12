using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Noah.Scripts
{
    public class RumbleManager : MonoBehaviour
    {
        public static RumbleManager Instance;

        private Gamepad pad;

        private Coroutine stopRumbleAfterTimeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void RumblePulse(float lowFrequency, float highFrequency, float duration)
        {
            pad = Gamepad.current;

            if (pad != null)
            {
                pad.SetMotorSpeeds(lowFrequency, highFrequency);

                stopRumbleAfterTimeCoroutine = StartCoroutine(StopRumble(duration, pad));
            }
        }

        private IEnumerator StopRumble(float duration, Gamepad pad)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            pad.SetMotorSpeeds(0f, 0f);
        }
    }
    
}

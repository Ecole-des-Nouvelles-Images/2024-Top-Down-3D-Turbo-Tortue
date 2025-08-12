using Intégration.V1.Scripts.UI;
using Michael.Scripts.Manager;
using Michael.Scripts.Ui;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace Intégration.V1.Scripts.Game.Characters
{
    public class WhiteLysController : FlowerController
    {
        [SerializeField] private VisualEffect _shareEnergy;
        [SerializeField] private float _speedBoostDuration;
        [SerializeField] private float _speedBonus;
        [SerializeField] private ParticleSystem _boostParticules;
        private bool _isBoosted;

        protected override void MainCapacity()
        {
            //give his sun to other flowers
            if (Sun > 0)
            {
                _shareEnergy.Play();
                AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersVoices);
                foreach (GameObject floweralive in GameManager.Instance.FlowersAlive)
                {
                    if (floweralive != gameObject)
                    {
                        if (floweralive.GetComponent<FlowerController>().Sun < 3)
                        {
                            if (floweralive.GetComponent<FlowerController>().GatherEnergy != null)
                            {
                                floweralive.GetComponent<FlowerController>().GatherEnergy.Play();
                            }

                            floweralive.GetComponent<FlowerController>().AddSun(Sun);
                            RumbleManager.Instance.RumblePulse(Gamepad);
                           
                        }
                    }
                }
                Sun = 0;
            }
        }


        protected override void PassiveCapacity()
        {
            throw new System.NotImplementedException(); // Revive cost 1. 
        }

        protected override void ThirdCapacity()
        {
            _playerStats.flowersRevived++;
            deadFlowerController.GetRevive();
            canReanimate = false;

            if (_isBoosted) return;
            
            moveSpeed += _speedBonus;
            _isBoosted = true;
            _boostParticules.Play();
            Invoke(nameof(CancelSpeedBoost),_speedBoostDuration);
        }

        private void CancelSpeedBoost()
        {
            _isBoosted = false;
            moveSpeed = normalMoveSpeed;
            _boostParticules.Stop();
        }

        public override void OnThirdCapacity(InputAction.CallbackContext context)
        {
            if (canReanimate && !IsStunned && !PauseControlller.IsPaused)
            {
                if (context.started)
                {
                    isCharging = true;
                }
                else if (context.canceled)
                {
                    isCharging = false;
                    reanimateTimer = 0;
                }
            }
        }
        

    }
}
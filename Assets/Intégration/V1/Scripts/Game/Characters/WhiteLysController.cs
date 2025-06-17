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
        [SerializeField] private VisualEffect shareEnergy;

        protected override void MainCapacity()
        {
            //give his sun to other flowers
            if (Sun > 0)
            {
                shareEnergy.Play();
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
            /*  sun =- 1;
              if (sun < 0) {
                  sun = 0;

              }*/
            deadFlowerController.GetRevive();
            canReanimate = false;
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
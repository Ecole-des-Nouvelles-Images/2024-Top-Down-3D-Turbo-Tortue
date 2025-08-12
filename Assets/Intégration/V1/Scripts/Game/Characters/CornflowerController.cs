using Michael.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Intégration.V1.Scripts.Game.Characters
{
    public class CornflowerController : FlowerController
    {
        [SerializeField] Shield _Shield;

        protected override void MainCapacity()
        {
            if (Sun >= CapacityCost && !IsPlanted)
            {
                AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersVoices);
                _Shield.OpenShield();
                OnLooseSunCapacity(CapacityCost);
                RumbleManager.Instance.RumblePulse(Gamepad);
            }
        }


        protected override void PassiveCapacity()
        {
            ;
        }
    }
}
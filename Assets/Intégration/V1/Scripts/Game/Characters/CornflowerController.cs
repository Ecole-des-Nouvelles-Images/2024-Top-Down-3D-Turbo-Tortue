using Michael.Scripts;
using Michael.Scripts.Controller;
using UnityEngine;

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
            }
        }


        protected override void PassiveCapacity()
        {
            ;
        }
    }
}
using Intégration.V1.Scripts.Game;
using Michael.Scripts;
using UnityEngine;

namespace Noah.Scripts
{
    public class CornflowerController : FlowerController
    {
        [SerializeField] Shield _Shield;

        protected override void MainCapacity()
        {
            _Shield.OpenShield();
        }

        protected override void PassiveCapacity()
        {
            ;
        }
    }
}

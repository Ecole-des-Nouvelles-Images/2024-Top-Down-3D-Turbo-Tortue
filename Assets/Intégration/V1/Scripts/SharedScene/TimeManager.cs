using Michael.Scripts.Manager;
using UnityEngine;

namespace Intégration.V1.Scripts.SharedScene
{
    public class TimeManager : MonoBehaviourSingleton<TimeManager>
    {
        public float timeScale = 1;
        public float deltaTime;
        public float fixedDeltaTime;
        public float time;

        private void Update()
        {
            time += Time.deltaTime * timeScale;
            deltaTime = Time.deltaTime * timeScale;
        }

        private void FixedUpdate()
        {
            fixedDeltaTime = Time.fixedDeltaTime * timeScale;

            // Plug physics fixed delta time simulation on time manager
            if (Physics2D.simulationMode != SimulationMode2D.Script) return;
            Physics2D.Simulate(fixedDeltaTime);
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

namespace Intégration.V1.Scripts.Game.Characters
{
    public class CloverController : FlowerController
    {
        [SerializeField] private GameObject[] plantedFlowers;

        public bool HasDecoys => plantedFlowers != null && plantedFlowers.Length > 0;

        protected override void Start()
        {
            base.Start();
            plantedFlowers = GameObject.FindGameObjectsWithTag("Fake");
        }

        protected override void MainCapacity()
        {
            if (IsPlanted && Sun >= CapacityCost && plantedFlowers.Length != 0)
            {
                AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersVoices);
                Vector3 initialPosition = gameObject.transform.position;
                GameObject randomSpawnPoint = plantedFlowers[Random.Range(0, plantedFlowers.Length)];

                Rb.MovePosition(new Vector3(randomSpawnPoint.transform.position.x, 0,
                    randomSpawnPoint.transform.position.z));

                randomSpawnPoint.transform.position = new Vector3(initialPosition.x,
                    randomSpawnPoint.transform.position.y, initialPosition.z);

                OnLooseSunCapacity(CapacityCost);
                RumbleManager.Instance.RumblePulse(Gamepad);
            }
        }

        private void DisableKinematic()
        {
            Rb.isKinematic = false;
        }

        protected override void PassiveCapacity()
        {
            throw new System.NotImplementedException();
        }
    }
}

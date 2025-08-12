using System.Collections;
using UnityEngine;

namespace Noah.Scripts
{
    public class RoseController : FlowerController
    {
        [SerializeField] private GameObject spawnTrap;
        [SerializeField] private float respawnDelay = 5f;

        private GameObject _currentTrap;
        
        protected override void PassiveCapacity()
        {
            throw new System.NotImplementedException();
        }
        
        protected override void Update()
        {
            PassiveCapacity();
            base.Update();
            Respawn();
        }

        protected override void MainCapacity()
        {
            if (_currentTrap != null)
            {
                Instantiate(spawnTrap, transform.position, transform.rotation);
                _currentTrap = spawnTrap;
            }

            else
            {
                _currentTrap.transform.position = transform.position;
            }
        }
        
        

        private void Respawn()
        {
            if (isDead)
            {
                StartCoroutine(RespawnWaiter());
            }
        }

        IEnumerator RespawnWaiter()
        {
            yield return new WaitForSeconds(respawnDelay);
            transform.position = _currentTrap.transform.position;
            Invoke(nameof(GetRevive), 0.05f);
        }
    }
}

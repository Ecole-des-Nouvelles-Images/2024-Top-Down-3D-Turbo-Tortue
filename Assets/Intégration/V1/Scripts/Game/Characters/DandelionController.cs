using UnityEngine;
using UnityEngine.InputSystem;

namespace Intégration.V1.Scripts.Game.Characters
{
    public class DandelionController : FlowerController
    {
        [SerializeField] private Collider dandelionCollider;
        [SerializeField] private float unhittableDuration = 5f;
        [SerializeField] private ParticleSystem pollenVfx;
        private float _unhittableTimer;
        private bool _isBoosted;
        private bool _isColliding;

        protected override void Start()
        {
            base.Start();
            _unhittableTimer = 0;
        }

        protected override void Update()
        {
            base.Update();

            if (isUnhittable)
            {
                _unhittableTimer += Time.deltaTime;

                if (_unhittableTimer >= unhittableDuration)
                {
                    isUnhittable = false;
                    _unhittableTimer = 0;
                    ResetBoost();
                }
                else if (!_isBoosted && !isDead)
                {
                    ActivateBoost();
                }
            }
            else
            {
                if (_isBoosted)
                {
                    ResetBoost();
                }
            }
        }

        protected override void PassiveCapacity()
        {
            // Implement passive capacity logic here if needed
        }

        private void ActivateBoost()
        {
            // Changer le layer mask en CanMoveThroughWalls
            gameObject.layer = LayerMask.NameToLayer("Dandelion");

            pollenVfx.Play();
            //   aliveModel.SetActive(false);
            moveSpeed += 250;
            dandelionCollider.enabled = false;// desactive collider pour eviter de recuperer des soleils
            _isBoosted = true;
            _animator.SetBool("IsInvincible", true);
            Debug.Log("Boost Activated");
        }

        private void ResetBoost()
        {
            gameObject.layer = LayerMask.NameToLayer("Default");

            pollenVfx.Stop();
            //  aliveModel.SetActive(true);
            moveSpeed = 525;
            dandelionCollider.enabled = true; 
            _isBoosted = false;
            _animator.SetBool("IsInvincible", false);
            ExitObstacle();
        }

        private void ExitObstacle()
        {
            if (_isColliding)
            {
                /*
                Vector3 safePosition = FindSafePosition();
                transform.position = safePosition;
                */
                _isColliding = false;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            _isColliding = true;
        }

        protected override void MainCapacity()
        {
            if (Sun >= CapacityCost && !IsPlanted)
            {
                AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersVoices);
                isUnhittable = true;
                OnLooseSunCapacity(CapacityCost);
                RumbleManager.Instance.RumblePulse(_gamepad);
            }
        }
        /*
        private Vector3 FindSafePosition()
        {
            float radius = 1f;
            float maxDistance = 10f;

            Vector3[] directions = {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right,
            };

            foreach (var direction in directions)
            {
                RaycastHit hit;
                if (!Physics.Raycast(transform.position, direction, out hit, maxDistance))
                {
                    return transform.position + direction * radius;
                }
            }

            return transform.position + Vector3.up * radius;
        }
        */
    }
}
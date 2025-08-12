using System;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Noah.Scripts
{
    public abstract class FlowerController : CharacterController
    {
        public static Action OnSunCollected;
        public int sun = 0;
        public int maxSun = 3;
        public int CapacityCost = 2;
        public bool canReanimate;
        public FlowerController deadFlowerController;
        public bool IsPlanted = false;
        public bool isInvincible = false;
        [FormerlySerializedAs("IsStun")] public bool IsStunned;
        [SerializeField] bool isCharging;
        [SerializeField] float reanimateTimer = 0;
        [SerializeField] private float reanimateDuration = 1;
        [SerializeField] private GameObject deadModel;
        [SerializeField] private GameObject aliveModel;
        protected Collider AliveModelCollider;
        protected bool isDead;
        [SerializeField] private float magnetudeToStun = 22f;
        [SerializeField] private float stunDuration = 3f;
        [SerializeField] private float stunTimer = 0;
        [SerializeField] private ParticleSystem stunParticleSystem;
        [SerializeField] private float plantingCooldown = 0.7f;
        private float currentPlantingCooldown = 0f;

        protected virtual void Start()
        {
        }

        protected override void FixedUpdate()
        {
            if (!IsStunned)
            {
                Move();
            }
        }

        protected override void Update()
        {
            _animator.SetFloat("Velocity", Rb.velocity.magnitude);
            
            
            if (sun < 0)
            {
                sun = 0;
            }

            if (sun > maxSun)
            {
                sun = maxSun;
            }

            if (isCharging)
            {
                reanimateTimer += Time.deltaTime;
                if (reanimateTimer >= reanimateDuration + 0.1)
                {
                    ThirdCapacity();
                    isCharging = false;
                    reanimateTimer = 0;
                }
            }

            if (IsStunned)
            {
                stunTimer += Time.deltaTime;
                if (stunTimer >= stunDuration)
                {
                    stunParticleSystem.gameObject.SetActive(false);
                    IsStunned = false;
                    stunTimer = 0;
                    _animator.SetBool("IsDizzy", false);
                }
            }

            if (currentPlantingCooldown > 0)
            {
                currentPlantingCooldown -= Time.deltaTime;
            }
        }

        public override void OnSecondaryCapacity(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!IsStunned && currentPlantingCooldown <= 0)
                {
                    if (!IsPlanted)
                    {
                        SecondaryCapacity();
                    }
                    else
                    {
                        GetUnplanted();
                    }
                }
            }
        }

        protected override void SecondaryCapacity()
        {
            GetPlanted();
            currentPlantingCooldown = plantingCooldown;
        }

        private void GetPlanted()
        {
            IsPlanted = true;
            Rb.isKinematic = true;
            _animator.SetBool("isPlanted", IsPlanted);
            AliveModelCollider.enabled = false;
        }

        private void GetUnplanted()
        {
            if (IsPlanted)
            {
                IsPlanted = false;
                Rb.isKinematic = false;
                _animator.SetBool("isPlanted", IsPlanted);
                AliveModelCollider.enabled = true;
                currentPlantingCooldown = plantingCooldown;
            }
        }

        public override void OnThirdCapacity(InputAction.CallbackContext context)
        {
            if (canReanimate && sun == maxSun && !IsStunned)
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

        protected override void ThirdCapacity()
        {
            Debug.Log("revive");
            sun -= maxSun;
            deadFlowerController.GetRevive();
            canReanimate = false;
        }

        protected abstract void PassiveCapacity();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Turtle Collider"))
            {
                TakeHit();
            }
            

            if (other.CompareTag("TurtleTrap"))
            {
                GameManager.Instance.TurtleTrap.Remove(other.gameObject);
                Destroy(other.gameObject);
                GetStunned();
            }

            if (other.CompareTag("Seed"))
            {
                canReanimate = true;
                deadFlowerController = other.GetComponentInParent<FlowerController>();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("Turtle") && !isDead)
            {
                Rigidbody turtleRb = other.gameObject.GetComponent<Rigidbody>();
                if (turtleRb.velocity.magnitude > magnetudeToStun)
                {
                    GetStunned();
                }
            }
            
            if (other.CompareTag("Shield"))
            {
                isInvincible = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Seed"))
            {
                canReanimate = false;
                isCharging = false;
                reanimateTimer = 0;
                deadFlowerController = null;
            }
            
            if (other.CompareTag("Shield"))
            {
                isInvincible = false;
            }
        }

        [ContextMenu("GetStunned")]
        private void GetStunned()
        {
            if (!isInvincible)
            {
                GetUnplanted();
                stunParticleSystem.gameObject.SetActive(true);
                _animator.SetBool("IsDizzy", true);
                IsStunned = true; 
            }
        }

        [ContextMenu("TakeHit")]
        private void TakeHit()
        {
            if (!isInvincible)
            {
                AliveModelCollider.enabled = false;
                aliveModel.SetActive(false);
                deadModel.SetActive(true);
                GetComponent<PlayerInput>().enabled = false;
                isDead = true;
                sun = 0;
                GameManager.Instance.FlowersAlive.Remove(this.gameObject);
            }
        }

        [ContextMenu("GetRevive")]
        public void GetRevive()
        {
            AliveModelCollider.enabled = true;
            GetComponent<PlayerInput>().enabled = true;
            isDead = false;
            aliveModel.SetActive(true);
            deadModel.SetActive(false);
            GameManager.Instance.FlowersAlive.Add(this.gameObject);
        }

        public void OnLooseSunCapacity(int capacityCost)
        {
            sun -= capacityCost;
        }
    }
}
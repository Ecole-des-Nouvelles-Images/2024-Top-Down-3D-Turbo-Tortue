using System;
using DG.Tweening;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Manager;
using Michael.Scripts.Ui;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Intégration.V1.Scripts.Game.Characters
{
    public abstract class FlowerController : CharacterController
    {
        public event Action<int> OnSunChanged;
        public event Action<bool> OnDeathChanged;
        
        public bool CanRespawn;
        public int characterIndex;
        private int _sun;
        public int Sun
        {
            get => _sun;
            set
            {
                int clamped = Mathf.Clamp(value, 0, maxSun);
                if (_sun != clamped)
                {
                    _sun = clamped;
                    OnSunChanged?.Invoke(_sun);
                }
            }
        }

        public int maxSun = 3;
        public int CapacityCost = 2;
        public bool canReanimate;
        public FlowerController deadFlowerController;
        public bool IsPlanted = false;
        public bool isInvincible = false;
        public bool isUnhittable = false;
        public bool isUnstoppable = false;
        public bool IsStunned;
        public bool isDead;
        public bool isCharging;
        public float reanimateTimer = 0;
        public VisualEffect GatherEnergy;
        [SerializeField] private float reanimateDuration = 1;
        [SerializeField] protected GameObject deadModel;
        [SerializeField] protected GameObject aliveModel;
        [SerializeField] protected Collider aliveModelCollider;
        [SerializeField] private float magnetudeToStun = 22f;
        [SerializeField] private float stunDuration = 3f;
        [SerializeField] private float stunTimer = 0;
        [SerializeField] private ParticleSystem explosionParticleSystem;
        [SerializeField] private ParticleSystem stunParticleSystem;
        [SerializeField] private float plantingCooldown = 0.7f;
        [SerializeField] private Image reviveChargingIcon;
        [SerializeField] private GameObject deadArrowUI;
        [SerializeField] private VisualEffect ReviveVFX;
        [SerializeField] private GameObject dirt;
        private float currentPlantingCooldown = 0f;


        private void OnEnable()
        {
            GameManager.Instance.OnFlowersWin += WinAnimation;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnFlowersWin -= WinAnimation;
        }

        protected virtual void Start()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            playerInput.onDeviceLost += input =>
            {
                Debug.Log($"OnDeviceLost: {input}");
            };
            playerInput.onDeviceRegained += input =>
            {
                Debug.Log($"OnDeviceRegained: {input}");
            };
            StartAnimation();
        }

        private void StartAnimation()
        {
            if (deadArrowUI)
            {
                deadArrowUI.transform.DOLocalMoveY(0.05f, 1)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
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

          
            if (deadFlowerController)
            {
                if (isCharging)
                {
                    deadFlowerController.reviveChargingIcon.fillAmount = 0;
                    reanimateTimer += Time.deltaTime;
                    deadFlowerController.reviveChargingIcon.fillAmount = reanimateTimer / reanimateDuration;

                    if (reanimateTimer >= reanimateDuration + 0.1f)
                    {
                        ThirdCapacity();
                        isCharging = false;
                        reanimateTimer = 0;
                    }
                }
                else
                {
                    DOTween.To(() => deadFlowerController.reviveChargingIcon.fillAmount,
                        value => deadFlowerController.reviveChargingIcon.fillAmount = value, 0f, 0.3f);
                }
            }


            if (IsStunned)
            {
                stunTimer += Time.deltaTime;
                if (stunTimer >= stunDuration)
                {
                    stunParticleSystem.gameObject.SetActive(false);
                    stunParticleSystem.Clear();
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

        public override void OnMainCapacity(InputAction.CallbackContext context)
        {
            if (context.performed && !IsStunned && !PauseControlller.IsPaused)
            {
                MainCapacity();
            }
        }


        public override void OnSecondaryCapacity(InputAction.CallbackContext context)
        {
            if (context.started && !IsStunned && !PauseControlller.IsPaused)
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
            // SE PLANTER DANS LE SOL 

            GetPlanted();
            currentPlantingCooldown = plantingCooldown;
        }

        private void GetUnplanted()
        {
            if (IsPlanted)
            {
                Instantiate(dirt, transform.localPosition, quaternion.identity);
                IsPlanted = false;
                Rb.isKinematic = false;
                _animator.SetBool("isPlanted", IsPlanted);
                aliveModelCollider.enabled = true;
                currentPlantingCooldown = plantingCooldown;
                
                AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersDig);
            }
        }

        public override void OnThirdCapacity(InputAction.CallbackContext context)
        {
            // REANIMATION

            if (canReanimate && Sun == maxSun && !IsStunned && !PauseControlller.IsPaused)
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

        protected override void ThirdCapacity() // revive ally 
        {
            Debug.Log("revive"); 
            Sun = 0;
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
                if (!isDead)
                {
                    GameManager.Instance.TurtleTrap.Remove(other.gameObject);
                    Destroy(other.gameObject);
                    GetStunned();
                   AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleTrapActivated);
                }
            }

            if (other.CompareTag("Seed"))
            {
                canReanimate = true;
                deadFlowerController = other.GetComponentInParent<FlowerController>();
                deadFlowerController.reviveChargingIcon.gameObject.SetActive(true);
                deadFlowerController.deadArrowUI.SetActive(false);
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
                if (deadFlowerController)
                {
                    deadFlowerController.reviveChargingIcon.gameObject.SetActive(false);
                    deadFlowerController.deadArrowUI.SetActive(true);
                    deadFlowerController = null;
                }

                if (other.CompareTag("Shield"))
                {
                    isInvincible = false;
                }
            }
        }


        private void GetPlanted()
        {
            IsPlanted = true;
            Rb.isKinematic = true;
            _animator.SetBool("isPlanted", IsPlanted);
            aliveModelCollider.enabled = false;
            AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersDig);
        }

        [ContextMenu("GetStunned")]
        private void GetStunned()
        {
            explosionParticleSystem.Play();

            if (!isInvincible && !isUnstoppable && !isUnhittable)
            {
                stunParticleSystem.gameObject.SetActive(true);
                stunParticleSystem.Play();
                GetUnplanted();
                _animator.SetBool("IsDizzy", true);
                IsStunned = true;
            }
        }


        [ContextMenu("TakeHit")]
        private void TakeHit()
        {
            if (!isInvincible && !isUnhittable)
            {
                aliveModelCollider.enabled = false;
                aliveModel.SetActive(false);
                deadModel.SetActive(true);
                //GetComponent<PlayerInput>().enabled = false;
                GetComponent<PlayerInput>().currentActionMap = null;
                Sun = 0;
                isDead = true;
                OnDeathChanged?.Invoke(isDead);
                GameManager.Instance.FlowersAlive.Remove(this.gameObject);
               
                if (!CanRespawn) {
                    GameManager.Instance.WinVerification();
                }

                if (!GameManager.Instance.GameFinished)
                {
                    deadArrowUI.SetActive(true);
                }
                
               
            }
        }

        [ContextMenu("GetRevive")]
        public void GetRevive()
        {
            if (!isDead) return;

            aliveModelCollider.enabled = true;
            //GetComponent<PlayerInput>().enabled = true;
            GetComponent<PlayerInput>().SwitchCurrentActionMap("Character");
            isDead = false;
            OnDeathChanged?.Invoke(isDead);
            aliveModel.SetActive(true);
            deadModel.SetActive(false);
            GameManager.Instance.FlowersAlive.Add(this.gameObject);
            ReviveVFX.Play();
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.FlowersRevive);
        }


        protected void OnLooseSunCapacity(int capacityCost)
        {
            Sun -= capacityCost;
        }

        public void AddSun(int amount)
        {
            Sun += amount;
        }

        private void WinAnimation()
        {
            _animator.SetTrigger("Victory");
            _animator.SetInteger("DanceIndex", Random.Range(0, 3));

        }
    }
}
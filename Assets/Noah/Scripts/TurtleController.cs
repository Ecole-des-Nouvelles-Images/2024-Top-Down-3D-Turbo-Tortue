using System;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Noah.Scripts
{
    public class TurtleController : CharacterController
    {
        [SerializeField] private float boosterMultiplier = 1.2f;
        
        [Header("General References")] [SerializeField]
        private Collider _attackCollider;

        // [SerializeField] private TrailRenderer dashTrail;
        [SerializeField] private Material dashMaterial;

        [Header("Charging & Dashing")] [SerializeField]
        private float firstDashLevelTime = 0.7f;

        [SerializeField] private float firstDashLevelPower = 5;
        [SerializeField] private float secondDashLevelTime = 1.5f, secondDashLevelPower = 10;
        [SerializeField] private float thirdDashLevelTime = 3f, thirdDashLevelPower = 20;
        [SerializeField] private Material materialToUpdate;
        [SerializeField] private List<Color> colorsDashLevel;
        [SerializeField] private GameObject chargingParticules;
        [SerializeField] private GameObject chargingSmokeParticules;
        private float _chargeTime;
        private bool _isCharging;
        private bool _isDashing;
        private Vector3 _lastDashDirection;
        private float _normalSpeed;
        private bool _isAttacking;
        
        [Header("Scanning")] [SerializeField] private float scanTime, scanRange, scanDuration;
        [SerializeField] private GameObject scanSphereArea;
        private bool _isScanning;

        [Header("Trap")] [SerializeField] private GameObject TrapPrefab;
        [SerializeField] private Transform TrapSpawn;

        public Transform TransformWall;
        [SerializeField] private Collider headCollider;
        private bool _isTouchingWall;

        private void Start()
        {
            QteManager.Instance.OnQteFinished += AnimationDash;
            _attackCollider.enabled = false;
            gameObject.SetActive(false);
            _normalSpeed = moveSpeed;
        }

        private void AnimationDash()
        {
            _animator.SetBool("QteSuccess", true);
        }

        protected override void FixedUpdate()
        {
            if (!_isDashing && !_isCharging)
            {
                Move();
            }
        }

        protected override void Update()
        {
            DashingUpdate();
            ScanningUpdate();
            _animator.SetFloat("Velocity", Rb.velocity.magnitude);

            if (GameManager.Instance.TurtleIsDead)
            {
                _animator.SetBool("IsDead", true);
                GetComponent<PlayerInput>().enabled = false;
                materialToUpdate.SetColor("_EmissionColor", Color.black);
            }
            else if (!_isCharging)
            {
                materialToUpdate.SetColor("_EmissionColor", colorsDashLevel[0]);
                dashMaterial.SetColor("_EmissionColor", colorsDashLevel[0]);
            }
        }

        public void OnBooster(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("turbo");
                moveSpeed *= boosterMultiplier;
            }
            else
            {
                moveSpeed = _normalSpeed;
            }
        }
        
        protected override void Move()
        { 
            Vector3 movement = new Vector3(move.x, 0f, move.y) * moveSpeed;
            if (movement != Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(movement, Vector3.up);
                Rb.rotation = Quaternion.Slerp(Rb.rotation, newRotation, 0.15f);
            }
            
            // Rb.MovePosition(transform.position + new Vector3(movement.x, 2, movement.z) * Time.deltaTime);
            Rb.AddForce(movement * Time.deltaTime, ForceMode.Force);
            // Rb.velocity = new Vector3(movement.x, Rb.velocity.y, movement.z);
            
        }

        #region Main Capacity

        public override void OnMainCapacity(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                StartCharging();
            }
            else if (context.canceled)
            {
                StopCharging();
                MainCapacity();
            }
        }

        protected override void MainCapacity()
        {
            if (!_isDashing)
            {
                Vector3 dashDirection = new Vector3(move.x, 0f, move.y);
                if (dashDirection == Vector3.zero)
                {
                    dashDirection = transform.forward;
                }

                if (_lastDashDirection != Vector3.zero)
                {
                    dashDirection = _lastDashDirection;
                }

                float currentDashForce = 0;

                if (_chargeTime > firstDashLevelTime && _chargeTime < secondDashLevelTime)
                {
                    currentDashForce = firstDashLevelPower * firstDashLevelTime;
                }
                else if (_chargeTime > firstDashLevelTime && _chargeTime > secondDashLevelTime &&
                         _chargeTime < thirdDashLevelTime)
                {
                    currentDashForce = secondDashLevelPower * secondDashLevelTime;
                    //BatteryManager.Instance.BatteryCost(10);
                }
                else if (_chargeTime > firstDashLevelTime && _chargeTime > secondDashLevelTime &&
                         _chargeTime > thirdDashLevelTime)
                {
                    currentDashForce = thirdDashLevelPower * thirdDashLevelTime;
                   // BatteryManager.Instance.BatteryCost(20);
                }
                else
                {
                    Debug.Log("No Force");
                }

                _animator.SetBool("IsDashing", true);
                Rb.AddForce(currentDashForce * dashDirection, ForceMode.Impulse);
                if (dashDirection != Vector3.zero)
                {
                    Rb.rotation = Quaternion.LookRotation(dashDirection);
                }

                _isDashing = true;
            }
        }

        private void DashingUpdate()
        {
            if (_isDashing && Rb.velocity.magnitude < 0.01f)
            {
                _isDashing = false;
                _animator.SetBool("IsDashing", false);
                //dashTrail.enabled = false;
                chargingSmokeParticules.SetActive(false);
                _lastDashDirection = Vector3.zero;
                chargingParticules.SetActive(false);
            }

            if (_isCharging)
            {
                _animator.SetBool("IsDashing", true);
                _animator.SetFloat("DashTimer", _chargeTime);
                _chargeTime += Time.deltaTime;

                if (move.magnitude > 0.5f)
                {
                    _lastDashDirection = new Vector3(move.x, 0f, move.y);
                }

                // change light of turtle when charging 
                if (_chargeTime > firstDashLevelTime && _chargeTime < secondDashLevelTime)
                {
                    materialToUpdate.SetColor("_EmissionColor", colorsDashLevel[0]);
                    dashMaterial.SetColor("_EmissionColor", colorsDashLevel[0]);
                }
                else if (_chargeTime > firstDashLevelTime && _chargeTime > secondDashLevelTime &&
                         _chargeTime < thirdDashLevelTime)
                {
                    materialToUpdate.SetColor("_EmissionColor", colorsDashLevel[1]);
                    dashMaterial.SetColor("_EmissionColor", colorsDashLevel[1]);
                    //dashTrail.enabled = true;
                    chargingSmokeParticules.SetActive(true);
                    chargingParticules.SetActive(true);
                }
                else if (_chargeTime > firstDashLevelTime && _chargeTime > secondDashLevelTime &&
                         _chargeTime > thirdDashLevelTime)
                {
                    materialToUpdate.SetColor("_EmissionColor", colorsDashLevel[2]);
                    dashMaterial.SetColor("_EmissionColor", colorsDashLevel[2]);
                }
                else
                {
                    materialToUpdate.SetColor("_EmissionColor", colorsDashLevel[0]);
                    dashMaterial.SetColor("_EmissionColor", colorsDashLevel[0]);
                }
            }
        }

        private void StartCharging()
        {
            _isCharging = true;
            _chargeTime = 0f;
        }

        private void StopCharging()
        {
            _isCharging = false;
        }

        #endregion

        #region Secondary Capacity

        protected override void SecondaryCapacity()
        {
            if (!_isDashing)
            {
                /*
                RaycastHit hit;
                float raycastDistance = 6.0f;
                if (Physics.Raycast(transform.position, TransformWall.forward, out hit, raycastDistance))
                {
                    if (hit.collider.CompareTag("Wall"))
                    {
                        Debug.Log("Wall detected, attack cancelled.");
                        return;
                    }
                }
                */
                EnableAttackCollider();
                Invoke(nameof(DisableAttackCollider), 0.7f);
                _animator.SetTrigger("Attack");
              // BatteryManager.Instance.BatteryCost(10);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                _isTouchingWall = true;
                if (_isAttacking)
                {
                    _animator.SetTrigger("Idle");
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                _isTouchingWall = false;
            }
        }
        private void EnableAttackCollider()
        {
            _attackCollider.enabled = true;
            _isAttacking = true;

        }

        private void DisableAttackCollider()
        {
            _attackCollider.enabled = false;
            _isAttacking = false;
        }

        #endregion

        #region Third Capacity

        protected override void ThirdCapacity()
        {
            if (GameManager.Instance.TurtleTrap.Count <= 1)
            {
                GameObject trap = Instantiate(TrapPrefab, TrapSpawn.position, TrapSpawn.rotation);
                GameManager.Instance.TurtleTrap.Add(trap);
               // BatteryManager.Instance.BatteryCost(10);
            }
            else
            {
                Debug.Log("2 trap maximum");
            }
        }

        #endregion

        #region Fourth Capacity

        protected override void FourthCapacity()
        {
            //SCANNER LES FLEURS

            if (!_isScanning)
            {
                scanSphereArea.transform.DOScale(scanRange, 3f);
                _isScanning = true;
               // BatteryManager.Instance.BatteryCost(20);
            }
        }

        private void ScanningUpdate()
        {
            if (_isScanning)
            {
                scanTime += Time.deltaTime;
                if (scanTime >= scanDuration)
                {
                    _isScanning = false;
                    scanTime = 0;
                    scanSphereArea.transform.DOScale(0, 0);
                }
            }
        }

        #endregion
        
    }
}

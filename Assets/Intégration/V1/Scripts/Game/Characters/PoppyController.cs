using System;
using Intégration.V1.Scripts.UI;
using Michael.Scripts;
using Michael.Scripts.Manager;
using Michael.Scripts.Ui;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Intégration.V1.Scripts.Game.Characters
{
    public class PoppyController : FlowerController
    {
        public MeshTrail MeshTrail;

        [Header("Grappling - References")] [SerializeField]
        private Transform Gun;

        [SerializeField] private LineRenderer lr;
        [SerializeField] private LayerMask whatIsGrappleable;
        [SerializeField] private ParticleSystem impactGrappleParticules;

        [Header("Grappling - Stats")] [SerializeField]
        private float grappleSpeed;

        [SerializeField] private float maxGrappleDistance;
        [SerializeField] private float grapplingSpeed;

        [Header("Grappling - Cooldown")] [SerializeField]
        private float grapplingCd;

        private float _grapplingCdTimer;

        private float _grappleHoldTimer;
        private Vector3 grapplePoint;
        private bool _isgrappling;
        private bool _shouldApplyGrappleForce;
        private bool isBoosted;

        protected override void Start()
        {
            base.Start();
            lr.enabled = false;
            lr.positionCount = 2;
            impactGrappleParticules = Instantiate(impactGrappleParticules, Vector3.zero, Quaternion.identity);
        }


        protected override void PassiveCapacity()
        {
            if (!isBoosted && GameManager.Instance.FlowersAlive!.Count == 1)
            {
                MeshTrail.InvokePassive();
                moveSpeed += 150;
                isBoosted = true;
            }

            if (GameManager.Instance.FlowersAlive.Count > 1)
            {
                isBoosted = false;
                moveSpeed = 525;
                MeshTrail.StopPassive();
            }

            else if (isDead || GameManager.Instance.GameFinished)
            {
                MeshTrail.StopPassive();
            }
        }

        protected override void Update()
        {
            PassiveCapacity();
            base.Update();
            GrapplingUpdate();
        }

        private void GrapplingUpdate()
        {
            if (_grapplingCdTimer > 0)
                _grapplingCdTimer -= Time.deltaTime;

            if (_isgrappling)
            {
                HoldGrapple();
            }
        }

        protected override void FixedUpdate()
        {
            if (!_isgrappling && !IsStunned)
            {
                Move();
            }
            else if (_shouldApplyGrappleForce)
            {
                ApplyGrappleForce();
            }
        }

        public override void OnMainCapacity(InputAction.CallbackContext context)
        {
            if (context.started && !IsStunned && !PauseControlller.IsPaused)
            {
                MainCapacity();
            }

            if (context.canceled)
            {
                StopGrapple();
            }
        }

        protected override void MainCapacity()
        {
            if (Sun >= CapacityCost && !IsPlanted)
            {
                AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersVoices);
                StartGrapple();
                OnLooseSunCapacity(CapacityCost);
                RumbleManager.Instance.RumblePulse(_gamepad);
            }
        }

        private void StartGrapple()
        {
            if (_grapplingCdTimer > 0) return;
            _grappleHoldTimer = 0f;
            _isgrappling = true;
            lr.enabled = true;
        }

        private void HoldGrapple()
        {
            if (!_isgrappling) return;

            _grappleHoldTimer += Time.deltaTime;

            if (!_shouldApplyGrappleForce)
            {
                float currentGrappleDistance = Mathf.Clamp(grappleSpeed * _grappleHoldTimer, 0f, maxGrappleDistance);

                lr.SetPosition(0, Gun.position);
                lr.SetPosition(1, Gun.position + Gun.forward * currentGrappleDistance);

                RaycastHit hit;
                if (Physics.Raycast(Gun.position, Gun.forward, out hit, currentGrappleDistance, whatIsGrappleable))
                {
                    grapplePoint = hit.point;
                    impactGrappleParticules.transform.position = grapplePoint;
                    impactGrappleParticules.transform.forward = hit.normal;
                    impactGrappleParticules.Play();
                    _shouldApplyGrappleForce = true;

                    lr.SetPosition(0, Gun.position);
                    lr.SetPosition(1, grapplePoint);

                    _grapplingCdTimer = grapplingCd;
                }
                else if (Math.Abs(currentGrappleDistance - maxGrappleDistance) < 0.01f)
                {
                    StopGrapple();
                }
            }
        }

        private void ApplyGrappleForce()
        {
            Vector3 directionToGrapple = (grapplePoint - transform.position).normalized;
            Vector3 forceVector = directionToGrapple * grapplingSpeed * Time.deltaTime;

            Rb.AddForce(forceVector, ForceMode.Force);

            lr.SetPosition(0, Gun.position);
            lr.SetPosition(1, grapplePoint);

            if (Vector3.Distance(transform.position, grapplePoint) < 0.1f)
            {
                StopGrapple();
            }
        }

        private void StopGrapple()
        {
            _isgrappling = false;
            _shouldApplyGrappleForce = false;
            _grapplingCdTimer = grapplingCd;
            lr.enabled = false;
        }

        private void ReturnGrapple()
        {
            lr.SetPosition(0, Gun.position);
            lr.SetPosition(1, grapplePoint);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject != null)
            {
                StopGrapple();
            }
        }

        private void OnDrawGizmos()
        {
            if (grapplePoint != Vector3.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(grapplePoint, 0.1f);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Gun.position, maxGrappleDistance);
        }
    }
}
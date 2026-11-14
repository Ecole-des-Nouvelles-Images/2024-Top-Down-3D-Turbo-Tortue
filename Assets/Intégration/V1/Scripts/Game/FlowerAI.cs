using System.Collections.Generic;
using Intégration.V1.Scripts.Game.Characters;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Controller;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.AI;

namespace Intégration.V1.Scripts.Game
{
    [RequireComponent(typeof(FlowerController))]
    public class FlowerAI : MonoBehaviour
    {
        private enum Goal { Idle, Flee, Hide, CollectSun, Revive, Scatter }

        [Header("NavMesh")]
        [SerializeField] private float navMeshAgentRadius = 0.5f;
        [SerializeField] private float navMeshAgentHeight = 2.2f;
        [SerializeField] private float navMeshSmallObstaclePadding = 0.6f;

        [Header("Décision")]
        [SerializeField] private float decisionInterval = 0.2f;
        [SerializeField] private float panicDistance = 3.5f;
        [SerializeField] private float dangerDistance = 7f;
        [SerializeField] private float safeDistance = 13f;
        [SerializeField] private Vector2 reactionDelay = new Vector2(0.15f, 0.45f);

        [Header("Cachette")]
        [SerializeField] private Vector2 hideDuration = new Vector2(6f, 14f);
        [SerializeField] private float hideSearchRadius = 18f;
        [SerializeField] private float minHideDistanceFromTurtle = 10f;
        [SerializeField] private float spreadDistance = 9f;
        [SerializeField] private float spreadPenalty = 14f;
        [SerializeField] private float maxUsefulTurtleDistance = 18f;

        [Header("Ouverture")]
        [SerializeField] private Vector2 scatterDistance = new Vector2(11f, 18f);
        [SerializeField] private float scatterMaxDuration = 8f;
        [SerializeField] private float scatterAngleJitter = 20f;

        [Header("Fuite")]
        [SerializeField] private Vector2 fleeRing = new Vector2(6f, 16f);
        [SerializeField] private float fleeRepathInterval = 0.6f;

        [Header("Déplacement")]
        [SerializeField] private float turnSpeed = 420f;
        [SerializeField] private float waypointReach = 1f;
        [SerializeField] private float repathInterval = 0.3f;
        [SerializeField] private float trapAvoidRadius = 2.5f;
        [SerializeField] private float turtleAvoidRadius = 5f;
        [SerializeField] private float stuckSpeed = 0.4f;
        [SerializeField] private float stuckTime = 1f;

        [Header("Soleils")]
        [SerializeField] private float minSunDistanceFromTurtle = 8f;

        private static readonly List<FlowerAI> AllFlowerAIs = new();
        private static float _scatterBaseAngle = -1f;

        private FlowerController _flower;
        private Rigidbody _rb;
        private RuntimeNavMesh _navMesh;
        private NavMeshPath _navPath;

        private Goal _goal = Goal.Idle;
        private Vector3 _destination;
        private bool _hasDestination;
        private FlowerController _reviveTarget;
        private float _decisionTimer;
        private float _sunRefreshTimer;
        private List<Sun> _suns = new();

        private bool _scattering = true;
        private bool _scatterAssigned;
        private float _scatterUntil;
        private float _hiddenUntil;
        private bool _hideSpotChosen;
        private float _nextFleeRepath;
        private float _threatSince = -1f;
        private float _reactionTime;

        private Vector3[] _corners = new Vector3[0];
        private int _cornerIndex;
        private Vector3 _pathTarget;
        private float _nextRepath;
        private Vector3 _moveDir;
        private float _stuckTimer, _backOffTimer;
        private Vector3 _backOffDir;

        private Transform _turtle;
        private TurtleController _turtleController;
        private Rigidbody _turtleRb;
        private float _turtleDistance;
        private bool _turtleApproaching;
        private bool _revealedByScan;

        private void Awake()
        {
            _flower = GetComponent<FlowerController>();
            _rb = GetComponent<Rigidbody>();
            _navPath = new NavMeshPath();
        }

        private void OnEnable() => AllFlowerAIs.Add(this);

        private void OnDisable()
        {
            AllFlowerAIs.Remove(this);
            if (AllFlowerAIs.Count == 0) _scatterBaseAngle = -1f;
        }

        private Vector3 ClaimedSpot => _hasDestination ? _destination : transform.position;

        private void Start()
        {
            _navMesh = RuntimeNavMesh.Ensure(gameObject, navMeshAgentRadius, navMeshAgentHeight, navMeshSmallObstaclePadding);
            _moveDir = transform.forward;
            _reactionTime = Random.Range(reactionDelay.x, reactionDelay.y);
        }

        private void Update()
        {
            if (!CanAct())
            {
                _flower.AISetReviving(false);
                _flower.AISetMove(Vector2.zero);
                return;
            }

            _sunRefreshTimer -= Time.deltaTime;
            if (_sunRefreshTimer <= 0f)
            {
                _sunRefreshTimer = 0.5f;
                _suns = new List<Sun>(FindObjectsByType<Sun>(FindObjectsSortMode.None));
            }

            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer <= 0f)
            {
                _decisionTimer = decisionInterval;
                Perceive();
                Think();
            }

            if (_flower.IsPlanted)
            {
                _flower.AISetMove(Vector2.zero);
                return;
            }

            UseDefensiveCapacities();
            Steer();
        }

        private bool CanAct()
        {
            var gm = GameManager.Instance;
            return gm != null && !gm.GameFinished && !PauseControlller.IsPaused && !GameIntro.IsPlaying
                   && !_flower.isDead && !_flower.IsStunned;
        }

        #region Perception

        private void Perceive()
        {
            if (_turtle == null && GameManager.Instance.Turtle != null)
            {
                _turtle = GameManager.Instance.Turtle.transform;
                _turtleController = _turtle.GetComponent<TurtleController>();
                _turtleRb = _turtle.GetComponent<Rigidbody>();
            }

            if (_turtle == null || _turtleController == null || _turtleController.IsDead)
            {
                _turtleDistance = float.MaxValue;
                _turtleApproaching = false;
                _revealedByScan = false;
                return;
            }

            Vector3 toMe = Flat(transform.position - _turtle.position);
            _turtleDistance = toMe.magnitude;

            Vector3 turtleVelocity = _turtleRb != null ? Flat(_turtleRb.linearVelocity) : Vector3.zero;
            bool charging = _turtleController.CurrentState == TurtleState.Charging
                            && Vector3.Angle(_turtle.forward, toMe) < 35f;
            _turtleApproaching = charging
                                 || (turtleVelocity.magnitude > 1f && Vector3.Dot(turtleVelocity.normalized, toMe.normalized) > 0.5f);

            _revealedByScan = _turtleController.IsScanning
                              && Flat(transform.position - _turtleController.ScanCenter).magnitude <= _turtleController.CurrentScanRadius + 0.5f;
        }

        private bool IsThreatened()
        {
            bool threat = _turtleDistance < panicDistance || (_turtleDistance < dangerDistance && _turtleApproaching);
            if (!threat)
            {
                _threatSince = -1f;
                return false;
            }

            if (_threatSince < 0f) _threatSince = Time.time;
            return Time.time - _threatSince >= _reactionTime || _turtleDistance < panicDistance;
        }

        #endregion

        #region Décision

        private void Think()
        {
            if (_flower.IsPlanted)
            {
                ThinkWhilePlanted();
                return;
            }

            if (IsThreatened())
            {
                _flower.AISetReviving(false);
                if (_flower is CloverController threatenedClover && threatenedClover.HasDecoys
                    && _flower.Sun >= _flower.CapacityCost && _flower.AIPlant())
                {
                    _hiddenUntil = Time.time + 2f;
                    return;
                }
                SetGoal(Goal.Flee);
                UpdateFleeDestination();
                return;
            }

            if (_scattering && UpdateScatter()) return;

            if (TryPlaceRespawnPoint()) return;
            if (TryShareSuns()) return;

            FlowerController ally = FindAllyToRevive();
            if (ally != null)
            {
                _reviveTarget = ally;
                SetGoal(Goal.Revive);
                _destination = ally.transform.position;
                _hasDestination = true;

                bool inRange = Flat(ally.transform.position - transform.position).magnitude < 1.5f;
                _flower.AISetReviving(inRange && _flower.CanReviveAlly);
                return;
            }
            _flower.AISetReviving(false);

            Sun sun = FindSafeSun();
            if (_flower.Sun < _flower.maxSun && sun != null)
            {
                SetGoal(Goal.CollectSun);
                _destination = sun.transform.position;
                _hasDestination = true;
                return;
            }

            SetGoal(Goal.Hide);
            if (!_hideSpotChosen || TurtleNear(_destination, minHideDistanceFromTurtle * 0.7f))
            {
                _destination = PickHideSpot();
                _hasDestination = true;
                _hideSpotChosen = true;
            }

            if (Flat(_destination - transform.position).magnitude < 1.2f && _turtleDistance > dangerDistance)
            {
                if (_flower.AIPlant())
                {
                    _hiddenUntil = Time.time + Random.Range(hideDuration.x, hideDuration.y);
                    _hideSpotChosen = false;
                }
            }
        }

        private bool UpdateScatter()
        {
            if (!_scatterAssigned)
            {
                _scatterAssigned = true;
                _scatterUntil = Time.time + scatterMaxDuration;
                _destination = PickScatterPoint();
                _hasDestination = true;
            }

            bool arrived = Flat(_destination - transform.position).magnitude < 2f;
            if (arrived || Time.time > _scatterUntil)
            {
                _scattering = false;
                return false;
            }

            if (_navMesh != null && _navMesh.IsReady && _navMesh.SamplePosition(_destination, 4f, out Vector3 onMesh))
                _destination = onMesh;

            SetGoal(Goal.Scatter);
            return true;
        }

        private Vector3 PickScatterPoint()
        {
            if (_scatterBaseAngle < 0f) _scatterBaseAngle = Random.Range(0f, 360f);

            int index = Mathf.Max(0, AllFlowerAIs.IndexOf(this));
            int count = Mathf.Max(1, AllFlowerAIs.Count);
            Vector3 center = Vector3.zero;
            foreach (FlowerAI other in AllFlowerAIs) center += other.transform.position;
            center /= count;

            float angle = _scatterBaseAngle + 360f * index / count + Random.Range(-scatterAngleJitter, scatterAngleJitter);
            Vector3 outward = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 fromCenter = Flat(transform.position - center);
            if (count > 1 && fromCenter.sqrMagnitude > 1f)
                outward = Vector3.Slerp(outward, fromCenter.normalized, 0.3f).normalized;

            Vector3 best = transform.position + outward * scatterDistance.x;
            float bestScore = float.MinValue;
            foreach (float offset in new[] { 0f, 25f, -25f, 50f, -50f, 80f, -80f })
            {
                Vector3 dir = Quaternion.Euler(0f, offset, 0f) * outward;
                Vector3 candidate = transform.position + dir * Random.Range(scatterDistance.x, scatterDistance.y);
                if (_navMesh != null && _navMesh.IsReady && !_navMesh.SamplePosition(candidate, 5f, out candidate)) continue;

                float score = -Mathf.Abs(offset) * 0.05f;
                if (_turtle != null || GameManager.Instance.Turtle != null)
                {
                    Vector3 turtlePos = _turtle != null ? _turtle.position : GameManager.Instance.Turtle.transform.position;
                    float fromTurtle = Flat(candidate - turtlePos).magnitude;
                    score += Mathf.Min(fromTurtle, maxUsefulTurtleDistance) * 0.5f;
                    if (fromTurtle < minHideDistanceFromTurtle) score -= 15f;
                }
                score -= TrapPenalty(candidate) + AllyPenalty(candidate);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            return best;
        }

        private void ThinkWhilePlanted()
        {
            bool found = _revealedByScan && _turtleDistance < dangerDistance + 2f;
            bool closeCall = _turtleDistance < 3f && _turtleApproaching;

            if (_flower is CloverController cloverAI && cloverAI.HasDecoys && _flower.Sun >= _flower.CapacityCost)
            {
                bool threatened = found || closeCall || (_turtleDistance < dangerDistance && _turtleApproaching);
                if (threatened)
                {
                    _flower.AIUseMainCapacity();
                    _hiddenUntil = Time.time + Random.Range(hideDuration.x, hideDuration.y);
                    return;
                }
            }

            if (found || closeCall)
            {
                if (_flower is CloverController clover && clover.HasDecoys && _flower.Sun >= _flower.CapacityCost)
                {
                    _flower.AIUseMainCapacity();
                    return;
                }

                if (_flower.AIUnplant())
                {
                    SetGoal(Goal.Flee);
                    UpdateFleeDestination();
                }
                return;
            }

            if (Time.time < _hiddenUntil || _turtleDistance < safeDistance) return;

            bool hasReason = FindAllyToRevive() != null
                             || (_flower.Sun < _flower.maxSun && FindSafeSun() != null)
                             || (_flower is RoseController rose && !rose.HasRespawnPoint && _flower.Sun >= _flower.CapacityCost);

            if (hasReason || Time.time > _hiddenUntil + 10f)
                _flower.AIUnplant();
        }

        private void SetGoal(Goal goal)
        {
            if (goal != _goal)
            {
                _goal = goal;
                _nextRepath = 0f;
                if (goal != Goal.Hide) _hideSpotChosen = false;
            }
        }

        private bool TryPlaceRespawnPoint()
        {
            if (_flower is not RoseController rose) return false;
            if (_flower.Sun < _flower.CapacityCost || _turtleDistance < safeDistance) return false;

            bool needsPoint = !rose.HasRespawnPoint || TurtleNear(rose.RespawnPoint, 6f);
            if (!needsPoint) return false;

            _flower.AIUseMainCapacity();
            return true;
        }

        private bool TryShareSuns()
        {
            if (_flower is not WhiteLysController || _flower.Sun < 2) return false;

            foreach (GameObject go in GameManager.Instance.FlowersAlive)
            {
                if (go == null || go == gameObject) continue;
                var ally = go.GetComponent<FlowerController>();
                if (ally != null && !ally.isDead && ally.Sun <= 1)
                {
                    _flower.AIUseMainCapacity();
                    return true;
                }
            }
            return false;
        }

        private FlowerController FindAllyToRevive()
        {
            if (_flower.NeedsFullSunToRevive && _flower.Sun < _flower.maxSun) return null;

            FlowerController best = null;
            float bestDistance = float.MaxValue;
            foreach (GameObject go in GameManager.Instance.Players)
            {
                if (go == null || go == gameObject) continue;
                var ally = go.GetComponent<FlowerController>();
                if (ally == null || !ally.isDead) continue;
                if (TurtleNear(ally.transform.position, safeDistance * 0.7f)) continue;

                float distance = Flat(ally.transform.position - transform.position).magnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = ally;
                }
            }
            return best;
        }

        private Sun FindSafeSun()
        {
            Sun best = null;
            float bestDistance = float.MaxValue;
            foreach (Sun sun in _suns)
            {
                if (sun == null) continue;
                Vector3 position = sun.transform.position;
                if (TurtleNear(position, minSunDistanceFromTurtle) || ClaimedByOther(position)) continue;

                float distance = Flat(position - transform.position).magnitude;
                if (_turtle != null)
                {
                    Vector3 toSun = Flat(position - transform.position).normalized;
                    Vector3 toTurtle = Flat(_turtle.position - transform.position).normalized;
                    if (Vector3.Dot(toSun, toTurtle) > 0.6f && distance > _turtleDistance * 0.6f) continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = sun;
                }
            }
            return best;
        }

        private bool TurtleNear(Vector3 position, float radius)
        {
            return _turtle != null && Flat(_turtle.position - position).magnitude < radius;
        }

        private Vector3 PickHideSpot()
        {
            Vector3 best = transform.position;
            float bestScore = float.MinValue;
            for (int i = 0; i < 16; i++)
            {
                if (_navMesh == null || !_navMesh.RandomPoint(transform.position, hideSearchRadius, out Vector3 point)) continue;

                float score = Random.Range(0f, 3f) - Flat(point - transform.position).magnitude * 0.15f;
                if (_turtle != null)
                {
                    float fromTurtle = Flat(point - _turtle.position).magnitude;
                    score += Mathf.Min(fromTurtle, maxUsefulTurtleDistance);
                    if (fromTurtle < minHideDistanceFromTurtle) score -= 20f;
                }
                score -= TrapPenalty(point) + AllyPenalty(point);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = point;
                }
            }
            return best;
        }

        private void UpdateFleeDestination()
        {
            if (Time.time < _nextFleeRepath && _hasDestination && Flat(_destination - transform.position).magnitude > 1.5f) return;
            _nextFleeRepath = Time.time + fleeRepathInterval;

            Vector3 away = _turtle != null ? Flat(transform.position - _turtle.position).normalized : -transform.forward;
            if (away == Vector3.zero) away = -transform.forward;

            Vector3 best = transform.position + away * fleeRing.x;
            float bestScore = float.MinValue;
            for (int i = 0; i < 14; i++)
            {
                Vector2 r = Random.insideUnitCircle.normalized * Random.Range(fleeRing.x, fleeRing.y);
                Vector3 candidate = transform.position + new Vector3(r.x, 0f, r.y);
                if (_navMesh != null && _navMesh.IsReady && !_navMesh.SamplePosition(candidate, 3f, out candidate)) continue;

                Vector3 dir = Flat(candidate - transform.position).normalized;
                float score = Vector3.Dot(dir, away) * 8f;
                if (_turtle != null) score += Flat(candidate - _turtle.position).magnitude * 0.6f;
                if (_navMesh != null && _navMesh.IsReady && !_navMesh.HasClearLine(transform.position, candidate)) score -= 3f;
                score -= TrapPenalty(candidate) + AllyPenalty(candidate) * 0.3f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            _destination = best;
            _hasDestination = true;
        }

        private static float TrapPenalty(Vector3 point)
        {
            float penalty = 0f;
            foreach (GameObject trap in GameManager.Instance.TurtleTrap)
            {
                if (trap != null && Flat(trap.transform.position - point).magnitude < 3f) penalty += 15f;
            }
            return penalty;
        }

        private float AllyPenalty(Vector3 point)
        {
            float penalty = 0f;
            foreach (GameObject go in GameManager.Instance.FlowersAlive)
            {
                if (go == null || go == gameObject) continue;
                penalty += SpreadPenalty(go.transform.position, point);
            }
            foreach (FlowerAI other in AllFlowerAIs)
            {
                if (other == null || other == this) continue;
                penalty += SpreadPenalty(other.ClaimedSpot, point);
            }
            return penalty;
        }

        private float SpreadPenalty(Vector3 from, Vector3 point)
        {
            float distance = Flat(from - point).magnitude;
            return distance < spreadDistance ? spreadPenalty * (1f - distance / spreadDistance) : 0f;
        }

        private bool ClaimedByOther(Vector3 point)
        {
            foreach (FlowerAI other in AllFlowerAIs)
            {
                if (other == null || other == this || other._goal != Goal.CollectSun) continue;
                if (Flat(other._destination - point).magnitude < 1f) return true;
            }
            return false;
        }

        #endregion

        #region Capacités

        private void UseDefensiveCapacities()
        {
            if (_goal != Goal.Flee || _flower.Sun < _flower.CapacityCost) return;

            switch (_flower)
            {
                case DandelionController:
                    if (_turtleDistance < panicDistance && !_flower.isUnhittable) _flower.AIUseMainCapacity();
                    break;

                case CornflowerController cornflower:
                    if (_turtleDistance < panicDistance && !cornflower.ShieldIsOpen) _flower.AIUseMainCapacity();
                    break;

                case PoppyController poppy:
                    TryGrappleAway(poppy);
                    break;
            }
        }

        private void TryGrappleAway(PoppyController poppy)
        {
            if (_turtleDistance > dangerDistance || !poppy.GrappleReady || _turtle == null) return;

            Vector3 away = Flat(transform.position - _turtle.position).normalized;
            Vector3 aim = Flat(poppy.GrappleDirection).normalized;
            if (Vector3.Dot(aim, away) < 0.6f) return;
            if (!poppy.HasGrappleTarget(poppy.GrappleDirection, 3f, out _)) return;

            _flower.AIUseMainCapacity();
        }

        #endregion

        #region Déplacement

        private void Steer()
        {
            if (!_hasDestination || _goal == Goal.Idle)
            {
                _flower.AISetMove(Vector2.zero);
                return;
            }

            if (_goal == Goal.Revive && _reviveTarget != null)
                _destination = _reviveTarget.transform.position;

            Vector3 toDest = Flat(_destination - transform.position);
            float stopDistance = _goal == Goal.Revive ? 0.8f : 0.4f;
            if (toDest.magnitude < stopDistance)
            {
                _flower.AISetMove(Vector2.zero);
                return;
            }

            Vector3 desired;
            if (_backOffTimer > 0f)
            {
                _backOffTimer -= Time.deltaTime;
                desired = _backOffDir;
            }
            else
            {
                desired = Flat(NextWaypoint() - transform.position).normalized;
                if (desired == Vector3.zero) desired = toDest.normalized;
                desired = ApplyRepulsion(desired);
                DetectStuck();
            }

            if (_moveDir == Vector3.zero) _moveDir = transform.forward;
            _moveDir = Flat(Vector3.RotateTowards(_moveDir, desired, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f)).normalized;
            _flower.AISetMove(new Vector2(_moveDir.x, _moveDir.z));
        }

        private Vector3 NextWaypoint()
        {
            if (_navMesh == null || !_navMesh.IsReady) return _destination;

            if (Time.time >= _nextRepath || Flat(_pathTarget - _destination).magnitude > 1.5f)
            {
                _nextRepath = Time.time + repathInterval;
                _pathTarget = _destination;
                if (_navMesh.CalculatePath(transform.position, _destination, _navPath) && _navPath.corners.Length > 0)
                {
                    _corners = _navPath.corners;
                    _cornerIndex = Mathf.Min(1, _corners.Length - 1);
                }
                else
                {
                    _corners = new Vector3[0];
                }
            }

            if (_corners.Length == 0) return _destination;

            while (_cornerIndex < _corners.Length - 1 && Flat(_corners[_cornerIndex] - transform.position).magnitude < waypointReach)
                _cornerIndex++;

            while (_cornerIndex < _corners.Length - 1 && _navMesh.HasClearLine(transform.position, _corners[_cornerIndex + 1]))
                _cornerIndex++;

            return _corners[_cornerIndex];
        }

        private Vector3 ApplyRepulsion(Vector3 desired)
        {
            Vector3 push = Vector3.zero;

            foreach (GameObject trap in GameManager.Instance.TurtleTrap)
            {
                if (trap == null || _flower is RoseController) continue;
                Vector3 fromTrap = Flat(transform.position - trap.transform.position);
                float distance = fromTrap.magnitude;
                if (distance < trapAvoidRadius && distance > 0.01f)
                    push += fromTrap.normalized * (1f - distance / trapAvoidRadius) * 2f;
            }

            if (_turtle != null && _goal != Goal.Flee)
            {
                Vector3 fromTurtle = Flat(transform.position - _turtle.position);
                float distance = fromTurtle.magnitude;
                if (distance < turtleAvoidRadius && distance > 0.01f)
                    push += fromTurtle.normalized * (1f - distance / turtleAvoidRadius) * 1.5f;
            }

            Vector3 result = desired + push;
            return result.sqrMagnitude < 0.01f ? desired : result.normalized;
        }

        private void DetectStuck()
        {
            if (Flat(_rb.linearVelocity).magnitude < stuckSpeed)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= stuckTime)
                {
                    _stuckTimer = 0f;
                    _nextRepath = 0f;
                    _nextFleeRepath = 0f;
                    _backOffTimer = 0.5f;
                    float side = Random.value < 0.5f ? -1f : 1f;
                    _backOffDir = (Quaternion.Euler(0f, side * 110f, 0f) * _moveDir).normalized;
                }
            }
            else
            {
                _stuckTimer = 0f;
            }
        }

        #endregion

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        private void OnDrawGizmosSelected()
        {
            if (!_hasDestination) return;
            Gizmos.color = _goal switch
            {
                Goal.Flee => Color.red,
                Goal.Revive => Color.green,
                Goal.CollectSun => Color.yellow,
                Goal.Hide => Color.cyan,
                _ => Color.white
            };
            Gizmos.DrawWireSphere(_destination, 0.4f);
            for (int i = Mathf.Max(0, _cornerIndex - 1); i < _corners.Length - 1; i++)
                Gizmos.DrawLine(_corners[i] + Vector3.up * 0.3f, _corners[i + 1] + Vector3.up * 0.3f);
        }
    }
}

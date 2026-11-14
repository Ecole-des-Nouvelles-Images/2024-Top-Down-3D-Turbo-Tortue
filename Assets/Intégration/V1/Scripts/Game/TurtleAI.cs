using System.Collections.Generic;
using Intégration.V1.Scripts.Game.Characters;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Controller;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.AI;

namespace Intégration.V1.Scripts.Game
{
    [RequireComponent(typeof(TurtleController))]
    public class TurtleAI : MonoBehaviour
    {
        private enum Goal { Chase, Unearth, Search, CollectSun, BiteRoseTrap }

        private class FlowerMemory
        {
            public Vector3 LastSeen;
            public bool Hidden;
            public Vector3 Guess;
            public bool Revealed;
            public bool GuessSearched;
        }

        [Header("Corps")]
        [SerializeField] private float bodyRadius = 1.1f;
        [SerializeField] private float bodyHeight = 2.2f;
        [SerializeField] private float navMeshAgentRadius = 0.5f;

        [Header("Décision")]
        [SerializeField] private float decisionInterval = 0.2f;
        [Tooltip("Imprécision sur la position d'une fleur qui s'est plantée.")]
        [SerializeField] private float hiddenGuessError = 4f;

        [Header("Déplacement")]
        [SerializeField] private float turnSpeed = 300f;
        [SerializeField] private float waypointReach = 1.3f;
        [SerializeField] private float repathInterval = 0.35f;
        [SerializeField] private float avoidDistance = 2f;
        [SerializeField] private float stuckSpeed = 0.5f;
        [SerializeField] private float stuckTime = 1.2f;

        [Header("Morsure")]
        [SerializeField] private float biteRange = 2.4f;
        [SerializeField] private float biteAngle = 70f;
        [SerializeField] private float biteCooldown = 0.6f;

        [Header("Dash")]
        [SerializeField] private float dashMinDistance = 4.5f;
        [SerializeField] private float dashMaxDistance = 10f;
        [SerializeField] private float dashCooldown = 2.5f;
        [SerializeField, Range(0f, 1f)] private float dashChance = 0.8f;
        [SerializeField] private float unearthStandOff = 4.5f;
        [SerializeField, Range(0f, 1f)] private float unearthDashChance = 0.3f;

        [Header("Turbo")]
        [SerializeField] private float turboMinDistance = 8f;
        [SerializeField] private float turboMaxDistance = 30f;
        [Tooltip("Si la Tortue ne se rapproche pas au moins de cette vitesse (m/s), elle tente le turbo.")]
        [SerializeField] private float turboClosingSpeed = 2.5f;
        [SerializeField] private Vector2 turboDuration = new Vector2(1.2f, 2.2f);
        [SerializeField] private float turboCooldown = 4f;
        [SerializeField, Range(0f, 1f)] private float minBatteryForTurbo = 0.25f;

        [Header("Scan")]
        [SerializeField] private float searchScanInterval = 5f;
        [SerializeField] private float scanCooldown = 4f;
        [SerializeField, Range(0f, 1f)] private float minBatteryForScan = 0.12f;

        [Header("Pièges")]
        [SerializeField] private float firstTrapDelay = 12f;
        [SerializeField] private float trapCooldown = 10f;
        [SerializeField] private float minDistanceBetweenTraps = 7f;
        [SerializeField, Range(0f, 1f)] private float minBatteryForTrap = 0.3f;

        [Header("Pots destructibles (dash chargé à fond)")]
        [SerializeField] private float potMinDistance = 2.5f;
        [SerializeField] private float minRunUpGap = 2f;
        [SerializeField] private float noProgressTimeout = 5f;
        [SerializeField] private float maxChaseTime = 15f;
        [SerializeField] private float ignoreDuration = 8f;
        [SerializeField] private float roseTrapBiteRange = 6f;
        [SerializeField] private float potMaxDistance = 8f;
        [SerializeField] private float potCooldown = 8f;
        [Tooltip("Casse un pot qui bloque le passage si le chemin est X fois plus long que la ligne droite.")]
        [SerializeField] private float potDetourFactor = 1.3f;
        [SerializeField, Range(0f, 1f)] private float minBatteryForPot = 0.4f;

        [Header("Animation")]
        [Tooltip("État de l'Animator dans lequel la Tortue peut bouger (après l'anim où elle se relève).")]
        [SerializeField] private string locomotionStateName = "Moove Tree";
        [SerializeField] private float maxStandUpWait = 3f;

        [Header("Batterie")]
        [SerializeField, Range(0f, 1f)] private float lowBatteryRatio = 0.5f;
        [SerializeField] private float maxSunDetour = 35f;
        [SerializeField] private float closeSunRange = 2f;
        [SerializeField] private float passingSunRange = 6f;
        [SerializeField] private float passingSunMaxAngle = 45f;
        [SerializeField] private float passingSunMinTargetDistance = 5f;
        [SerializeField, Range(0f, 1f)] private float passingSunMaxBattery = 0.9f;

        private TurtleController _turtle;
        private Rigidbody _rb;
        private BatteryManager _battery;
        private RuntimeNavMesh _navMesh;
        private Animator _animator;
        private QteManager _qteManager;

        private bool _waitingStandUp;
        private float _standUpMinEnd, _standUpTimeout;

        private List<FractureObject> _pots = new();
        private readonly Dictionary<FractureObject, bool> _potIntact = new();
        private float _potListRefreshTimer;
        private float _nextPot;
        private FractureObject _smashTarget;
        private List<RoseTrap> _roseTraps = new();
        private RoseTrap _roseTrapTarget;
        private readonly Dictionary<UnityEngine.Object, float> _ignoredUntil = new();
        private UnityEngine.Object _progressKey;
        private float _progressBest, _progressSince, _keySince;
        private FractureObject _forcedSmashPot;
        private float _forcedSmashUntil;

        private Goal _goal = Goal.Search;
        private FlowerController _target;
        private FlowerController _unearthTarget;
        private bool _unearthWithDash;
        private Vector3 _destination;
        private readonly Dictionary<FlowerController, FlowerMemory> _memory = new();
        private readonly List<FlowerController> _flowers = new();
        private List<Sun> _suns = new();
        private readonly List<(Vector3 pos, float time)> _searchedSpots = new();
        private readonly HashSet<Sun> _sunsConsideredForTrap = new();
        private Vector3 _searchPoint;
        private bool _hasSearchPoint;
        private float _decisionTimer, _sunRefreshTimer;
        private float _startTime;

        private NavMeshPath _navPath;
        private Vector3[] _corners = new Vector3[0];
        private int _cornerIndex;
        private Vector3 _pathTarget;
        private float _nextRepath;
        private Vector3 _moveDir;
        private float _stuckTimer, _backOffTimer;
        private Vector3 _backOffDir;

        private bool _isCharging, _isBoosting;
        private float _chargeTarget, _boostTimer;
        private float _nextBite, _nextDash, _nextBoost, _nextScan, _nextSearchScan, _nextTrap;
        private float _lastTargetDistance = -1f, _closingSpeed;

        private void Awake()
        {
            _turtle = GetComponent<TurtleController>();
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            _qteManager = GetComponent<QteManager>();
            _navPath = new NavMeshPath();
        }

        private void Start()
        {
            _battery = FindFirstObjectByType<BatteryManager>();
            _navMesh = RuntimeNavMesh.Ensure(gameObject, navMeshAgentRadius, bodyHeight, bodyRadius - navMeshAgentRadius);
            _startTime = Time.time;
            _moveDir = transform.forward;
            if (_qteManager != null) _qteManager.OnQteFinished += OnQteFinished;
        }

        private void OnDestroy()
        {
            if (_qteManager != null) _qteManager.OnQteFinished -= OnQteFinished;
        }

        private void OnQteFinished()
        {
            _waitingStandUp = true;
            _standUpMinEnd = Time.time + 0.3f;
            _standUpTimeout = Time.time + maxStandUpWait;
        }

        private bool HasStoodUp()
        {
            if (Time.time < _standUpMinEnd) return false;
            if (Time.time >= _standUpTimeout || _animator == null) return true;
            return _animator.GetCurrentAnimatorStateInfo(0).IsName(locomotionStateName) && !_animator.IsInTransition(0);
        }

        private void Update()
        {
            if (!CanAct())
            {
                ResetActions();
                return;
            }

            if (_waitingStandUp)
            {
                if (!HasStoodUp())
                {
                    _turtle.AISetMove(Vector2.zero);
                    return;
                }
                _waitingStandUp = false;
            }

            _sunRefreshTimer -= Time.deltaTime;
            if (_sunRefreshTimer <= 0f)
            {
                _sunRefreshTimer = 0.5f;
                _suns = new List<Sun>(FindObjectsByType<Sun>(FindObjectsSortMode.None));
                _roseTraps = new List<RoseTrap>(FindObjectsByType<RoseTrap>(FindObjectsSortMode.None));
                RefreshPots();
            }

            if (_turtle.IsScanning) RevealWithScan();

            if (_isCharging)
            {
                UpdateCharging();
                return;
            }

            if (_turtle.CurrentState == TurtleState.Dashing)
            {
                _turtle.AISetMove(Vector2.zero);
                return;
            }

            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer <= 0f)
            {
                _decisionTimer = decisionInterval;
                Perceive();
                Think();
            }

            Steer();
            UpdateTurbo();
            UseCapacities();
        }

        private bool CanAct()
        {
            var gm = GameManager.Instance;
            return gm != null && gm.GameisStarted && !gm.GameFinished && !gm.TurtleIsDead
                   && !PauseControlller.IsPaused && !_turtle.IsDead && !_rb.isKinematic;
        }

        private void ResetActions()
        {
            _isCharging = false;
            if (_isBoosting) { _turtle.AIStopBoost(); _isBoosting = false; }
            if (!_turtle.IsDead) _turtle.AISetMove(Vector2.zero);
        }

        #region Perception

        private void Perceive()
        {
            _flowers.Clear();
            foreach (GameObject go in GameManager.Instance.Players)
            {
                if (go == null) continue;
                var flower = go.GetComponent<FlowerController>();
                if (flower == null) continue;
                _flowers.Add(flower);

                if (!_memory.TryGetValue(flower, out var mem))
                {
                    mem = new FlowerMemory { LastSeen = flower.transform.position };
                    _memory[flower] = mem;
                }

                if (flower.isDead || !flower.IsPlanted)
                {
                    mem.LastSeen = flower.transform.position;
                    mem.Hidden = false;
                    mem.Revealed = false;
                    mem.GuessSearched = false;
                }
                else if (!mem.Hidden)
                {
                    mem.Hidden = true;
                    mem.Revealed = false;
                    mem.GuessSearched = false;
                    Vector2 err = Random.insideUnitCircle * hiddenGuessError;
                    mem.Guess = mem.LastSeen + new Vector3(err.x, 0f, err.y);
                }
            }

            if (_goal == Goal.Chase && _target != null)
            {
                float d = Flat(_target.transform.position - transform.position).magnitude;
                if (_lastTargetDistance >= 0f)
                    _closingSpeed = Mathf.Lerp(_closingSpeed, (_lastTargetDistance - d) / decisionInterval, 0.3f);
                _lastTargetDistance = d;
            }
            else
            {
                _lastTargetDistance = -1f;
                _closingSpeed = 0f;
            }
        }

        private void RevealWithScan()
        {
            float radius = _turtle.CurrentScanRadius;
            Vector3 center = _turtle.ScanCenter;
            foreach (var kvp in _memory)
            {
                if (kvp.Key == null || !kvp.Value.Hidden) continue;
                if (Flat(kvp.Key.transform.position - center).magnitude <= radius)
                    kvp.Value.Revealed = true;
            }
        }

        #endregion

        #region Décision

        private bool IsIgnored(UnityEngine.Object target)
        {
            return target != null && _ignoredUntil.TryGetValue(target, out float until) && Time.time < until;
        }

        private void UpdateProgressWatchdog()
        {
            UnityEngine.Object key = null;
            Vector3 point = _destination;
            switch (_goal)
            {
                case Goal.Chase:
                case Goal.Unearth:
                    key = _target;
                    if (_target != null) point = _target.transform.position;
                    break;
                case Goal.BiteRoseTrap:
                    key = _roseTrapTarget;
                    if (_roseTrapTarget != null) point = _roseTrapTarget.transform.position;
                    break;
                case Goal.CollectSun:
                    key = FindNearestSun(_destination, out _);
                    break;
            }

            if (key == null)
            {
                _progressKey = null;
                return;
            }

            float distance = Flat(point - transform.position).magnitude;
            if (key != _progressKey)
            {
                _progressKey = key;
                _progressBest = distance;
                _progressSince = Time.time;
                _keySince = Time.time;
                return;
            }

            if (distance < _progressBest - 1f)
            {
                _progressBest = distance;
                _progressSince = Time.time;
            }

            bool noProgress = Time.time - _progressSince > noProgressTimeout;
            bool chasingTooLong = _goal == Goal.Chase && Time.time - _keySince > maxChaseTime;
            if (!noProgress && !chasingTooLong) return;

            _ignoredUntil[key] = Time.time + ignoreDuration;
            _progressKey = null;
            _nextRepath = 0f;
        }

        private void Think()
        {
            UpdateProgressWatchdog();

            Sun nearestSun = FindNearestSun(transform.position, out float sunDistance);

            if (BatteryRatio() < lowBatteryRatio && nearestSun != null && sunDistance < maxSunDetour)
            {
                SetGoal(Goal.CollectSun, null, nearestSun.transform.position);
                return;
            }

            if (nearestSun != null && sunDistance <= closeSunRange && !TargetInBiteRange())
            {
                SetGoal(Goal.CollectSun, null, nearestSun.transform.position);
                return;
            }

            RoseTrap roseTrap = FindNearestRoseTrap(out float roseTrapDistance);
            if (roseTrap != null && roseTrapDistance <= roseTrapBiteRange && !TargetInBiteRange())
            {
                _roseTrapTarget = roseTrap;
                SetGoal(Goal.BiteRoseTrap, null, roseTrap.transform.position);
                return;
            }

            FlowerController visible = FindBestVisibleFlower(out float visibleScore);
            FlowerController revealed = FindNearestRevealed(out float revealedDistance);

            if (revealed != null && (visible == null || revealedDistance < visibleScore * 0.7f))
            {
                if (revealed != _unearthTarget)
                {
                    _unearthTarget = revealed;
                    _unearthWithDash = Random.value < unearthDashChance;
                }

                Vector3 flowerPos = revealed.transform.position;
                Vector3 away = Flat(transform.position - flowerPos);
                if (away.sqrMagnitude < 0.01f) away = -transform.forward;
                float approach = _unearthWithDash ? unearthStandOff : biteRange * 0.6f;
                SetGoal(Goal.Unearth, revealed, flowerPos + away.normalized * approach);
                return;
            }

            if (visible != null)
            {
                Vector3 chasePoint = PredictPosition(visible);
                if (TryGrabPassingSun(nearestSun, sunDistance, chasePoint)) return;
                SetGoal(Goal.Chase, visible, chasePoint);
                return;
            }

            if (!_hasSearchPoint || ReachedSearchPoint())
                PickSearchPoint();
            if (TryGrabPassingSun(nearestSun, sunDistance, _searchPoint)) return;
            SetGoal(Goal.Search, null, _searchPoint);
        }

        private bool TargetInBiteRange()
        {
            return _target != null && !_target.isDead
                   && Flat(_target.transform.position - transform.position).magnitude <= biteRange + 0.5f;
        }

        private bool TryGrabPassingSun(Sun sun, float sunDistance, Vector3 heading)
        {
            if (sun == null || sunDistance > passingSunRange || BatteryRatio() > passingSunMaxBattery) return false;

            Vector3 toHeading = Flat(heading - transform.position);
            if (toHeading.magnitude < passingSunMinTargetDistance) return false;

            Vector3 toSun = Flat(sun.transform.position - transform.position);
            if (sunDistance > 2.5f && Vector3.Angle(toSun, toHeading) > passingSunMaxAngle) return false;

            SetGoal(Goal.CollectSun, null, sun.transform.position);
            return true;
        }

        private void SetGoal(Goal goal, FlowerController target, Vector3 destination)
        {
            if (goal != _goal && goal == Goal.Search)
                _nextSearchScan = Time.time + 1.5f;
            _goal = goal;
            _target = target;
            _destination = destination;
        }

        private FlowerController FindBestVisibleFlower(out float bestScore)
        {
            FlowerController best = null;
            bestScore = float.MaxValue;
            foreach (var flower in _flowers)
            {
                if (flower.isDead || flower.IsPlanted) continue;
                if (IsIgnored(flower) && Flat(flower.transform.position - transform.position).magnitude > biteRange) continue;

                float score = PathDistance(flower.transform.position);
                if (flower.IsStunned) score *= 0.5f;
                if (flower.isInvincible || flower.isUnhittable) score *= 2.5f;
                if (flower == _target) score *= 0.75f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = flower;
                }
            }
            return best;
        }

        private FlowerController FindNearestRevealed(out float distance)
        {
            FlowerController best = null;
            distance = float.MaxValue;
            foreach (var kvp in _memory)
            {
                if (kvp.Key == null || !kvp.Value.Hidden || !kvp.Value.Revealed || kvp.Key.isDead) continue;
                float d = Flat(kvp.Key.transform.position - transform.position).magnitude;
                if (d < distance)
                {
                    distance = d;
                    best = kvp.Key;
                }
            }
            return best;
        }

        private Vector3 PredictPosition(FlowerController flower)
        {
            Vector3 pos = flower.transform.position;
            var rb = flower.GetComponent<Rigidbody>();
            if (rb == null || rb.isKinematic) return pos;
            float distance = Flat(pos - transform.position).magnitude;
            float lead = Mathf.Clamp(distance / 10f, 0f, 0.8f);
            return pos + Flat(rb.linearVelocity) * lead;
        }

        private void PickSearchPoint()
        {
            _hasSearchPoint = true;

            FlowerMemory bestMem = null;
            float bestDist = float.MaxValue;
            foreach (var mem in _memory.Values)
            {
                if (!mem.Hidden || mem.Revealed || mem.GuessSearched) continue;
                float d = Flat(mem.Guess - transform.position).magnitude;
                if (d < bestDist) { bestDist = d; bestMem = mem; }
            }
            if (bestMem != null)
            {
                bestMem.GuessSearched = true;
                _searchPoint = bestMem.Guess;
                return;
            }

            Sun bestSun = null;
            bestDist = float.MaxValue;
            foreach (Sun sun in _suns)
            {
                if (sun == null || WasSearchedRecently(sun.transform.position)) continue;
                float d = Flat(sun.transform.position - transform.position).magnitude;
                if (d < bestDist) { bestDist = d; bestSun = sun; }
            }
            if (bestSun != null)
            {
                _searchPoint = bestSun.transform.position;
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                if (_navMesh != null && _navMesh.RandomPoint(transform.position, 25f, out Vector3 p)
                    && Flat(p - transform.position).magnitude > 8f && !WasSearchedRecently(p))
                {
                    _searchPoint = p;
                    return;
                }
            }
            Vector2 r = Random.insideUnitCircle.normalized * 12f;
            _searchPoint = transform.position + new Vector3(r.x, 0f, r.y);
        }

        private bool ReachedSearchPoint() => Flat(_searchPoint - transform.position).magnitude < 2f;

        private bool WasSearchedRecently(Vector3 point)
        {
            foreach (var spot in _searchedSpots)
            {
                if (Time.time - spot.time < 25f && Flat(spot.pos - point).magnitude < 8f) return true;
            }
            return false;
        }

        private Sun FindNearestSun(Vector3 from, out float distance)
        {
            Sun best = null;
            distance = float.MaxValue;
            foreach (Sun sun in _suns)
            {
                if (sun == null || IsIgnored(sun)) continue;
                float d = Flat(sun.transform.position - from).magnitude;
                if (d < distance) { distance = d; best = sun; }
            }
            return best;
        }

        private float BatteryRatio()
        {
            if (_battery == null || _battery.MaxBatteryTime <= 0f) return 1f;
            return _battery.CurrentBatteryTime / _battery.MaxBatteryTime;
        }

        #endregion

        #region Déplacement

        private void Steer()
        {
            if (_goal == Goal.Chase && _target != null)
                _destination = PredictPosition(_target);

            Vector3 toDest = Flat(_destination - transform.position);
            float stopDistance = _goal == Goal.Unearth ? 0.8f : _goal == Goal.BiteRoseTrap ? 1.2f : 0.5f;
            if (toDest.magnitude < stopDistance)
            {
                Vector3 facePoint = _goal == Goal.BiteRoseTrap && _roseTrapTarget != null ? _roseTrapTarget.transform.position
                    : _target != null ? _target.transform.position
                    : _destination;
                Vector3 face = Flat(facePoint - transform.position);
                if (face.sqrMagnitude > 0.01f && Vector3.Angle(transform.forward, face) > 25f)
                {
                    RotateMoveDir(face.normalized);
                    _turtle.AISetMove(new Vector2(face.normalized.x, face.normalized.z));
                    return;
                }
                _turtle.AISetMove(Vector2.zero);
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
                desired = (NextWaypoint() - transform.position);
                desired = Flat(desired).normalized;
                if (desired == Vector3.zero) desired = toDest.normalized;
                desired = ApplyLocalAvoidance(desired);
                DetectStuck();
            }

            RotateMoveDir(desired);
            _turtle.AISetMove(new Vector2(_moveDir.x, _moveDir.z));
        }

        private void RotateMoveDir(Vector3 desired)
        {
            if (_moveDir == Vector3.zero) _moveDir = transform.forward;
            _moveDir = Vector3.RotateTowards(_moveDir, desired, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
            _moveDir = Flat(_moveDir).normalized;
        }

        private Vector3 NextWaypoint()
        {
            bool navReady = _navMesh != null && _navMesh.IsReady;
            if (!navReady) return _destination;

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

        private float StraightDistanceAhead()
        {
            if (_corners.Length == 0) return Flat(_destination - transform.position).magnitude;
            return Flat(_corners[Mathf.Clamp(_cornerIndex, 0, _corners.Length - 1)] - transform.position).magnitude;
        }

        private float PathDistance(Vector3 to)
        {
            float straight = Flat(to - transform.position).magnitude;
            if (_navMesh != null && _navMesh.IsReady && !_navMesh.HasClearLine(transform.position, to))
                straight *= 1.4f;
            return straight;
        }

        private Vector3 ApplyLocalAvoidance(Vector3 desired)
        {
            Vector3 origin = transform.position + Vector3.up * (bodyHeight * 0.5f);
            float probeRadius = bodyRadius * 0.45f;
            Vector3 right = Vector3.Cross(Vector3.up, desired);

            float push = 0f;
            if (Probe(origin, desired, avoidDistance, probeRadius, out float frontDist))
            {
                bool leftFree = !Probe(origin, Quaternion.Euler(0f, -45f, 0f) * desired, avoidDistance, probeRadius, out _);
                bool rightFree = !Probe(origin, Quaternion.Euler(0f, 45f, 0f) * desired, avoidDistance, probeRadius, out _);
                float strength = 1f - frontDist / avoidDistance;
                push = (rightFree || !leftFree ? 1f : -1f) * (0.6f + strength);
            }
            else
            {
                if (Probe(origin, Quaternion.Euler(0f, -35f, 0f) * desired, avoidDistance * 0.5f, probeRadius, out _)) push += 0.25f;
                if (Probe(origin, Quaternion.Euler(0f, 35f, 0f) * desired, avoidDistance * 0.5f, probeRadius, out _)) push -= 0.25f;
            }

            return (desired + right * push).normalized;
        }

        private bool Probe(Vector3 origin, Vector3 dir, float distance, float radius, out float hitDistance,
            Collider[] ignore = null)
        {
            hitDistance = distance;
            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, dir, distance, ~0, QueryTriggerInteraction.Ignore);
            bool blocked = false;
            foreach (RaycastHit hit in hits)
            {
                if (hit.distance <= 0f) continue;
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (ignore != null && System.Array.IndexOf(ignore, hit.collider) >= 0) continue;
                if (hit.collider.GetComponentInParent<FlowerController>() != null) continue;
                if (hit.normal.y > 0.6f) continue;
                blocked = true;
                hitDistance = Mathf.Min(hitDistance, hit.distance);
            }
            return blocked;
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

                    FractureObject blocking = FindBlockingPot();
                    if (blocking != null)
                    {
                        _forcedSmashPot = blocking;
                        _forcedSmashUntil = Time.time + 5f;
                        _backOffTimer = 0.8f;
                        _backOffDir = -Flat(blocking.transform.position - transform.position).normalized;
                        return;
                    }

                    _backOffTimer = 0.6f;
                    float side = Random.value < 0.5f ? -1f : 1f;
                    _backOffDir = (Quaternion.Euler(0f, side * 120f, 0f) * _moveDir).normalized;
                }
            }
            else
            {
                _stuckTimer = 0f;
            }
        }

        #endregion

        #region Turbo

        private void UpdateTurbo()
        {
            float alignment = Vector3.Angle(transform.forward, _moveDir);

            if (_isBoosting)
            {
                _boostTimer -= Time.deltaTime;
                bool targetClose = _goal == Goal.Chase && _target != null
                                   && Flat(_target.transform.position - transform.position).magnitude < 4.5f;
                bool turning = alignment > 50f;
                if (_boostTimer <= 0f || targetClose || turning || _turtle.CurrentState != TurtleState.Boosting
                    || BatteryRatio() < lowBatteryRatio)
                {
                    StopTurbo();
                }
                return;
            }

            if (Time.time < _nextBoost || alignment > 20f) return;

            bool want = false;
            float duration = Random.Range(turboDuration.x, turboDuration.y);

            if (_goal == Goal.Chase && _target != null && BatteryRatio() >= minBatteryForTurbo)
            {
                float distance = Flat(_target.transform.position - transform.position).magnitude;
                want = distance >= turboMinDistance && distance <= turboMaxDistance
                       && _closingSpeed < turboClosingSpeed
                       && StraightDistanceAhead() > 6f;
            }
            else if ((_goal == Goal.Search || _goal == Goal.CollectSun) && BatteryRatio() > 0.75f)
            {
                want = StraightDistanceAhead() > 15f && Random.value < 0.15f * Time.deltaTime;
                duration = 1f;
            }

            if (want && _turtle.AIStartBoost())
            {
                _isBoosting = true;
                _boostTimer = duration;
            }
            else if (want)
            {
                _nextBoost = Time.time + 1f;
            }
        }

        private void StopTurbo()
        {
            if (_isBoosting) _turtle.AIStopBoost();
            _isBoosting = false;
            _nextBoost = Time.time + turboCooldown;
        }

        #endregion

        #region Capacités

        private void UseCapacities()
        {
            if (_turtle.CurrentState != TurtleState.Default && _turtle.CurrentState != TurtleState.Boosting) return;

            if (_forcedSmashPot != null)
            {
                if (!IsIntact(_forcedSmashPot) || Time.time > _forcedSmashUntil) _forcedSmashPot = null;
                else if (_backOffTimer <= 0f && _turtle.CurrentState == TurtleState.Default && TryForcedSmash()) return;
            }

            switch (_goal)
            {
                case Goal.Chase:
                    if (TryBite(_target)) return;
                    if (_turtle.CurrentState == TurtleState.Default && TrySmashPot()) return;
                    if (_turtle.CurrentState == TurtleState.Default && TryDash(_target, false)) return;
                    break;

                case Goal.Unearth:
                    if (TryBite(_target)) return;
                    if (_unearthWithDash && _turtle.CurrentState == TurtleState.Default && TryDash(_target, true)) return;
                    break;

                case Goal.BiteRoseTrap:
                    if (TryBiteRoseTrap()) return;
                    break;

                case Goal.Search:
                    if (_turtle.CurrentState == TurtleState.Default && TrySmashPot()) return;
                    if (_turtle.CurrentState == TurtleState.Default && TrySearchScan()) return;
                    break;
            }

            if (_turtle.CurrentState == TurtleState.Default) TryTrap();
        }

        private bool TryBite(FlowerController target)
        {
            if (target == null || target.isDead || Time.time < _nextBite) return false;
            if (target.isInvincible || target.isUnhittable) return false;

            Vector3 toTarget = Flat(target.transform.position - transform.position);
            if (toTarget.magnitude > biteRange || Vector3.Angle(transform.forward, toTarget) > biteAngle) return false;

            if (_isBoosting) StopTurbo();
            _turtle.AIBite();
            _nextBite = Time.time + biteCooldown;
            return true;
        }

        private RoseTrap FindNearestRoseTrap(out float distance)
        {
            RoseTrap best = null;
            distance = float.MaxValue;
            foreach (RoseTrap trap in _roseTraps)
            {
                if (trap == null || IsIgnored(trap)) continue;
                float d = Flat(trap.transform.position - transform.position).magnitude;
                if (d < distance)
                {
                    distance = d;
                    best = trap;
                }
            }
            return best;
        }

        private bool TryBiteRoseTrap()
        {
            if (_roseTrapTarget == null || Time.time < _nextBite) return false;

            Vector3 toTrap = Flat(_roseTrapTarget.transform.position - transform.position);
            if (toTrap.magnitude > biteRange || Vector3.Angle(transform.forward, toTrap) > biteAngle) return false;

            if (_isBoosting) StopTurbo();
            _turtle.AIBite();
            _nextBite = Time.time + biteCooldown;
            return true;
        }

        private bool TryDash(FlowerController target, bool destruction)
        {
            if (target == null || target.isDead || Time.time < _nextDash) return false;
            if (!destruction && (target.isInvincible || target.isUnhittable)) return false;

            Vector3 aimPoint = destruction ? target.transform.position : PredictPosition(target);
            Vector3 toTarget = Flat(aimPoint - transform.position);
            float distance = toTarget.magnitude;
            float min = destruction ? 2f : dashMinDistance;
            float max = destruction ? unearthStandOff + 2.5f : dashMaxDistance;
            if (distance < min || distance > max) return false;

            if (_navMesh != null && !_navMesh.HasClearLine(transform.position, aimPoint)) return false;
            if (Probe(transform.position + Vector3.up * (bodyHeight * 0.5f), toTarget.normalized,
                    Mathf.Max(0.1f, distance - 1.5f), bodyRadius * 0.6f, out _)) return false;

            _nextDash = Time.time + dashCooldown;
            if (!destruction && Random.value > dashChance) return false;

            int level = destruction ? _turtle.DashLevelCount - 1 : Mathf.Min(1, _turtle.DashLevelCount - 1);
            if (_isBoosting) StopTurbo();
            if (!_turtle.AIStartCharge()) return false;

            _smashTarget = null;
            _isCharging = true;
            _chargeTarget = _turtle.GetDashThreshold(level) + 0.05f;
            return true;
        }

        private bool TrySearchScan()
        {
            if (Time.time < _nextScan || BatteryRatio() < minBatteryForScan) return false;

            bool arrived = _hasSearchPoint && Flat(_searchPoint - transform.position).magnitude < 4f;
            bool periodic = Time.time >= _nextSearchScan && BatteryRatio() > 0.3f;
            if (!arrived && !periodic) return false;

            _turtle.AIScan();
            _nextScan = Time.time + scanCooldown;
            _nextSearchScan = Time.time + searchScanInterval;
            _searchedSpots.Add((transform.position, Time.time));
            if (arrived) _hasSearchPoint = false;
            return true;
        }

        private void TryTrap()
        {
            if (Time.time - _startTime < firstTrapDelay || Time.time < _nextTrap) return;
            if (!_turtle.CanPlaceTrap || BatteryRatio() < minBatteryForTrap) return;
            if (TrapNearby(transform.position)) return;

            bool place = false;

            foreach (var flower in _flowers)
            {
                if (flower.isDead && Flat(flower.transform.position - transform.position).magnitude < 3f)
                {
                    place = true;
                    break;
                }
            }

            if (!place)
            {
                foreach (var flower in _flowers)
                {
                    if (flower.isDead || flower.IsPlanted) continue;
                    Vector3 to = Flat(flower.transform.position - transform.position);
                    if (to.magnitude < 6f && Vector3.Dot(transform.forward, to.normalized) < -0.4f)
                    {
                        place = true;
                        break;
                    }
                }
            }

            if (!place && _goal == Goal.Search)
            {
                Sun sun = FindNearestSun(transform.position, out float sunDistance);
                if (sun != null && sunDistance < 3f && _sunsConsideredForTrap.Add(sun))
                    place = Random.value < 0.4f;
            }

            if (!place) return;
            _turtle.AIPlaceTrap();
            _nextTrap = Time.time + trapCooldown;
        }

        private bool TrapNearby(Vector3 position)
        {
            foreach (GameObject trap in GameManager.Instance.TurtleTrap)
            {
                if (trap != null && Flat(trap.transform.position - position).magnitude < minDistanceBetweenTraps)
                    return true;
            }
            return false;
        }

        private void RefreshPots()
        {
            _potListRefreshTimer -= 0.5f;
            if (_potListRefreshTimer <= 0f)
            {
                _potListRefreshTimer = 5f;
                _pots = new List<FractureObject>(FindObjectsByType<FractureObject>(FindObjectsSortMode.None));
            }

            bool changed = false;
            foreach (var pot in _pots)
            {
                if (pot == null) continue;
                bool intact = IsIntact(pot);
                if (_potIntact.TryGetValue(pot, out bool wasIntact) && wasIntact != intact) changed = true;
                _potIntact[pot] = intact;
            }
            if (changed && _navMesh != null && _navMesh.IsReady)
            {
                _navMesh.RequestRebuild();
                _nextRepath = 0f;
            }
        }

        private static bool IsIntact(FractureObject pot)
        {
            var col = pot.GetComponent<Collider>();
            return col != null && col.enabled;
        }

        private bool TrySmashPot()
        {
            if (Time.time < _nextPot || Time.time < _nextDash || BatteryRatio() < minBatteryForPot) return false;

            Vector3 toDest = Flat(_destination - transform.position);
            float destDistance = toDest.magnitude;
            bool detour = destDistance > potMinDistance && PathLength() > destDistance * potDetourFactor;
            Vector3 origin = transform.position + Vector3.up * (bodyHeight * 0.5f);

            foreach (var pot in _pots)
            {
                if (pot == null || !IsIntact(pot)) continue;

                Vector3 toPot = Flat(pot.transform.position - transform.position);
                float distance = toPot.magnitude;
                if (distance > potMaxDistance) continue;

                bool blocksTheWay = detour && distance < destDistance && Vector3.Angle(toPot, toDest) < 30f;
                bool hidingSpot = _goal == Goal.Search && HiddenGuessNear(pot.transform.position, 4f);
                if (!blocksTheWay && !hidingSpot) continue;

                if (GapTo(pot) < minRunUpGap)
                {
                    _forcedSmashPot = pot;
                    _forcedSmashUntil = Time.time + 4f;
                    _backOffTimer = 0.5f;
                    _backOffDir = -toPot.normalized;
                    return true;
                }

                var potColliders = pot.GetComponentsInChildren<Collider>();
                if (Probe(origin, toPot.normalized, Mathf.Max(0.1f, distance - 1f), bodyRadius * 0.6f, out _, potColliders))
                    continue;

                if (_isBoosting) StopTurbo();
                if (!_turtle.AIStartCharge()) return false;

                _smashTarget = pot;
                _isCharging = true;
                _chargeTarget = _turtle.GetDashThreshold(_turtle.DashLevelCount - 1) + 0.05f;
                _nextPot = Time.time + potCooldown;
                _nextDash = Time.time + dashCooldown;
                return true;
            }
            return false;
        }

        private FractureObject FindBlockingPot()
        {
            FractureObject best = null;
            float bestDistance = bodyRadius + 2f;
            foreach (var pot in _pots)
            {
                if (pot == null || !IsIntact(pot)) continue;
                Vector3 toPot = Flat(pot.transform.position - transform.position);
                float distance = toPot.magnitude;
                if (distance > bestDistance || Vector3.Angle(_moveDir, toPot) > 80f) continue;
                bestDistance = distance;
                best = pot;
            }
            return best;
        }

        private bool TryForcedSmash()
        {
            Vector3 toPot = Flat(_forcedSmashPot.transform.position - transform.position);
            if (GapTo(_forcedSmashPot) < minRunUpGap)
            {
                _backOffTimer = 0.5f;
                _backOffDir = toPot.sqrMagnitude > 0.01f ? -toPot.normalized : -transform.forward;
                return false;
            }

            if (_isBoosting) StopTurbo();
            if (!_turtle.AIStartCharge()) return false;

            _smashTarget = _forcedSmashPot;
            _forcedSmashPot = null;
            _isCharging = true;
            _chargeTarget = _turtle.GetDashThreshold(_turtle.DashLevelCount - 1) + 0.05f;
            _nextPot = Time.time + potCooldown;
            _nextDash = Time.time + dashCooldown;
            return true;
        }

        private float GapTo(FractureObject pot)
        {
            float closest = float.MaxValue;
            foreach (Collider col in pot.GetComponentsInChildren<Collider>())
            {
                if (!col.enabled || col.isTrigger) continue;
                Vector3 point = col.bounds.ClosestPoint(transform.position);
                closest = Mathf.Min(closest, Flat(point - transform.position).magnitude);
            }
            if (closest == float.MaxValue) closest = Flat(pot.transform.position - transform.position).magnitude;
            return closest - bodyRadius;
        }

        private bool HiddenGuessNear(Vector3 position, float radius)
        {
            foreach (var mem in _memory.Values)
            {
                if (mem.Hidden && !mem.Revealed && Flat(mem.Guess - position).magnitude <= radius) return true;
            }
            return false;
        }

        private float PathLength()
        {
            if (_corners.Length == 0) return Flat(_destination - transform.position).magnitude;
            int i = Mathf.Clamp(_cornerIndex, 0, _corners.Length - 1);
            float length = Flat(_corners[i] - transform.position).magnitude;
            for (; i < _corners.Length - 1; i++)
                length += Flat(_corners[i + 1] - _corners[i]).magnitude;
            return length;
        }

        private void UpdateCharging()
        {
            if (_turtle.CurrentState != TurtleState.Charging)
            {
                _isCharging = false;
                return;
            }

            Vector3 aim = transform.forward;
            if (_smashTarget != null)
            {
                aim = Flat(_smashTarget.transform.position - transform.position);
                if (aim.sqrMagnitude < 0.01f) aim = transform.forward;
            }
            else if (_target != null)
            {
                Vector3 point = _goal == Goal.Unearth ? _target.transform.position : PredictPosition(_target);
                aim = Flat(point - transform.position);
                if (aim.sqrMagnitude < 0.01f) aim = transform.forward;
            }
            aim.Normalize();
            _turtle.AISetMove(new Vector2(aim.x, aim.z));

            if (_turtle.ChargeTime >= _chargeTarget)
            {
                _turtle.AIReleaseCharge();
                _isCharging = false;
                _smashTarget = null;
            }
        }

        #endregion

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _goal switch
            {
                Goal.Chase => Color.red,
                Goal.Unearth => Color.magenta,
                Goal.CollectSun => Color.yellow,
                _ => Color.cyan
            };
            Gizmos.DrawWireSphere(_destination, 0.5f);
            for (int i = Mathf.Max(0, _cornerIndex - 1); i < _corners.Length - 1; i++)
                Gizmos.DrawLine(_corners[i] + Vector3.up * 0.3f, _corners[i + 1] + Vector3.up * 0.3f);
            Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + _moveDir * 2f);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using CharacterController = Intégration.V1.Scripts.Game.Characters.CharacterController;

namespace Intégration.V1.Scripts.Game
{
    public class RuntimeNavMesh : MonoBehaviour
    {
        public static RuntimeNavMesh Instance { get; private set; }

        public bool IsReady { get; private set; }
        public int AgentTypeID => _settings.agentTypeID;

        private NavMeshBuildSettings _settings;
        private NavMeshData _data;
        private NavMeshDataInstance _instance;
        private AsyncOperation _operation;
        private bool _rebuildRequested;
        private float _smallObstaclePadding;
        private const float SmallObstacleSize = 2f;

        public static RuntimeNavMesh Ensure(GameObject owner, float agentRadius, float agentHeight, float smallObstaclePadding)
        {
            Scene scene = owner.scene;
            if (Instance != null)
            {
                if (Instance.gameObject.scene == scene) return Instance;
                Destroy(Instance.gameObject);
                Instance = null;
            }

            var go = new GameObject("Runtime NavMesh (IA)");
            SceneManager.MoveGameObjectToScene(go, scene);
            Instance = go.AddComponent<RuntimeNavMesh>();
            Instance._smallObstaclePadding = Mathf.Max(0f, smallObstaclePadding);
            Instance.Build(agentRadius, agentHeight);
            return Instance;
        }

        private void Build(float agentRadius, float agentHeight)
        {
            _settings = NavMesh.CreateSettings();
            _settings.agentRadius = agentRadius;
            _settings.agentHeight = agentHeight;
            _settings.agentSlope = 45f;
            _settings.agentClimb = 0.6f;
            _settings.minRegionArea = 1f;

            _data = new NavMeshData(_settings.agentTypeID);
            _instance = NavMesh.AddNavMeshData(_data);
            StartUpdate();
        }

        public void RequestRebuild()
        {
            if (_operation != null && !_operation.isDone) _rebuildRequested = true;
            else StartUpdate();
        }

        private void StartUpdate()
        {
            var sources = new List<NavMeshBuildSource>();
            var hugeBounds = new Bounds(Vector3.zero, Vector3.one * 5000f);
            NavMeshBuilder.CollectSources(hugeBounds, ~0, NavMeshCollectGeometry.PhysicsColliders, 0,
                new List<NavMeshBuildMarkup>(), sources);

            bool hasBounds = false;
            Bounds levelBounds = default;
            for (int i = sources.Count - 1; i >= 0; i--)
            {
                var col = sources[i].component as Collider;
                if (col == null) continue;
                bool isDynamic = col.attachedRigidbody != null && !col.attachedRigidbody.isKinematic;
                bool isCharacter = col.GetComponentInParent<CharacterController>() != null;
                if (col.isTrigger || isDynamic || isCharacter)
                {
                    sources.RemoveAt(i);
                    continue;
                }

                if (sources[i].shape == NavMeshBuildSourceShape.Mesh && sources[i].sourceObject is Mesh mesh && !mesh.isReadable)
                {
                    var box = sources[i];
                    box.shape = NavMeshBuildSourceShape.Box;
                    box.sourceObject = null;
                    box.size = mesh.bounds.size;
                    box.transform = col.transform.localToWorldMatrix * Matrix4x4.Translate(mesh.bounds.center);
                    sources[i] = box;
                }

                Bounds b = col.bounds;
                if (_smallObstaclePadding > 0f && col is not TerrainCollider
                    && Mathf.Max(b.size.x, b.size.z) < SmallObstacleSize)
                {
                    var padded = sources[i];
                    padded.shape = NavMeshBuildSourceShape.Box;
                    padded.sourceObject = null;
                    padded.size = b.size + new Vector3(_smallObstaclePadding * 2f, 0f, _smallObstaclePadding * 2f);
                    padded.transform = Matrix4x4.TRS(b.center, Quaternion.identity, Vector3.one);
                    sources[i] = padded;
                }

                if (!hasBounds) { levelBounds = col.bounds; hasBounds = true; }
                else levelBounds.Encapsulate(col.bounds);
            }

            if (!hasBounds) levelBounds = new Bounds(Vector3.zero, new Vector3(200f, 50f, 200f));
            levelBounds.Expand(new Vector3(2f, 4f, 2f));

            _operation = NavMeshBuilder.UpdateNavMeshDataAsync(_data, _settings, sources, levelBounds);
        }

        private void Update()
        {
            if (_operation == null || !_operation.isDone) return;

            if (!IsReady)
            {
                IsReady = true;
            }

            if (_rebuildRequested)
            {
                _rebuildRequested = false;
                StartUpdate();
            }
        }

        private void OnDestroy()
        {
            if (_instance.valid) NavMesh.RemoveNavMeshData(_instance);
            if (Instance == this) Instance = null;
        }

        public NavMeshQueryFilter Filter => new NavMeshQueryFilter
        {
            agentTypeID = _settings.agentTypeID,
            areaMask = NavMesh.AllAreas
        };

        public bool SamplePosition(Vector3 position, float maxDistance, out Vector3 result)
        {
            result = position;
            if (!IsReady) return false;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, Filter))
            {
                result = hit.position;
                return true;
            }
            return false;
        }

        public bool CalculatePath(Vector3 from, Vector3 to, NavMeshPath path)
        {
            if (!IsReady) return false;
            if (!SamplePosition(from, 3f, out Vector3 start)) return false;
            if (!SamplePosition(to, 6f, out Vector3 end)) return false;
            return NavMesh.CalculatePath(start, end, Filter, path) && path.status != NavMeshPathStatus.PathInvalid;
        }

        public bool HasClearLine(Vector3 a, Vector3 b)
        {
            if (!IsReady) return true;
            if (!SamplePosition(a, 3f, out Vector3 start)) return false;
            return !NavMesh.Raycast(start, b, out _, Filter);
        }

        public bool RandomPoint(Vector3 around, float radius, out Vector3 point)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 r = Random.insideUnitCircle * radius;
                if (SamplePosition(around + new Vector3(r.x, 0f, r.y), 4f, out point)) return true;
            }
            point = around;
            return false;
        }
    }
}

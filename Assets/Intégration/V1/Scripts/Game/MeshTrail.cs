using System.Collections;
using UnityEngine;

namespace Intégration.V1.Scripts.Game
{
    public class MeshTrail : MonoBehaviour
    {
        [SerializeField] private float activateTime = 2f;

        [Header("Mesh Related")] [SerializeField]
        private float meshRefreshRate = 0.1f;

        [SerializeField] private float meshDestroyDelay = 3f;
        [SerializeField] private Transform positionToSpawn;

        [Header("Shader Related")] [SerializeField]
        private Material mat;

        [SerializeField] private string shaderVarRef;
        [SerializeField] private float shaderVarRate = 0.1f;
        [SerializeField] private float shaderVarRefreshRate = 0.05f;

        private bool isTrailActive;
        private SkinnedMeshRenderer[] _skinnedMeshRenderers;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive)
            {
                isTrailActive = true;
                StartCoroutine(ActivateTrail(activateTime));
            }
        }

        public void InvokePassive()
        {
            InvokeRepeating(nameof(MeshTrail.PassiveMeshActivate), 0.5f, 2f);
        }

        public void StopPassive()
        {
            isTrailActive = false;
            CancelInvoke(nameof(PassiveMeshActivate));
        }

        private void PassiveMeshActivate()
        {
            if (!isTrailActive)
            {
                isTrailActive = true;
                StartCoroutine(ActivateTrail(activateTime));
            }
        }

        IEnumerator ActivateTrail(float timeActive)
        {
            while (timeActive > 0)
            {
                timeActive -= meshRefreshRate;

                if (_skinnedMeshRenderers == null)
                {
                    _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                }

                for (int i = 0; i < _skinnedMeshRenderers.Length; i++)
                {
                    GameObject gObj = new GameObject();
                    gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);
                    MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                    MeshFilter mf = gObj.AddComponent<MeshFilter>();

                    Mesh mesh = new Mesh();
                    _skinnedMeshRenderers[i].BakeMesh(mesh);

                    mf.mesh = mesh;
                    mr.material = mat;
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mr.receiveShadows = false;

                    StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));
                    Destroy(gObj, meshDestroyDelay);
                }

                yield return new WaitForSeconds(meshRefreshRate);
            }

            isTrailActive = false;
        }

        IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
        {
            float valueToAnimate = mat.GetFloat(shaderVarRef);

            while (valueToAnimate > goal)
            {
                valueToAnimate -= rate;
                mat.SetFloat(shaderVarRef, valueToAnimate);
                yield return new WaitForSeconds(refreshRate);
            }
        }
    }
}
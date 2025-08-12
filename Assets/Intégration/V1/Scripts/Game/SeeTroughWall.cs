using DG.Tweening;
using UnityEngine;

namespace Intégration.V1.Scripts.Game
{
    public class SeeTroughWall : MonoBehaviour
    {
        [SerializeField] private GameObject _camera;
        public static GameObject _turtle;
        [SerializeField] private LayerMask _mask;

        private void Awake()
        {
            _camera = FindObjectOfType<Camera>().gameObject;
        }

        void Update()
        {
            RaycastHit hit;
            Vector3 cameraPos = _camera.transform.position;
            if (Physics.Raycast(cameraPos, (_turtle.transform.position - cameraPos).normalized, out hit,
                    Mathf.Infinity, _mask))
                //  Debug.DrawRay(cameraPos, (_turtle.transform.position - cameraPos), Color.red,50);

            {
                if (hit.collider?.gameObject.tag == "SeeTroughZone")
                {
                    gameObject.transform.DOScale(0, 0.5f);
                    // Debug.Log("can see");
                }
                else
                {
                    gameObject.transform.DOScale(1.4f, 0.5f);
                    //  Debug.Log("Hide");
                }
            }
        }
    }
}
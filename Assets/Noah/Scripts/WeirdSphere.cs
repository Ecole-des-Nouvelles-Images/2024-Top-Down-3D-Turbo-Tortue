using UnityEngine;
using Cinemachine;
using Screen = UnityEngine.Device.Screen;

namespace Noah.Scripts
{
    public class WeirdSphere : MonoBehaviour
    {
        public Camera cam;
        private Renderer _renderer;
        private static readonly int ObjScreenPos = Shader.PropertyToID("_ObjScreenPos");

        private void Start()
        {
            cam = Camera.main;
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam != null)
            {
                Vector3 screenPoint = cam.WorldToScreenPoint(transform.position);
                screenPoint.x = screenPoint.x / Screen.width;
                screenPoint.y = screenPoint.y / Screen.height;
                _renderer.material.SetVector(ObjScreenPos, screenPoint);
            }
        }
    }
}
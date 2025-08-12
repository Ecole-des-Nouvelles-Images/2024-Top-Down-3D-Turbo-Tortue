using UnityEngine;
using Cinemachine;
using Screen = UnityEngine.Device.Screen;

namespace Noah.Scripts
{
    public class LookAtCamera : MonoBehaviour
    {
        public Camera cam;

        private void Start()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam != null)
            {
                transform.forward = cam.transform.position - transform.position;
            }
        }
    }
}
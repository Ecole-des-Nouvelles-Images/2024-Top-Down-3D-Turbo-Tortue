using UnityEngine;

namespace Intégration.V1.Scripts.Game
{
    public class OverlayFollowCamera : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera overlayCamera;


        private void Update()
        {
            overlayCamera.fieldOfView = mainCamera.fieldOfView;
        }
    }
}
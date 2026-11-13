using Michael.Scripts.Manager;
using UnityEngine;

namespace Intégration.V1.Scripts.Game.FeedBack
{
    public class SlowMotion : MonoBehaviour
    {
        public void StartSlowMotion()
        {
            GameManager.Instance.CameraShake(0.5f, 0.1f, 10);
        }

        public void StopSlowMotion()
        {
        }
    }
}

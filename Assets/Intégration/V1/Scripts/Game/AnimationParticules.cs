using UnityEngine;

namespace Intégration.V1.Scripts.Game
{
    public class AnimationParticules : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _crashParticules;
        [SerializeField] private ParticleSystem _dirtParticules;
        [SerializeField] private ParticleSystem _runParticules;
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void ShowCrashParticules()
        {
            if (_crashParticules)
            {
                _crashParticules.Play();
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.QTEFailed);
            }
        }

        public void ShowDirtParticules()
        {
            if (_dirtParticules)
            {
                _dirtParticules.Play();
            }
        }

        public void RunParticules()
        {
            if (_runParticules)
            {
                _runParticules.Play();
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.FlowersRun);
            }
        }


        public void DestroyDirt()
        {
            Destroy(gameObject);
        }
    }
}
using System;
using DG.Tweening;
using Michael.Scripts.Controller;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Intégration.V1.Scripts.Game
{
    public class FractureObject : MonoBehaviour
    {
        [SerializeField] private GameObject originalObject;
        [SerializeField] private GameObject explosionVFX;
        [SerializeField] private float _respawnDelay = 30f;
        
        private TurtleController _turtleController;
        private Collider _collider;

        private void Start()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Turtle"))
            {
                _turtleController = collision.gameObject.GetComponent<TurtleController>();
                if (_turtleController.destructionMode)
                {
                    Explode();
                    _turtleController.destructionMode = false;
                }
            }
        }

        [ContextMenu("explode")]
        private void Explode()
        {
            _collider.enabled = false;
           originalObject.SetActive(false);
           explosionVFX.SetActive(true);
           
           Invoke(nameof(RespawnObject),_respawnDelay);
        }

        private void RespawnObject()
        {
            _collider.enabled = true;
            originalObject.SetActive(true);
            explosionVFX.SetActive(false);
        }

     
    }
}
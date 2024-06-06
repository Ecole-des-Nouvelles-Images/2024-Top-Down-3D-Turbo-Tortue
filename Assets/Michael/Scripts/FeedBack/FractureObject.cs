using System;
using System.Collections;
using DG.Tweening;
using Michael.Scripts.Controller;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Michael.Scripts.FeedBack
{
    public class FractureObject : MonoBehaviour
    {
        [SerializeField] private GameObject originalObject;
        [SerializeField] private GameObject fracturedObject;
        [SerializeField] private GameObject explosionVFX;
        [SerializeField] private float explosionMinForce = 5f;
        [SerializeField] private float explosionMaxForce = 100f;
        [SerializeField] private float explosionForceRadius = 10f;
        [SerializeField] private float shrinkDuration = 2f;

        private TurtleController _turtleController;
        private GameObject _fractObject;

        private void Awake()
        {
            _turtleController = FindObjectOfType<TurtleController>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Turtle"))
            {
                if (_turtleController.destructionMode)
                {
                    Explode();
                    Debug.Log("explode");
                    _turtleController.destructionMode = false;
                }
            }
        }

        private void Explode()
        {
            if (originalObject != null)
            {
                originalObject.SetActive(false);

                if (fracturedObject != null)
                {
                    _fractObject = Instantiate(fracturedObject, transform.position, transform.rotation) as GameObject;

                    foreach (Transform t in _fractObject.transform)
                    {
                        var rb = t.GetComponent<Rigidbody>();

                        if (rb != null)
                        {
                            rb.AddExplosionForce(Random.Range(explosionMinForce, explosionMaxForce), originalObject.transform.position, explosionForceRadius);

                            ShrinkAndDestroy(t);
                        }
                    }

                    if (explosionVFX != null)
                    {
                        GameObject exploVFX = Instantiate(explosionVFX,  transform.position, transform.rotation) as GameObject;
                        Destroy(exploVFX, 7);
                    }
                }
            }
        }

        private void ShrinkAndDestroy(Transform t)
        {
            t.DOScale(Vector3.zero, shrinkDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => Destroy(t.gameObject));
        }
    }
}

using System;
using Intégration.V1.Scripts.Game.Characters;
using UnityEngine;

namespace Intégration.V1.Scripts.Game
{
    public class ScannerArea : MonoBehaviour
    {

        public event Action OnFlowerDetected;

        private void ChangeLayerOfScannedObjects(GameObject scannedObject, string targetLayerName)
        {
            int targetLayer = LayerMask.NameToLayer(targetLayerName);
            if (targetLayer == -1)
            {
                Debug.LogWarning("Layer not found: " + targetLayerName);
                return;
            }

            // Change uniquement les MeshRenderer, pas les UI ou autres
            Renderer[] renderers = scannedObject.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                renderer.gameObject.layer = targetLayer;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Flower"))
            {
                DandelionController dandelionController = other.GetComponent<DandelionController>();
                OnFlowerDetected?.Invoke();

                if (dandelionController == null || !dandelionController.IsPlanted)
                {
                    ChangeLayerOfScannedObjects(other.gameObject, "Flower");
                }

            }
            else if ( (other.CompareTag("FlowerTrap")))
            {
                ChangeLayerOfScannedObjects(other.gameObject, "Flower");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Flower"))
            {
                ChangeLayerOfScannedObjects(other.gameObject, "Default");
            }
            else if ( (other.CompareTag("FlowerTrap")))
            {
                ChangeLayerOfScannedObjects(other.gameObject, "Default");
            }
        }
    }
}
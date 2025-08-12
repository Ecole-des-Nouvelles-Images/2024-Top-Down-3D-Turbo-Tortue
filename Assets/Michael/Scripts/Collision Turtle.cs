using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTurtle : MonoBehaviour
{
    [SerializeField] private Collider attackCollider;
    

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            attackCollider.enabled = false;
        }
        
    }
    
}

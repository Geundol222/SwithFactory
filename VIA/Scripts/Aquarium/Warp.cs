using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : MonoBehaviour
{
    public Transform otherSpawnPoint;
    public LayerMask playerMask;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            other.gameObject.GetComponent<PlayerController>()?.Warp(otherSpawnPoint.position);
        }    
    }
}

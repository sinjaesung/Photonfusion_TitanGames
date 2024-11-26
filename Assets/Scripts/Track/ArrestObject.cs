using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class ArrestObject : MonoBehaviour
{
    public int ArrestTime = 3;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ArrestObject>>");
        if (other.TryGetComponent(out PlayerEntity player))
        {
            player.ArrestUp(ArrestTime);
        }
    }
}


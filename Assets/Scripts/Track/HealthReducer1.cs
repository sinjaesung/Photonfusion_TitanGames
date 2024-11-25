using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class HealthReducer1 : MonoBehaviour
{
    public int ReduceAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HealthReducer1>>");
        if (other.TryGetComponent(out PlayerEntity player))
        {
            //player.Controller.GiveBoost(true, boostLevel);
            player.Controller.UpdateHealth(ReduceAmount);
        }
    }
}


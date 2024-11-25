using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boostpad : MonoBehaviour
{
    public int boostLevel = 3;

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("BoostPad>>");
        if (other.TryGetComponent(out PlayerEntity player))
        {
            player.Controller.GiveBoost(true, boostLevel);
        }
    }
}
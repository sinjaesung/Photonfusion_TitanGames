using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class SpinOutObject : MonoBehaviour
{
    public int SpinOutTime = 3;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("SpinOutObject>>");
        if (other.TryGetComponent(out PlayerEntity player))
        {
            //player.Controller.GiveBoost(true, boostLevel);
            player.Controller.IsArrested = true;

            StartCoroutine(OnArrestOut(player));
        }
    }
    private IEnumerator OnArrestOut(PlayerEntity player)
    {
        yield return new WaitForSeconds(SpinOutTime);

        player.Controller.IsArrested = false;
    }
}


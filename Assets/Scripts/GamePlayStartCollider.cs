using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayStartCollider : NetworkBehaviour
{
    public bool IsCollide = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("4명중 임의 한명이 여기에 닿으면 거인스폰 하게끔>>");

        IsCollide = true;
    }
}

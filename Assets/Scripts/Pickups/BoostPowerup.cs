using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPwerup : SpawnedPowerup
{
    public override void Init(PlayerEntity spawner)
    {
        base.Init(spawner);

        Debug.Log("BoostPowerup Init>>" + spawner.name);
        spawner.Controller.GiveBoost(false, 1);
    }

    public override void Spawned()
    {
        base.Spawned();

        Runner.Despawn(Object);
    }
}

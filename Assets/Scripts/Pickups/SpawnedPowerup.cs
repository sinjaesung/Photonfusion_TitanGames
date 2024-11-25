using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public abstract class SpawnedPowerup : NetworkBehaviour, ICollidable
{
    [Networked] public NetworkBool HasInit { get; private set; }

    public virtual void Init(PlayerEntity spawner) { }

    public override void Spawned()
    {
        base.Spawned();

        Debug.Log("SpawnedPowerup>>");
        HasInit = true;
    }

    public virtual bool Collide(PlayerEntity player)
    {
        return false;
    }
}
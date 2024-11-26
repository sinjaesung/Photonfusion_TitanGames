using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class ShieldPowerup : SpawnedPowerup
{
    public new Collider collider;
    public float enableDelay = 0.5f;

    public float DefensePower;

    [Networked] public TickTimer CollideTimer { get; set; }

    private void Awake()
    {
        collider.enabled = false;
    }

    public override void Spawned()
    {
        base.Spawned();

        AudioManager.PlayAndFollow("ItemCollect", transform, AudioManager.MixerTarget.SFX);

        CollideTimer = TickTimer.CreateFromSeconds(Runner, enableDelay);
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        collider.enabled = CollideTimer.ExpiredOrNotRunning(Runner);
    }

    public override bool Collide(PlayerEntity player)
    {
        Debug.Log("ShieldPowerup Collide>>" + HasInit);
        if (Object.IsValid && !HasInit) return false;

        player.DefenseUp(DefensePower);

        Runner.Despawn(Object);

        return true;
    }
}

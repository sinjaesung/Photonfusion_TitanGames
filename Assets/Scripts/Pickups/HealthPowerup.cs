using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class HealthPowerup : SpawnedPowerup
{
    public new Collider collider;
    public float enableDelay = 0.5f;

    [Networked] public TickTimer CollideTimer { get; set; }

    public int ReduceAmount = -10;

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
        Debug.Log("HealthPowerup Collide>>" + HasInit);
        if (Object.IsValid && !HasInit) return false;

        //player.SpinOut();
        player.Controller.UpdateHealth(ReduceAmount);

        Runner.Despawn(Object);

        return true;
    }
}

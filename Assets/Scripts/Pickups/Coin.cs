using Fusion;
using UnityEngine;

public class Coin : NetworkBehaviour, ICollidable
{
    [Networked]
    public NetworkBool IsActive { get; set; } = true;

    public Transform visuals;

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach(var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(IsActive):
                    OnIsEnabledChangedCallback(this);
                    break;
            }
        }
    }

    public bool Collide(PlayerEntity player)
    {
        if (Object == null || !Runner.Exists(Object))
            return false;

        if (IsActive)
        {
            player.CoinCount++;

            IsActive = false;

            if (player.Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }

        return true;
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
    }
    private static void OnIsEnabledChangedCallback(Coin changed)
    {
        changed.visuals.gameObject.SetActive(changed.IsActive);

        if (!changed.IsActive)
            AudioManager.PlayAndFollow("coinSFX", changed.transform, AudioManager.MixerTarget.SFX);
    }
}
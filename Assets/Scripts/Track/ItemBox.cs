using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

public class ItemBox: NetworkBehaviour, ICollidable
{
    public GameObject model;
    public ParticleSystem breakParticle;
    public float cooldown = 5f;
    public Transform visuals;

    [Networked] public PlayerEntity player { get; set; }
    [Networked] public TickTimer DisabledTimer { get; set; }

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
                case nameof(player):
                    OnPlayerChanged(this);
                    break;
            }
        }
    }

    public bool Collide(PlayerEntity player_)
    {
        if(player_ != null && DisabledTimer.ExpiredOrNotRunning(Runner))
        {
            player = player_;
            DisabledTimer = TickTimer.CreateFromSeconds(Runner, cooldown);
            var powerUp = GetRandomPowerup();
            player.SetHeldItem(powerUp);
        }

        return true;
    }

    private static void OnPlayerChanged(ItemBox changed) { changed.OnPlayerChanged(); }
    private void OnPlayerChanged()
    {
        visuals.gameObject.SetActive(player == null);

        if (player == null)
            return;

        AudioManager.PlayAndFollow(
            player.HeldItem != null ? "itemCollectSFX" : "itemWasteSFX",
            transform,
            AudioManager.MixerTarget.SFX
        );

        breakParticle.Play();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if(DisabledTimer.ExpiredOrNotRunning(Runner) && player != null)
        {
            player = null;
        }
    }

    private int GetRandomPowerup()
    {
        var powerUps = ResourceManager.Instance.powerups;
        var seed = Runner.Tick;

        Random.InitState(seed);

        return Random.Range(0, powerUps.Length);
    }
}
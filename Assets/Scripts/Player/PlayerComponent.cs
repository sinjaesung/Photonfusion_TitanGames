using Fusion;
using UnityEngine;

public class PlayerComponent : NetworkBehaviour
{
    public Player player { get; private set; }
    public PlayerEntity playerentity { get; private set; }
    private void Awake()
    {
        Debug.Log("PlayerComponent Awake>>");
        player = GetComponentInParent<Player>();
    }
    public virtual void Init(PlayerEntity entity)
    {
        Debug.Log("PlayerComponent entity>>" + entity.name);
        playerentity = entity;
    }

    /// <summary>
    /// Called on the tick that the race has started. This method is tick-aligned.
    /// </summary>
    //public virtual void OnRaceStart() { }
    /// <summary>
    /// Called when this kart has crossed the finish line. This method is tick-aligned.
    /// </summary>
    //public virtual void OnLapCompleted(int lap, bool isFinish) { }
    /// <summary>
    /// Called when an item has been picked up. This method is tick-aligned.
    /// </summary>
    public virtual void OnEquipItem(Powerup powerup, float timeUntilCanUse) { }
}
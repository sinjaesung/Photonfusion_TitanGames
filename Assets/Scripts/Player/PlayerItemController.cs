using Fusion;
using UnityEngine;

public class PlayerItemController : PlayerComponent
{
    public float equipItemTimeout = 3f;
    public float useItemTimeout = 2.5f;

    [Networked]
    public TickTimer EquipCooldown { get; set; }

    public bool CanUseItem => playerentity.HeldItemIndex != -1 && EquipCooldown.ExpiredOrNotRunning(Runner);

    public override void OnEquipItem(Powerup powerup, float timeUntilCanUse)
    {
        base.OnEquipItem(powerup, timeUntilCanUse);

        EquipCooldown = TickTimer.CreateFromSeconds(Runner, equipItemTimeout);
    }

    public void UseItem()
    {
        if (!CanUseItem)
        {
            // We dont want to play the horn on re-simulations.
            if (!Runner.IsForward) return;

            playerentity.Audio.PlayHorn();
        }
        else
        {
            Debug.Log("PlayerItemController UseItem" + playerentity.HeldItem.itemName);
            playerentity.HeldItem.Use(Runner,playerentity);
            playerentity.HeldItemIndex = -1;
        }
    }
}
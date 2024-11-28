using Fusion;
using UnityEngine;
public class NetworkHealth : NetworkBehaviour
{
    [Header("Setup")]
    public float InitialHealth=3;

    public bool IsAlive => CurrentHealth > 0;
    public bool IsFinished => IsAlive == false;

    [Networked, HideInInspector, OnChangedRender(nameof(OnCurrentHealthChanged))]
    public float CurrentHealth { get; set; }

    public bool TakeHit(float damage)
    {
        if (IsAlive == false)
            return false;

        CurrentHealth -= damage;

        return true;
    }
    public void Revive()
    {
        CurrentHealth = InitialHealth;
    }
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            //Set initial health
            CurrentHealth = InitialHealth;
        }
    }

    public override void Render()
    {
        var interpolator = new NetworkBehaviourBufferInterpolator(this);
        bool isAlive = interpolator.Float(nameof(CurrentHealth)) > 0;
    }

    private void OnCurrentHealthChanged()
    {
        if (CurrentHealth <= 0)
            return; // Just health reset

       /* if (HasInputAuthority == false && ScalingRoot != null)
        {
            // Show hit reaction by simple scale. Scaling root
            // scale is lerped back to one in the Player script.
            ScalingRoot.localScale = new Vector3(0.85f, 1.15f, 0.85f);
        }*/
    }
}

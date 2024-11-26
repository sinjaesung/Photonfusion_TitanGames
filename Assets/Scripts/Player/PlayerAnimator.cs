using Fusion;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerAnimator : PlayerComponent
{
    //public ParticleSystem[] backfireEmitters;
    public ParticleSystem[] boostEmitters;
    //public ParticleSystem[] driftEmitters;
    //public ParticleSystem[] driftTierEmitters;
    //public ParticleSystem[] tireSmokeEmitters;

    [SerializeField] private NetworkMecanimAnimator _nma;
    [SerializeField] private Animator _animator;

    public Player controller;

    public GameObject DefensePowerup;
    public GameObject ArrestBox;

    private void Start()
    {
        //controller = player;
    }
    private void Update()
    {
        //controller = player;
    }
    public void AllowDrive()
    {
        controller.RefreshAppliedSpeed();
    }

    public override void Spawned()
    {
        base.Spawned();

        //controller.OnDriftTierIndexChanged += UpdateDriftState;
        controller.OnBoostTierIndexChanged += UpdateBoostState;

        controller.OnSpinOutChanged += val =>
        {
            if (!val) return;
            SetTrigger("Spinout");
        };
        controller.OnDefenseChanged += val =>
        {
            if (controller.PrevDefense == val)
            {
                return;
            }

            Debug.Log("PlayerAnimator OnDefenseChanged>>" + controller.PrevDefense + ">" + val);
            if (controller.PrevDefense < val)
            {
                Debug.Log("방어력 증가");
                DefensePowerup.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("방어력 감소");
                DefensePowerup.gameObject.SetActive(false);
            }
        };
        controller.OnArrestChanged += val =>
        {
            Debug.Log("PlayerAnimator OnArrestChanged>>");

            if (val == true)
            {
                Debug.Log("PlayerAnimator ArrestOn>>");
                ArrestBox.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("PlayerAnimator ArrestOff>>");
                ArrestBox.gameObject.SetActive(false);
            }
        };
        controller.OnDieChanged += val =>
        {
            if (!val) return;
            Debug.Log("PlayerAnimator OnDieChanged>>");
            SetTrigger("Die");
        };
        /* Kart.Controller.OnBumpedChanged += val =>
         {
             if (val)
             {
                 SetTrigger("Bump");
                 AudioManager.Play("bumpSFX", AudioManager.MixerTarget.SFX, transform.position);
             }
             else
             {
                 Kart.Controller.RefreshAppliedSpeed();
             }
         };*/

        /*controller.OnBackfireChanged += val =>
        {
            if (!val) return;
            PlayBackfire();
            AudioManager.Play("backfireSFX", AudioManager.MixerTarget.SFX, transform.position);
        };*/

        /* Kart.Controller.OnHopChanged += val => {
             if (!val) return;
             Kart.Animator.SetTrigger("Hop");
         };*/
    }

    private void OnDestroy()
    {
       // controller.OnDriftTierIndexChanged -= UpdateDriftState;
        controller.OnBoostTierIndexChanged -= UpdateBoostState;
    }

    /*private void UpdateDriftState(int index)
    {
        if (index == -1)
        {
            StopDrift();
            return;
        }

        var color = controller.driftTiers[index].color;
        foreach (var emitter in driftEmitters)
        {
            var main = emitter.main;
            main.startColor = color;
            foreach (var subEmitter in emitter.GetComponentsInChildren<ParticleSystem>())
            {
                var sub = subEmitter.main;
                sub.startColor = color;
            }

            emitter.Play(true);
        }

        foreach (var emitter in tireSmokeEmitters)
        {
            emitter.Play(true);
        }
    }
    private void StopDrift()
    {
        foreach (var emitter in driftEmitters)
        {
            emitter.Stop(true);
        }

        foreach (var emitter in tireSmokeEmitters)
        {
            emitter.Stop(true);
        }

        //StopSkidFX();
    }*/
    private void UpdateBoostState(int index)
    {
        if (index == 0)
        {
            Debug.Log("UpdateBoostState StopBoost>>"+ index);
            Invoke(nameof(StopBoost),4f);
            return;
        }

        //SetTrigger("Boost");

        Color color = controller.driftTiers[index].color;
        Debug.Log("UpdateBoostState>>" + color);
        foreach (var emitter in boostEmitters)
        {
            Debug.Log("UpdateBoostState boostEmitters>>" + emitter.name);
            var main = emitter.main;
            main.startColor = color;
            foreach (var subEmitter in emitter.GetComponentsInChildren<ParticleSystem>())
            {
                var sub = subEmitter.main;
                sub.startColor = color;
                Debug.Log("UpdateBoostState subEmitter>>" + subEmitter.name);
            }

            emitter.Play(true);
        }

        /*if (Object.HasInputAuthority)
        {
            Kart.Camera.speedLines.Play();
        }*/
    }
    public void StopBoost()
    {
        foreach (var emitter in boostEmitters)
        {
            emitter.Stop(true);
        }

       /* if (Object.HasInputAuthority)
        {
            Kart.Camera.speedLines.Stop();
        }*/
    }
    /*private void PlayBackfire()
    {
        //SetTrigger("Stall");
        foreach (var emitter in backfireEmitters)
        {
            emitter.Play(true);
        }
    }*/
    public void SetTrigger(string trigger)
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log("PlayerAnimator SetTrigger Object.HasStateAuthority>>" + trigger);
            _nma.SetTrigger(trigger);
        }
        else if (Object.HasInputAuthority && Runner.IsForward)
        {
            Debug.Log("PlayerAnimator  SetTrigger Object.HasInputAuthority>>" + trigger);
            _animator.SetTrigger(trigger);
        }
    }
}
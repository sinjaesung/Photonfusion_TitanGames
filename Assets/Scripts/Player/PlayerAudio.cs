using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerAudio : PlayerComponent
{
    public AudioSource StartSound;
    public AudioSource IdleSound;
    public AudioSource RunningSound;
    public AudioSource ReverseSound;
    public AudioSource Drift;
    public AudioSource Boost;
    //public AudioSource Offroad;
    //public AudioSource Crash;
    public AudioSource Horn;
    [Range(0.1f, 1.0f)] public float RunningSoundMaxVolume = 1.0f;
    [Range(0.1f, 2.0f)] public float RunningSoundMaxPitch = 1.0f;
    [Range(0.1f, 1.0f)] public float ReverseSoundMaxVolume = 0.5f;
    [Range(0.1f, 2.0f)] public float ReverseSoundMaxPitch = 0.6f;
    [Range(0.1f, 1.0f)] public float IdleSoundMaxVolume = 0.6f;

    [Range(0.1f, 1.0f)] public float DriftMaxVolume = 0.5f;

    public Player controller;

    private void Start()
    {
       // controller = player;
    }
    private void Update()
    {
      //  controller = player;
    }
    public override void Spawned()
    {
        base.Spawned();

        controller.OnSpinOutChanged += val => {
            if (!val) return;
            AudioManager.PlayAndFollow("slipSFX", transform, AudioManager.MixerTarget.SFX);
        };
        controller.OnHealthChanged += val => {
            //if (!val) return;
            if(controller.PrevHealth == val)
            {
                return;
            }
            Debug.Log("OnHealthChanged>>" + controller.PrevHealth + "->" + val);
            if(controller.PrevHealth > val)
            {
                Debug.Log("체력 감소>>");
                AudioManager.PlayAndFollow("HPReduce", transform, AudioManager.MixerTarget.SFX);
            }
            else
            {
                Debug.Log("체력 증가>>");
                AudioManager.PlayAndFollow("HPSFX", transform, AudioManager.MixerTarget.SFX);
            }
            controller.PrevHealth = val;
        };
        controller.OnDefenseChanged += val =>
        {
            if (controller.PrevDefense == val)
            {
                return;
            }

            Debug.Log("PlayerAudio OnDefenseChanged>>" + controller.PrevDefense + ">" + val);
            if (controller.PrevDefense < val)
            {
                Debug.Log("방어력 증가");
                AudioManager.PlayAndFollow("DefenseActive", transform, AudioManager.MixerTarget.SFX);
            }
            else
            {
                Debug.Log("방어력 감소");
                AudioManager.PlayAndFollow("DefenseReset", transform, AudioManager.MixerTarget.SFX);
            }
            controller.PrevDefense = val;
        };
        controller.OnArrestChanged += val =>
        {
            Debug.Log("PlayerAudio ArrestChanged>>");

            if(val == true)
            {
                Debug.Log("PlayerAudio ArrestOn>>");
                AudioManager.PlayAndFollow("Lock", transform, AudioManager.MixerTarget.SFX);
            }
            else
            {
                Debug.Log("PlayerAudio ArrestOff>>");
                AudioManager.PlayAndFollow("UnLock", transform, AudioManager.MixerTarget.SFX);
            }
        };
        controller.OnBoostTierIndexChanged += val => {
            if (val == 0) return;

            Boost.Play();
        };
    }

    public override void Render()
    {
        base.Render();

        //var rb = controller.Rigidbody;
        // var speed = rb.transform.InverseTransformVector(rb.velocity / controller.maxSpeedBoosting).z;
        var speed = controller.AppliedSpeed;
        Debug.Log("PlayerAudio Render rb speed>>" + speed);
        //HandleDriftAudio(speed);
        //HandleOffroadAudio(speed);
        HandleDriveAudio(speed);

        IdleSound.volume = Mathf.Lerp(IdleSoundMaxVolume, 0.0f, speed * 4);//0.6->0
    }

    private void HandleDriveAudio(float speed)
    {
        Debug.Log("HandleDriveAudio playerController AppliedSpeed" + speed);
        if (speed < 0.0f)
        {
            // In reverse
            RunningSound.volume = 0.0f;
            ReverseSound.volume = Mathf.Lerp(0.1f, ReverseSoundMaxVolume, -speed * 1.2f);//0.1->0.5
            Debug.Log("HandleDriveAudio ReverseSound Mathf.Lerp(0.1f, ReverseSoundMaxVolume, -speed * 1.2f)" + Mathf.Lerp(0.1f, ReverseSoundMaxVolume, -speed * 1.2f));
            ReverseSound.pitch = Mathf.Lerp(0.1f, ReverseSoundMaxPitch, -speed + (Mathf.Sin(Time.time) * .1f));//0.1->0.6
            Debug.Log("HandleDriveAudio ReverseSound Mathf.Lerp(0.1f, ReverseSoundMaxPitch, -speed + (Mathf.Sin(Time.time) * .1f))" + Mathf.Lerp(0.1f, ReverseSoundMaxPitch, -speed + (Mathf.Sin(Time.time) * .1f)));
        }
        else
        {
            // Moving forward
            ReverseSound.volume = 0.0f;
            RunningSound.volume = Mathf.Lerp(0.1f, RunningSoundMaxVolume, speed * 1.2f);//0.1->1.0
            Debug.Log("HandleDriveAudio RunningSound Mathf.Lerp(0.1f, RunningSoundMaxVolume, speed * 1.2f)" + Mathf.Lerp(0.1f, RunningSoundMaxVolume, speed * 1.2f));
            RunningSound.pitch = Mathf.Lerp(0.3f, RunningSoundMaxPitch, speed + (Mathf.Sin(Time.time) * .1f));
            Debug.Log("HandleDriveAudio RunningSound Mathf.Lerp(0.3f, RunningSoundMaxPitch, speed + (Mathf.Sin(Time.time) * .1f))" + Mathf.Lerp(0.3f, RunningSoundMaxPitch, speed + (Mathf.Sin(Time.time) * .1f)));
        }
    }

    /*private void HandleDriftAudio(float speed)
    {
        Debug.Log("HandDriftAudio" + speed);
        var b = controller.IsDrifting && controller.IsGrounded
            ? speed * DriftMaxVolume
            : 0.0f;
        Drift.volume = Mathf.Lerp(Drift.volume, b, Time.deltaTime * 20f);
    }*/

   /* private void HandleOffroadAudio(float speed)
    {
        Offroad.volume = Controller.IsOffroad
            ? Mathf.Lerp(0, 0.25f, Mathf.Abs(speed) * 1.2f)
            : Mathf.Lerp(Offroad.volume, 0, Time.deltaTime * 10f);
    }*/

    public void PlayHorn()
    {
        Horn.Play();
    }
}

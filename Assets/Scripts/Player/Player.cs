using Fusion;
using Fusion.Addons.KCC;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Player : PlayerComponent
{
    [SerializeField] private SkinnedMeshRenderer[] modelParts;
    [SerializeField] private KCC kcc;
    [SerializeField] private KCCProcessor glideProcessor;
    [SerializeField] private Transform camTarget;
    [SerializeField] private AudioSource source;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private Vector3 jumpImpulse = new(0f, 10f, 0f);
    [SerializeField] private float doubleJumpMultiplier = 0.75f;
    [SerializeField] private float grappleCD = 2f;
    [SerializeField] private float glideCD = 20f;
    [SerializeField] private float doubleJumpCD = 5f;
    [SerializeField] private float grappleStrength = 12f;
    [SerializeField] private float maxGlideTime = 2f;
    [field: SerializeField] public float AbilityRange { get; private set; } = 25f;

    public float GrappleCDFactor => (GrappleCD.RemainingTime(Runner) ?? 0f) / grappleCD;
    public float GlideCDFactor => (GlideCD.RemainingTime(Runner) ?? 0f) / glideCD;
    public float DoubleJumpCDFactor => (DoubleJumpCD.RemainingTime(Runner) ?? 0f) / doubleJumpCD;
    public double Score => Math.Round(transform.position.y, 1);
    public bool IsReady; // Server is the only one who cares about this
    public bool IsStartRequest;
    public bool IsArrive;
    private bool CanGlide => !kcc.Data.IsGrounded && GlideCharge > 0f;

    [Networked] public string Name { get; private set; }
    [Networked] public float GlideCharge { get; private set; }
    [Networked] public bool IsGliding { get; private set; }
    [Networked] private TickTimer GrappleCD { get; set; }
    [Networked] private TickTimer GlideCD { get; set; }
    [Networked] private TickTimer DoubleJumpCD { get; set; }
    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked, OnChangedRender(nameof(Jumped))] private int JumpSync { get; set; }

    public InputManager inputManager;
    private Vector2 baseLookRotation;
    private float glideDrain;

    public Animator anim;
    public Keyboard keyboard;

    //kart System(카트 무빙 인풋 시스템)
    public new SphereCollider collider;
    public DriftTier[] driftTiers;

    public float maxSpeedNormal;
    public float maxSpeedBoosting;
    public float reverseSpeed;
    public float acceleration;
    public float deceleration;

    [Tooltip("X-Axis: steering\nY-Axis: velocity\nCoordinate space is normalized")]
    public AnimationCurve steeringCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float maxSteerStrength = 35;
    public float steerAcceleration;
    public float steerDeceleration;
    public Vector2 driftInputRemap = new Vector2(0.5f, 1f);
    public float hopSteerStrength;
    public float speedToDrift;
    public float driftRotationLerpFactor = 10f;

    public Rigidbody Rigidbody;

    public bool IsBackfire => !BackfireTimer.ExpiredOrNotRunning(Runner);
    public bool IsHopping => !HopTimer.ExpiredOrNotRunning(Runner);
    public bool CanDrive => !IsSpinOut && !IsBackfire;
    public float BoostTime => BoostEndTick == -1 ? 0f : (BoostEndTick - Runner.Tick) * Runner.DeltaTime;
    public float RealSpeed => transform.InverseTransformDirection(Rigidbody.velocity).z;
    public bool IsDrifting => IsDriftingLeft || IsDriftingRight;
    public bool IsBoosting => BoostTierIndex != 0;
    public float DriftTime => (Runner.Tick - DriftStartTick) * Runner.DeltaTime;

    [Networked] public float MaxSpeed { get; set; }
    [Networked]
    public int BoostTierIndex { get; set; }
    [Networked] public TickTimer BoostpadCooldown{get;set;}
    [Networked] public int DriftTierIndex { get; set; } = -1;

    [Networked] public NetworkBool IsGrounded { get; set; }
    [Networked] public int BoostEndTick { get; set; } = -1;

    [Networked]
    public NetworkBool IsSpinOut { get; set; }
    [Networked] public NetworkBool IsDriftingLeft { get; set; }
    [Networked] public NetworkBool IsDriftingRight { get; set; }
    [Networked] public int DriftStartTick { get; set; }
    [Networked]
    public TickTimer BackfireTimer { get; set; }
    [Networked]
    public TickTimer HopTimer { get; set; }
    [Networked] public float AppliedSpeed { get; set; } = 0;

    //[Networked] private KartInput.NetworkInputData kartInputs { get; set; }

    public event Action<int> OnDriftTierIndexChanged;
    public event Action<int> OnBoostTierIndexChanged;
    public event Action<bool> OnSpinOutChanged;
    public event Action<bool> OnHopChanged;
    public event Action<bool> OnBackfireChanged;

    [Networked] private float SteerAmount { get; set; }
    [Networked] private int AcceleratePressedTick { get; set; }
    [Networked] private bool IsAccelerateThisFrame { get; set; }
    private ChangeDetector _changeDetector;

    private static void OnIsBackfireChangedCallback(Player changed) =>
        changed.OnBackfireChanged?.Invoke(changed.IsBackfire);
    
    private static void OnIsHopChangedCallback(Player changed) =>
        changed.OnHopChanged?.Invoke(changed.IsHopping);

    private static void OnSpinoutChangedCallback(Player changed) =>
        changed.OnSpinOutChanged?.Invoke(changed.IsSpinOut);

    private static void OnDriftTierIndexChangedCallback(Player changed) =>
        changed.OnDriftTierIndexChanged?.Invoke(changed.DriftTierIndex);

    private static void OnBoostTierIndexChangedCallback(Player changed) =>
        changed.OnBoostTierIndexChanged?.Invoke(changed.BoostTierIndex);

    private void Awake()
    {
        collider = GetComponent<SphereCollider>();
    }

    public override void Spawned()
    {
        //kart system
        base.Spawned();
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        MaxSpeed = maxSpeedNormal;

        glideDrain = 1f / (maxGlideTime * Runner.TickRate);
        GlideCharge = 1f;

        inputManager = Runner.GetComponent<InputManager>();
        keyboard = inputManager.keyboard;

        if (HasInputAuthority)
        {
           /* foreach (SkinnedMeshRenderer renderer in modelParts)
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;*/

            inputManager.LocalPlayer = this;
            Name = PlayerPrefs.GetString("Photon.Menu.Username");
            RPC_PlayerName(Name);
            CameraFollow.Singleton.SetTarget(camTarget);
            UIManager.Singleton.LocalPlayer = this;
            kcc.Settings.ForcePredictedLookRotation = true;
        }
    }
    private void Update()
    {
        GroundNormalRotation();//바닥 착지 여부
        //UpdateTireRotation();
    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

     

        if (GetInput(out NetInput input))
        {
            if (input.Buttons.IsSet(InputButton.W) || input.Buttons.IsSet(InputButton.S)
                || input.Buttons.IsSet(InputButton.A) || input.Buttons.IsSet(InputButton.D))
            {
                Debug.Log("Player 이동하고있는경우>>");
                anim.SetBool("IsRunning", true);
            }
            else if(!input.Buttons.IsSet(InputButton.W) && !input.Buttons.IsSet(InputButton.S)
                && !input.Buttons.IsSet(InputButton.A) && !input.Buttons.IsSet(InputButton.D))
            {
                Debug.Log("Player 멈춰있는경우>>");
                anim.SetBool("IsRunning", false);
            }

            Move(input);//AppliedSpeed관련 적용(수치)
            SpinOut(input);
            Boost(input);
            UseItems(input);

            /*if(GetInput(out KartInput.NetworkInputData input_))
            {
                //Get Kart Inputs
                Debug.Log("GetInput Network KartInput>>"+ input_);
              
                kartInputs = input_;
       
                //if (CanDrive)
                Move(kartInputs);//AppliedSpeed관련 적용(수치)
                *//* else
                     RefreshAppliedSpeed();//속도0초기화*//*

                //HandleStartRace();
                SpinOut(kartInputs);
                Boost(kartInputs);
                Drift(kartInputs);
                Steer(kartInputs);
                //UpdateTireYaw(Inputs);
                UseItems(kartInputs);
            }*/

            CheckGlide(input);
            CheckJump(input);
            kcc.AddLookRotation(input.LookDelta * lookSensitivity, -maxPitch, maxPitch);
            UpdateCamTarget();

            if (input.Buttons.WasPressed(PreviousButtons, InputButton.Grapple))
                TryGrapple(camTarget.forward);

            if (IsGliding && !CanGlide)
                ToggleGlide(false);

            SetInputDirection(input);
            PreviousButtons = input.Buttons;
            baseLookRotation = kcc.GetLookRotation();

            if (kcc.FixedData.IsGrounded)
            {
                //Debug.Log("Player개체 바닥착지>>");
                anim.SetBool("IsGround", true);
            }
            else
            {
               // Debug.Log("Player개체 공중에 있는경우>>");
                anim.SetBool("IsGround", false);
            }
        }
    }

    public override void Render()
    {
        foreach(var change in _changeDetector.DetectChanges(this))
        {
            switch (change) 
            {
                case nameof(BoostTierIndex):
                {
                    Debug.Log("BoostTierIndex Changed>>");
                    OnBoostTierIndexChangedCallback(this);
                    break;
                }
                case nameof(DriftTierIndex):
                {
                    Debug.Log("DriftTierIndex Changed>>");
                    OnDriftTierIndexChangedCallback(this);
                    break;
                }
                case nameof(IsSpinOut):
                {
                    Debug.Log("IsSpinOut Changed>>");
                    OnSpinoutChangedCallback(this);
                    break;
                }
                case nameof(BackfireTimer):
                {
                    Debug.Log("IsSpinOut Changed>>");

                    OnIsBackfireChangedCallback(this);
                    break;
                }
                case nameof(HopTimer):
                {
                    Debug.Log("HopTimer Changed>>");

                    OnIsHopChangedCallback(this);
                    break;
                }
            }
        }
        if (kcc.Settings.ForcePredictedLookRotation)
        {
            Vector2 predictedLookRotation = baseLookRotation + inputManager.AccumulatedMouseDelta * lookSensitivity;
            kcc.SetLookRotation(predictedLookRotation);
        }

        UpdateCamTarget();
    }
    private void UseItems(NetInput inputs)
    {
        /*if (inputs.IsDownThisFrame(KartInput.NetworkInputData.UseItem))
        {
            playerentity.Items.UseItem();
        }*/
        if (inputs.Buttons.IsSet(InputButton.F))
        {
            Debug.Log("Player Input UseItems 아이템 사용>>");
            playerentity.Items.UseItem();
        }
    }
    private void SpinOut(NetInput inputs)
    {
        // var isAccelerate = inputs.IsDown(KartInput.NetworkInputData.ButtonAccelerate);
        var isAccelerate = inputs.Buttons.IsSet(InputButton.W);

        if (isAccelerate && !IsAccelerateThisFrame)
        {
            AcceleratePressedTick = Runner.Tick;
        }

        if (AcceleratePressedTick != -1 && !isAccelerate)
        {
            AcceleratePressedTick = -1;
        }

        IsAccelerateThisFrame = isAccelerate;
    }
    private void Move(NetInput inputs)
    {
        Debug.Log("Player Move Input>>");
        if (inputs.Buttons.IsSet(InputButton.W))
        {
            Debug.Log("IsAccelerate");
            AppliedSpeed = Mathf.Lerp(AppliedSpeed, MaxSpeed, acceleration * Runner.DeltaTime);
        }
        else if (inputs.Buttons.IsSet(InputButton.S))
        {
            Debug.Log("IsReverse");
            AppliedSpeed = Mathf.Lerp(AppliedSpeed, -reverseSpeed, acceleration * Runner.DeltaTime);
        }
        else
        {
            Debug.Log("Idle");
            AppliedSpeed = Mathf.Lerp(AppliedSpeed, 0, deceleration * Runner.DeltaTime);
        }

       // var vel = (Rigidbody.rotation * Vector3.forward) * AppliedSpeed;
        //vel.y = Rigidbody.velocity.y;
        //Rigidbody.velocity = vel;
    }
    /*private void Steer(KartInput.NetworkInputData input)
    {
        var steerTarget = GetSteerTarget(input);

        if(SteerAmount != steerTarget)
        {
            var steerLerp = Mathf.Abs(SteerAmount) < Mathf.Abs(steerTarget) ? steerAcceleration : steerDeceleration;
            SteerAmount = Mathf.Lerp(SteerAmount, steerTarget, Runner.DeltaTime * steerLerp);
        }

        if(IsDrifting)

         {
             model.localEulerAngles = LerpAxis(Axis.Y, model.localEulerAngles, SteerAmount * 2,
                 driftRotationLerpFactor * Runner.DeltaTime);
         }

         else
         {
             model.localEulerAngles = LerpAxis(Axis.Y, model.localEulerAngles, 0, 6 * Runner.DeltaTime);
         }

         if (CanDrive)
         {
             var rot = Quaternion.Euler(
                 Vector3.Lerp(
                     Rigidbody.rotation.eulerAngles,
                     Rigidbody.rotation.eulerAngles + Vector3.up * SteerAmount,
                     3 * Runner.DeltaTime)
             );

             Rigidbody.MoveRotation(rot);
         }
    }
    private float GetSteerTarget(KartInput.NetworkInputData input)
    {
        var steerFactor = steeringCurve.Evaluate(Mathf.Abs(RealSpeed) / maxSpeedNormal) * maxSteerStrength *
             Mathf.Sign(RealSpeed);

        if (IsHopping && RealSpeed < speedToDrift)
            return input.Steer * hopSteerStrength;

        if (IsDriftingLeft)
            return Remap(input.Steer, -1, 1, -driftInputRemap.y, -driftInputRemap.x) * maxSteerStrength;
        if (IsDriftingRight)
            return Remap(input.Steer, -1, 1, driftInputRemap.x, driftInputRemap.y) * maxSteerStrength;

        return input.Steer * steerFactor;
    }
    private void Drift(KartInput.NetworkInputData input)
    {
        var startDrift = input.IsDriftPressedThisFrame && CanDrive && !IsDrifting;
        if (startDrift && IsGrounded)
        {
            StartDrifting(input);
            DriftStartTick = Runner.Tick;
            HopTimer = TickTimer.CreateFromSeconds(Runner, 0.367f);
        }

        if (IsDrifting)
        {
            if (!input.IsDriftPressed || RealSpeed < speedToDrift)
            {
                StopDrifting();
            }
            else if (IsGrounded)
            {
                EvaluateDrift(DriftTime, out var index);
                if (DriftTierIndex != index) DriftTierIndex = index;
            }
        }
    }*/
    private void Boost(NetInput input)
    {
        if (BoostTime > 0)
        {
            MaxSpeed = maxSpeedBoosting;
            AppliedSpeed = Mathf.Lerp(AppliedSpeed, MaxSpeed, Runner.DeltaTime);

            Debug.Log("PlayerInput Boost 적용>>"+ AppliedSpeed);
        }
        else if (BoostEndTick != -1)
        {
            StopBoosting();
        }
    }
    private void GroundNormalRotation()
    {
        IsGrounded = Physics.SphereCast(collider.transform.TransformPoint(collider.center), collider.radius - 0.1f,
            Vector3.down, out var hit, 0.3f, ~LayerMask.GetMask("Player"));

        if (IsGrounded)
        {
            Debug.DrawRay(hit.point, hit.normal, Color.magenta);
            //GroundResistance = hit.collider.material.dynamicFriction;

           /* model.transform.rotation = Quaternion.Lerp(
                model.transform.rotation,
                Quaternion.FromToRotation(model.transform.up * 2, hit.normal) * model.transform.rotation,
                7.5f * Time.deltaTime);*/
        }
    }
    /*private void StartDrifting(KartInput.NetworkInputData input)
    {
        if(AppliedSpeed < speedToDrift || input.Steer == 0)
        {
            StopDrifting();
            return;
        }

        IsDriftingRight = input.Steer > 0f;
        IsDriftingLeft = input.Steer < 0f;
    }
    private void StopDrifting()
    {
        BoostTierIndex = DriftTierIndex == -1 ? 0 : DriftTierIndex;
        BoostEndTick = BoostTierIndex == 0
            ? -1
            : Runner.Tick +
            (int)(driftTiers[BoostTierIndex].boostDuration / Runner.DeltaTime);

        if (BoostTime <= 0) StopBoosting();

        DriftStartTick = -1;
        DriftTierIndex = -1;
        IsDriftingLeft = false;
        IsDriftingRight = false;
    }*/
    private void StopBoosting()
    {
        BoostTierIndex = 0;
        BoostEndTick = -1;
        MaxSpeed = maxSpeedNormal;
    }
    public void GiveBoost(bool isBoostpad,int tier = 1)
    {
        if (isBoostpad)
        {
            if (!BoostpadCooldown.ExpiredOrNotRunning(Runner))
                return;

            BoostpadCooldown = TickTimer.CreateFromSeconds(Runner, 4f);
        }

        BoostTierIndex = BoostTierIndex > tier ? BoostTierIndex : tier;
        Debug.Log("GiveBoost BoostTierIndex>>" + BoostTierIndex);
        if (BoostEndTick == -1) BoostEndTick = Runner.Tick;
        BoostEndTick += (int)(driftTiers[tier].boostDuration / Runner.DeltaTime);
    }
    public void RefreshAppliedSpeed()
    {
        //AppliedSpeed = transform.InverseTransformDirection(kcc.Snap
        AppliedSpeed = 1;
    }
    private static Vector3 LerpAxis(Axis axis,Vector3 euler,float tgtVal,float t)
    {
        if (axis == Axis.X) return new Vector3(Mathf.LerpAngle(euler.x, tgtVal, t), euler.y, euler.z);
        if (axis == Axis.Y) return new Vector3(euler.x, Mathf.LerpAngle(euler.y, tgtVal, t), euler.z);
        return new Vector3(euler.x, euler.y, Mathf.LerpAngle(euler.z, tgtVal, t));
    }
    private static float Remap(float value, float srcMin, float srcMax, float destMin, float destMax, bool clamp = false)
    {
        if (clamp) value = Mathf.Clamp(value, srcMin, srcMax);
        return (value - srcMin) / (srcMax - srcMin) * (destMax - destMin) + destMin;
    }
    public DriftTier EvaluateDrift(float driftDuration,out int index)
    {
        var i = 0;
        var tier = driftTiers[0];
        while(i < driftTiers.Length)
        {
            if(driftDuration < tier.startTime)
            {
                tier = driftTiers[--i];
                break;
            }

            if (i < driftTiers.Length - 1)
                tier = driftTiers[++i];
            else
                break;
        }

        index = i;
        return tier;
    }
    public void ResetState()
    {
        Rigidbody.velocity = Vector3.zero;
        AppliedSpeed = 0;
        BoostEndTick = -1;
        BoostTierIndex = 0;
        transform.up = Vector3.up;
        //model.transform.up = Vector3.up;
    }
    public enum Axis
    {
        X,
        Y,
        Z
    }
    [Serializable]
    public struct DriftTier
    {
        public Color color;
        public float boostDuration;
        public float startTime;
    }
    private void CheckGlide(NetInput input)
    {
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Glide) && GlideCD.ExpiredOrNotRunning(Runner) && CanGlide)
            ToggleGlide(true);
        else if (input.Buttons.WasReleased(PreviousButtons, InputButton.Glide) && IsGliding)
            ToggleGlide(false);
    }

    private void CheckJump(NetInput input)
    {
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Jump))
        {
            if (kcc.FixedData.IsGrounded)
            {
                kcc.Jump(jumpImpulse);
                JumpSync++;
                anim.SetBool("Jumping",true);

                Invoke(nameof(JumpBooleanStatus), 0.6f);
            }
            else if (DoubleJumpCD.ExpiredOrNotRunning(Runner))
            {
                kcc.Jump(jumpImpulse * doubleJumpMultiplier);
                DoubleJumpCD = TickTimer.CreateFromSeconds(Runner, doubleJumpCD);
                ToggleGlide(false);
                JumpSync++;
                anim.SetBool("Jumping", true);

                Invoke(nameof(JumpBooleanStatus), 0.6f);
            }
        }
    }
    private void JumpBooleanStatus()
    {
        anim.SetBool("Jumping", false);
    }

    private void SetInputDirection(NetInput input)
    {
        Vector3 worldDirection;
        if (IsGliding)
        {
            GlideCharge = Mathf.Max(0f, GlideCharge - glideDrain);
            worldDirection = kcc.Data.TransformDirection;
        }
        else
            worldDirection = kcc.FixedData.TransformRotation * input.Direction.X0Y();

        Debug.Log("Player SetInputDirection>>" + worldDirection * AppliedSpeed);
        kcc.SetInputDirection(worldDirection * AppliedSpeed);
    }

    private void UpdateCamTarget()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        IsReady = true;
        if (HasInputAuthority)
            UIManager.Singleton.DidSetReady();
    }
    [Rpc(RpcSources.InputAuthority,RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
    public void RPC_SetStart()
    {
        Debug.Log("Q키 요청>>");
        IsStartRequest = true;
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        kcc.SetPosition(position);
        kcc.SetLookRotation(rotation);
    }

    public void ResetCooldowns()
    {
        GrappleCD = TickTimer.None;
        GlideCD = TickTimer.None;
        DoubleJumpCD = TickTimer.None;
    }

    private void TryGrapple(Vector3 lookDirection)
    {
        if (GrappleCD.ExpiredOrNotRunning(Runner) && Physics.Raycast(camTarget.position, lookDirection, out RaycastHit hitInfo, AbilityRange))
        {
            if (hitInfo.collider.TryGetComponent(out Block _))
            {
                GrappleCD = TickTimer.CreateFromSeconds(Runner, grappleCD);
                Vector3 grappleVector = Vector3.Normalize(hitInfo.point - transform.position);
                if (grappleVector.y > 0f)
                    grappleVector = Vector3.Normalize(grappleVector + Vector3.up);

                kcc.Jump(grappleVector * grappleStrength);
                ToggleGlide(false);
            }
        }
    }

    private void ToggleGlide(bool isGliding)
    {
        if (IsGliding == isGliding)
            return;

        if (isGliding)
        {
            kcc.AddModifier(glideProcessor);
            Vector3 velocity = kcc.Data.DynamicVelocity;
            velocity.y *= 0.25f;
            kcc.SetDynamicVelocity(velocity);
        }
        else
        {
            kcc.RemoveModifier(glideProcessor);
            GlideCharge = 1f;
            GlideCD = TickTimer.CreateFromSeconds(Runner, glideCD);
        }

        IsGliding = isGliding;
    }

    private void Jumped()
    {
        source.Play();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerName(string name)
    {
        Name = name;
    }
}

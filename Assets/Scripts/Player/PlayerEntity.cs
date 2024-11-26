using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Fusion;
//using Fusion.Addons.Physics;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerEntity : PlayerComponent
{
    //public static event Action<PlayerEntity> OnKartSpawned;
    //public static event Action<PlayerEntity> OnKartDespawned;

    public event Action<int> OnHeldItemChanged;
    public event Action<int> OnCoinCountChanged;

    public PlayerAnimator Animator { get; private set; }
    //public KartCamera Camera { get; private set; }
    public Player Controller { get; private set; }
    public InputManager Input { get; private set; }
   // public KartLapController LapController { get; private set; }
    public PlayerAudio Audio { get; private set; }
    public GameUI Hud { get; private set; }
    public PlayerItemController Items { get; private set; }
    //public NetworkRigidbody3D Rigidbody { get; private set; }

    public Powerup HeldItem =>
        HeldItemIndex == -1
            ? null
            : ResourceManager.Instance.powerups[HeldItemIndex];

    [Networked]
    public int HeldItemIndex { get; set; } = -1;

    [Networked]
    public int CoinCount { get; set; }

    public Transform itemDropNode;

    private bool _despawned;

    private ChangeDetector _changeDetector;


    private static void OnHeldItemIndexChangedCallback(PlayerEntity changed)
    {
        changed.OnHeldItemChanged?.Invoke(changed.HeldItemIndex);

        if (changed.HeldItemIndex != -1)
        {
            foreach (var behaviour in changed.GetComponentsInChildren<PlayerComponent>())
                behaviour.OnEquipItem(changed.HeldItem, 3f);
        }
    }

    private static void OnCoinCountChangedCallback(PlayerEntity changed)
    {
        changed.OnCoinCountChanged?.Invoke(changed.CoinCount);
    }
   

    private void Awake()
    {
        // Set references before initializing all components
        Animator = GetComponentInChildren<PlayerAnimator>();
       // Camera = GetComponent<KartCamera>();
        Controller = GetComponent<Player>();
        //Input = Runner.GetComponent<InputManager>();
        //LapController = GetComponent<KartLapController>();
        Audio = GetComponentInChildren<PlayerAudio>();
        Items = GetComponent<PlayerItemController>();
        //Rigidbody = GetComponent<NetworkRigidbody3D>();

        // Initializes all KartComponents on or under the Kart prefab
        var components = GetComponentsInChildren<PlayerComponent>();
        foreach (var component in components) component.Init(this);
    }

    public static readonly List<PlayerEntity> playerentities = new List<PlayerEntity>();

    public override void Spawned()
    {
        Debug.Log("PlayerEntity Spawned>>");
        base.Spawned();

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasInputAuthority)
        {
            // Create HUD
            Hud = Instantiate(ResourceManager.Instance.hudPrefab);
            Hud.Init(this);

            //Instantiate(ResourceManager.Instance.nicknameCanvasPrefab);
        }

        playerentities.Add(this);
        //OnKartSpawned?.Invoke(this);
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(HeldItemIndex):
                    OnHeldItemIndexChangedCallback(this);
                    break;
                case nameof(CoinCount):
                    OnCoinCountChangedCallback(this);
                    break;      
            }
        }
    }
   
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        playerentities.Remove(this);
        _despawned = true;
       // OnKartDespawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        playerentities.Remove(this);
        /*if (!_despawned)
        {
            OnKartDespawned?.Invoke(this);
        }*/
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ICollidable collidable))
        {
            Debug.Log("PlayerEntity Collide>>");
            collidable.Collide(this);
        }
    }

    public bool SetHeldItem(int index)
    {
        if (HeldItem != null) return false;

        HeldItemIndex = index;
        return true;
    }

    public void SpinOut()
    {
        Controller.IsSpinOut = true;
        StartCoroutine(OnSpinOut());
    }

    private IEnumerator OnSpinOut()
    {
        yield return new WaitForSeconds(4f);

        Controller.IsSpinOut = false;
    }

    public void DefenseUp(float DefensePower)
    {
        Controller.Defense = DefensePower;

        StartCoroutine(OnDefenseOut());
    }
    private IEnumerator OnDefenseOut()
    {
        yield return new WaitForSeconds(6f);

        Controller.Defense = 0;
    }

    public void ArrestUp(float ArrestTime)
    {
        Controller.IsArrested = true;

        StartCoroutine(OnArrestOut(ArrestTime));
    }
    private IEnumerator OnArrestOut(float ArrestTime)
    {
        yield return new WaitForSeconds(ArrestTime);

        Controller.IsArrested = false;
    }
}
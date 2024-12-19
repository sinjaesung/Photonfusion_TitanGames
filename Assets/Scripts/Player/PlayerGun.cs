using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using static UnityEngine.EventSystems.PointerEventData;

public class PlayerGun : NetworkBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform CharacterPivot;

    [Header("Fire Setup")]
    public LayerMask HitMask;
    public GameObject ImpactPrefab;
    public ParticleSystem MuzzleParticle;

    [Header("Sounds")]
    public AudioSource FireSound;

    [Networked]
    private Vector3 _hitPosition { get; set; }
    [Networked]
    private Vector3 _hitNormal { get; set; }
    [Networked]
    private int _fireCount { get; set; }

    //Animation IDs
    private int _animIDShoot;

    private int _visibleFireCount;
    [Networked] private NetworkButtons PreviousButtons { get; set; }

    [SerializeField] private float damage = 3;

    public CameraFollow camerafollow;
    public override void Spawned()
    {
        //Reset visible fire count
        _visibleFireCount = _fireCount;
    }

    public override void Render()
    {
        ShowFireEffects();

        camerafollow = FindObjectOfType<CameraFollow>();
    }

    private void Awake()
    {
        AssignAnimationIDs();
    }

    public override void FixedUpdateNetwork()
    {
        // new Vector3(transform.eulerAngles.x, currentPan, transform.eulerAngles.z)
        //var Dir = new Vector3(transform.eulerAngles.x, mainCam.currentPan, transform.eulerAngles.z);
        //Debug.Log("Player Forward Direction>>" + Dir);
        base.FixedUpdateNetwork();
     
        if (GetInput(out NetInput input))
        {
            if (input.Buttons.WasPressed(PreviousButtons, InputButton.Ctrl))
                Fire();

            PreviousButtons = input.Buttons;

        }
    }

    private void Fire()
    {
        //Clear hit position in case nothing will be hit
        _hitPosition = Vector3.zero;

        var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;

        //Whole projectile path and effects are immediately processed(= hitscan projectile)
        /* if(Runner.LagCompensation.Raycast(CharacterPivot.position,CharacterPivot.forward,30f,
             Object.InputAuthority,out var hit,HitMask,hitOptions,QueryTriggerInteraction.Ignore) == true)
         {
             Debug.Log("Target hit hitBox" + hit + "," + hit.Hitbox);
             //Deal damage
             var health = hit.Hitbox != null ? hit.Hitbox.Root.GetComponent<NetworkHealth>() : null;
             if(health != null && health.TakeHit(damage))
             {
                 Debug.Log("PlayerGun 대상 타깃>>" + health.transform.name + ">Damage:" + damage);
             }

             //Deal 플레이간 데미지처리
             Player healthCom = hit.Hitbox != null ? hit.Hitbox.Root.GetComponent<Player>() : null;
             if(healthCom != null)
             {
                 float takeDamage = damage - healthCom.Defense;
                 Debug.Log("PlayerGun 대상 타깃>>" + healthCom.transform.name + ">Damage:" + takeDamage);
                 if(takeDamage <= 0)
                 {
                     takeDamage = 0;
                 }
                 healthCom.UpdateHealth(takeDamage);
             }
             // Save hit point to correctly show bullet path on all clients.
             // This however works only for single projectile per FUN and with higher fire cadence
             // some projectiles might not be fired on proxies because we save only the position
             // of the LAST hit.

             _hitPosition = hit.Point;
             _hitNormal = hit.Normal;
             Debug.Log("PlayerGun hitPosition,hitNormal>>" + _hitPosition + "," + _hitNormal);
         }*/
        if (GetInput(out NetInput input))
        {
            // Check if the mouse is being moved or input detected
            if (Input.mousePosition != null && camerafollow && camerafollow.mainCam)
            {
                // Get the mouse position in screen coordinates
                Vector3 mouseScreenPosition = Input.mousePosition;

                // Create a ray from the camera to the mouse position
                Ray ray = camerafollow.mainCam.ScreenPointToRay(mouseScreenPosition);
                              
                 // Raycast hit information
                 RaycastHit mouseWorldhit_;

                // Perform the raycast to check for a collision with objects in the scene
                if (Physics.Raycast(ray, out mouseWorldhit_,999f,HitMask))
                {
                    // Log the point where the ray intersects a collider
                    Debug.Log("PlayerGun Mouse World Position (hit): " + mouseWorldhit_.point);

                    RaycastHit hit;
                    if (Physics.Raycast(CharacterPivot.position, (mouseWorldhit_.point - CharacterPivot.position), out hit, 30f, HitMask))
                    {
                        Debug.Log("Target hit>>" + hit.transform.name);
                        var health = hit.transform.GetComponent<NetworkHealth>();
                        if (health != null && health.TakeHit(damage))
                        {
                            Debug.Log("PlayerGun 대상 타깃>>" + health.transform.name + ">Damage:" + damage);
                        }

                        //Deal 플레이어간 데미지 처리
                        Player healthCom = hit.transform.GetComponent<Player>();
                        if (healthCom != null)
                        {
                            float takeDamage = damage - healthCom.Defense;
                            Debug.Log("PlayerGun 대상 타깃>>" + healthCom.transform.name + ">Damage:" + takeDamage);
                            if (takeDamage <= 0)
                            {
                                takeDamage = 0;
                            }
                            healthCom.UpdateHealth(takeDamage);
                        }

                        _hitPosition = hit.point;
                        _hitNormal = hit.normal;
                        Debug.Log("PlayerGun hitPosition,hitNormal>>" + _hitPosition + "," + _hitNormal);
                    }
                }
                else
                {
                    // If no object is hit, calculate world position on a flat plane
                    Vector3 worldPosition = ray.GetPoint(9999);
                    Debug.Log("Mouse World Position (no hit): " + worldPosition);
                }              
            }
        }
           

        // In this example projectile count property (fire count) is used not only for weapon fire effects
        // but to spawn the projectile visuals themselves.
        _fireCount++;
    }
    
    private void ShowFireEffects()
    {
        // Notice we are not using OnChangedRender for fireCount property but instead
        // we are checking against a local variable and show fire effects only when visible
        // fire count is SMALLER. This prevents triggering false fire effects when
        // local player mispredicted fire (e.g. input got lost) and fireCount property got decreased.
        if(_visibleFireCount < _fireCount)
        {
            FireSound.PlayOneShot(FireSound.clip);
            MuzzleParticle.Play();
            animator.SetTrigger(_animIDShoot);

            if(_hitPosition != Vector3.zero)
            {
                //Impact gets destroyed automatically with DestroyAfter script
                Instantiate(ImpactPrefab.transform, _hitPosition, Quaternion.LookRotation(_hitNormal));
            }
        }

        _visibleFireCount = _fireCount;
    }

    private void AssignAnimationIDs()
    {
        _animIDShoot = Animator.StringToHash("Shoot");
    }
}

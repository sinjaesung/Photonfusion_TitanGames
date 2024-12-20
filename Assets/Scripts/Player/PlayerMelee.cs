using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMelee : NetworkBehaviour
{
    [SerializeField] public float attackDamage;
    [SerializeField] private Transform playerController;
    [SerializeField] private PlayerGun playergun;

    public GameObject ImpactPrefab;
    void Start()
    {
    }
  
    private void OnEnable()
    {
        Debug.Log("PlayerMelee OnEnable:");
    }
    private void OnDisable()
    {
        Debug.Log("PlayerMelee OnDisable:");
        StopAllCoroutines();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other != null && playergun.IsAttackTime)
        { 
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("PlayerMelee damage>>" + other.transform.name + ">" + attackDamage);
                //other.GetComponent<PlayerController>().TakeDamage(damage);
                //playerController.TakeDamage(damage);

                var health = other.GetComponent<NetworkHealth>();
                if (health != null)
                {
                    health.TakeHit(attackDamage);
                    Instantiate(ImpactPrefab.transform, other.transform.position, Quaternion.identity);
                }
            }
            else if (other.CompareTag("Player"))
            {
                if (other.transform != playerController)
                {
                    Player healthCom = other.GetComponent<Player>();
                    if (healthCom != null)
                    {
                        float takeDamage = attackDamage - healthCom.Defense;
                        Debug.Log("PlayerMelee damage>>" + other.transform.name + ">:" + attackDamage);
                        if (takeDamage <= 0)
                        {
                            takeDamage = 0;
                        }
                        healthCom.UpdateHealth(takeDamage);
                        Instantiate(ImpactPrefab.transform, other.transform.position, Quaternion.identity);
                    }
                }
                else
                {
                    Debug.Log("PlayerMelee 타깃이 자기자신인 경우는 제외>>");
                }
            }
        }
    }
}
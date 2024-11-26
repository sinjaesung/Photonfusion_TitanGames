using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;

public class IEnemyMeleeCollider : MonoBehaviour
{
    [SerializeField] public float attackDamage;
    private Player playerController;
    //private CharacterStats characterstats;
    private float playerdefense;
    [SerializeField] public IEnemyFSM referMother;
   // private float playeraidefense;
   // private PlayerAI playerai;

    public AudioSource audioSource;

    void Start()
    {
        Debug.Log("IEnemyMeleeCollider origin Start:");
        playerController = FindObjectOfType<Player>();
        //characterstats = playerController.characterStats;
        playerdefense = playerController.Defense;
       // StartCoroutine(CalcFineStats());

        if (GetComponent<AudioSource>() != null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    /*private IEnumerator CalcFineStats()
    {
        yield return new WaitForSeconds(0.1f);

        characterstats = playerController.characterStats;

        while (true)
        {
            playerdefense = characterstats.CurrentDefense;
            //Debug.Log("IEnemyMeleeCollider find playerdefnse:" + playerdefense);
            if (playerai != null)
            {
                playeraidefense = playerai.MotherDefense;
            }
            yield return null;
        }
    }*/

    private void OnEnable()
    {
        Debug.Log("IEnemyMeleeCollider OnEnable:");
        playerController = FindObjectOfType<Player>();
        //characterstats = playerController.characterStats;
        playerdefense = playerController.CurrentDefense;
       /* playerai = FindObjectOfType<PlayerAI>();
        if (playerai != null)
        {
            playeraidefense = playerai.MotherDefense;
        }
        StartCoroutine(CalcFineStats());*/
    }
    private void OnDisable()
    {
        Debug.Log("IEnemyMeleeCollider OnDisable:");
        StopAllCoroutines();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.CompareTag("Player") && referMother.attacking == true)
            {
                Debug.Log("IEnemyMeleeCollider damage" + other.transform.name + "," + transform + "monsterdamage:"
                    + attackDamage + ",플레이어방어력:" + playerdefense + ",적용데미지:" + (attackDamage - playerdefense));
                //other.GetComponent<PlayerController>().TakeDamage(damage);
                //playerController.TakeDamage(damage);
                Player healthCom = playerController.GetComponent<Player>();
                float takeDamage = attackDamage - playerdefense;
                if (takeDamage <= 0)
                {
                    takeDamage = 0;
                }
                Debug.Log("IEnemyMeleeCollider 최종적용데미지:" + takeDamage);
                healthCom.UpdateHealth(takeDamage);
                if (audioSource != null)
                {
                    audioSource.Play();
                }
            }
           /* else if (other.CompareTag("PlayerAI") && referMother.attacking == true)
            {
                Debug.Log("IEnemyMeleeCollider damage" + other.transform.name + "," + transform + "monsterdamage:" +
                    attackDamage + ",플레이어ai방어력:" + playeraidefense + ",적용데미지:" + (attackDamage - playeraidefense));
                //other.GetComponent<PlayerController>().TakeDamage(damage);
                //playerController.TakeDamage(damage);
                PlayerAI playeraiCom = other.GetComponent<PlayerAI>();
                float takeDamage = attackDamage - playeraidefense;
                if (takeDamage <= 0)
                {
                    takeDamage = 0;
                }
                Debug.Log("IEnemyMeleeCollider 최종적용데미지:" + takeDamage);
                playeraiCom.PlayerAIHitDamage(takeDamage);
                if (audioSource != null)
                {
                    audioSource.Play();
                }
            }
            else if (other.CompareTag("InteractionObject") && referMother.attacking == true)
            {
                Debug.Log("IEnemyMeleeCollider" + other.transform.name + "," + transform + "monsterdamage:" + attackDamage + ",플레이어방어력:" + playerdefense + ",적용데미지:" + (attackDamage - playerdefense));
                other.GetComponent<InteractionObject>().TakeDamage(attackDamage);
                if (audioSource != null)
                {
                    audioSource.Play();
                }
            }*/
        }
    }
}
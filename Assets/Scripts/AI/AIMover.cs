using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiMover : MonoBehaviour
{
    EnemyFSM EnemyFSM;
    NavMeshAgent navmeshagent;
    [SerializeField] public Transform spawnPoint;

    private void Start()
    {
        EnemyFSM = GetComponent<EnemyFSM>();
        navmeshagent = EnemyFSM.navMeshAgent;
    }

    private void Update()
    {
        UpdateAnim();
    }

    void UpdateAnim()
    {
        //Vector3 velocity = GetComponent<NavMeshAgent>().velocity;
        if (EnemyFSM != null && navmeshagent != null)
        {
            Vector3 velocity = navmeshagent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            //Debug.Log("몬스터navMesh의 속력:" + speed);
            if (GetComponent<Animator>() != null)
            {
                GetComponent<Animator>().SetFloat("ForwardSpeed", speed);
            }
        }
    }
    public void ChaseTarget()
    {
        Vector3 Target = EnemyFSM.Target.transform.position;
        Debug.Log("목표 타깃을 발견(playerTag gameObject)하여 타깃을 쫓는다" + EnemyFSM.Target.transform.name);

        // GetComponent<NavMeshAgent>().destination = Target;
        navmeshagent.SetDestination(Target);
    }
    public void StopChaseTarget()
    {
        Debug.Log("타깃을 놓쳐서 탐색을 하면서도 원래 spawnPoint로 돌아간다");
        // GetComponent<NavMeshAgent>().isStopped = true;
        //GetComponent<NavMeshAgent>().isStopped = false;
        navmeshagent.isStopped = true;
        navmeshagent.isStopped = false;

        if (EnemyFSM.PatrolPattern == null)
        {
            //GetComponent<NavMeshAgent>().isStopped = true;
            //GetComponent<NavMeshAgent>().isStopped = false;
            //GetComponent<NavMeshAgent>().destination = spawnPoint.position;
            navmeshagent.isStopped = true;
            navmeshagent.isStopped = false;
            navmeshagent.SetDestination(spawnPoint.position);
        }
        else if (EnemyFSM.PatrolPattern != null)
        {
            //GetComponent<NavMeshAgent>().isStopped = true;
            // GetComponent<NavMeshAgent>().isStopped = false;
            //GetComponent<NavMeshAgent>().destination = AiControl.nextPosition;
            navmeshagent.isStopped = true;
            navmeshagent.isStopped = false;
            navmeshagent.SetDestination(EnemyFSM.nextPosition);
        }
    }
    public void WithinRange()
    {
        // GetComponent<NavMeshAgent>().isStopped = true;
        navmeshagent.isStopped = true;
    }
    public void NotWithinRange()
    {
        // GetComponent<NavMeshAgent>().isStopped = false;
        navmeshagent.isStopped = false;
    }
}
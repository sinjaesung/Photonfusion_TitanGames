using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Buffers;
using NanoSockets;
using UnityEditor.Experimental.GraphView;

public class EnemyFSM : MonoBehaviour, SEnemy
{
    [SerializeField] protected float AggroAreaDistance = 12f;
    public GameObject Target;
    [SerializeField] protected int ChaseDistance;
    [SerializeField] protected int DistanceSpawnPointReset = 30;
    protected bool returningToPoint = false;
    protected float DistanceToPoint;
    protected bool foundTarget = false;
    protected AiMover aiMover;
    [SerializeField] private float LeastDistance = 6.5f;

    [SerializeField] protected PatrolPattern patrolpattern;
    [SerializeField] protected float CloseToWayPoint = 3f;
    public Vector3 nextPosition;
    protected int currentWayPointIndex = 0;

    public NavMeshAgent navMeshAgent;//이동제어를 위한 NavMeshAgent

    public PatrolPattern PatrolPattern => patrolpattern;

    [SerializeField]
    protected float attackRange = 5f;//공격범위(이 범위 안에 들어오면 "Attack" 상태로 변경)
    [SerializeField]
    protected float attackRate = 1;//공격속도
    protected float lastAttackTime = 0;//공격 주기 계산용 변수

    [Header("Attack")]
    [SerializeField]
    protected projectiles projectilePrefab; //발사체 프리팹
    [SerializeField]
    protected Transform projectileSpawnPoint;//발사체 생성 위치

    public Status status;//이동속도 등의 정보
    protected EnemyMemoryPool enemyMemoryPool; //적 메모리 풀(적 오브젝트 관리)
    public EnemyProjectileMemeoryPool enemyProjectileMemoryPool;
    protected HealthPlayer targetHealth;

    [SerializeField] float naviMeshSpeed;

    [SerializeField] EnemyMeleeCollider[] enemymeleeColliders;
    [SerializeField] public bool attacking = false;

    [SerializeField] private ImpactMemoryPool impactmemorypool;
    [SerializeField] private ImpactType removeImpact;

    //PlayerAI WITH관련 2차기능추가
    //public GameObject ShootingRaycastArea;//발사체 발사기준origin raycasting 2차기능추가
    public Transform attacktarget; //적의 공격 대상 (플레이어류) 동적변경가능 2차기능추가

    public Transform playerTransform;
    public LayerMask PlayerLayer;//탐색감지 checkSphere layer
    public LayerMask AttackLayer;//공격레이어
    public Vector3 AttackDirection;//공격방향 동적변경
    public bool playerInshootingRadius;//공격범위내에있는지여부

    private void Awake()
    {
        playerTransform = FindObjectOfType<Player>().transform;
        Target = null;
        aiMover = GetComponent<AiMover>();

        navMeshAgent = GetComponent<NavMeshAgent>();

        //NavMeshAgent 컴포넌트에서 회전을 업데이트하지 않도록 설정
        // navMeshAgent.r
        //Debug.Log("Awake EnemyFSM셋업:" + navMeshAgent);

        status = GetComponent<Status>();
        navMeshAgent.speed = status.WalkSpeed;
        naviMeshSpeed = status.WalkSpeed;

        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetFloat("AttackSpeed", 5 * Mathf.Pow(attackRate, -1f));
        }

        for (int c = 0; c < enemymeleeColliders.Length; c++)
        {
            EnemyMeleeCollider colliderTarget = enemymeleeColliders[c];
            // Debug.Log("EnemyMeleeTargetss 근접유닛인 경우에한해 awake시에 셋업:" + colliderTarget+","+ status.attackdamage);
            colliderTarget.attackDamage = status.attackdamage;
            colliderTarget.referMother = this;
        }
    }
    private void Start()
    {
    }
    public virtual void Setup(Transform target, EnemyMemoryPool enemyMemoryPool)
    {
        status = GetComponent<Status>();
        status.CurrentHP = status.MaxHP;
        returningToPoint = false;

        navMeshAgent = GetComponent<NavMeshAgent>();
        this.attacktarget = target; //초기생성셋업시 플레이어로 설정되었다가 공격범위추산하여 근처에 플레이어류들이 적들로 동적변경가능.

        this.enemyMemoryPool = enemyMemoryPool;
        if (enemyProjectileMemoryPool != null)
        {
            this.enemyProjectileMemoryPool = enemyProjectileMemoryPool;
        }

        //NavMeshAgent 컴포넌트에서 회전을 업데이트하지 않도록 설정
        // Debug.Log("EnemyFSM setup pooing setup당시때의 Enemy생성당시의위치transform위치" + navMeshAgent+","+transform.position);
    }
    private void OnEnable()
    {
        returningToPoint = false;
        navMeshAgent.speed = status.WalkSpeed;
        naviMeshSpeed = status.WalkSpeed;

        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetFloat("AttackSpeed", 5 * Mathf.Pow(attackRate, -1f));
        }

        for (int c = 0; c < enemymeleeColliders.Length; c++)
        {
            EnemyMeleeCollider colliderTarget = enemymeleeColliders[c];
            //Debug.Log("EnemyMeleeTargetss 근접유닛인 경우에한해 Enable시에 셋업:" + colliderTarget + "," + status.attackdamage);
            colliderTarget.attackDamage = status.attackdamage;
            colliderTarget.referMother = this;
        }

        StartCoroutine(UpdateAttackTarget());
        /*if (IsPercentageRProjectile)
        {
            StartCoroutine(UpdatePercentageShoot());
        }*/
    }
    private void OnDisable()
    {
        //적이 비활성화될 때 현재 재생중인 상태를 종료하고, 상태를 "None"으로 설정
        AttackReset();
        StopAllCoroutines();
        Debug.Log("EnemyFSM OnDisable");
    }
    private void Update()
    {
        // Debug.Log("status??:" + status.CurrentHP);
        // Experience = experienceCond;

        if (status && status.CurrentHP > 0)
        {
            Aggro();
        }
        if (Target != null)
        {
            targetHealth = Target.GetComponent<HealthPlayer>();
            if (targetHealth.currentHealthPoint <= 0)
            {
                ReturnToSpawn();
            }
        }

        if (transform.position.y <= -999)
        {
            Debug.Log("EnemyFsm 현재 transformY위치가 -999이하로 터무니없이 작게 나오면 자신삭제" + gameObject);
            enemyMemoryPool.DeactivateEnemy(gameObject);
        }
        float originfromDistance = Vector3.Magnitude(new Vector3(transform.position.x, transform.position.y, transform.position.z) - new Vector3(0, 0, 0));
        if (originfromDistance >= 99999)
        {
            Debug.Log("EnemyFsm 현재 transform위치가 원점으로부터 터무니없이 멀면 자신삭제" + gameObject + "transformposition:" + transform.position);
            enemyMemoryPool.DeactivateEnemy(gameObject);
        }
    }

    private void Aggro()
    {
        //returningToPoint=true(returnToSpawnPoint) -> false로 다시 처리하는 부분에서 필요한 로직
        if (patrolpattern == null)
        {
            DistanceToPoint = Vector3.Distance(aiMover.spawnPoint.transform.position, this.transform.position);
            if (DistanceToPoint < LeastDistance)
            {
                //Debug.Log("맨처음 말고 캐릭터발견이후부터 다시 패트롤링까지 되려면 distanceToPoint < 2 이고,returningToPoint=false인경우에만 패트롤링,탐색시작" + DistanceToPoint);
                returningToPoint = false;
            }
        }
        else if (patrolpattern != null)
        {
            DistanceToPoint = Vector3.Distance(nextPosition, transform.position);
            if (DistanceToPoint < LeastDistance)
            {
                //Debug.Log("맨처음 말고 캐릭터발견이후부터 다시 패트롤링까지 되려면 distanceToPoint < 2 이고,returningToPoint=false인경우에만 패트롤링,탐색시작" + DistanceToPoint);
                returningToPoint = false;
            }
        }
        // Debug.Log("returnningtOpOOINT:" + returningToPoint);
        if (!returningToPoint)
        {
            if (Target == null)
            {
                searchTarget();
                if (patrolpattern != null)
                {
                    PatrolBehaviour();
                }
            }

            if (foundTarget && Target != null)
            {
                FollowTheTarget();
            }
        }
    }
    void searchTarget()
    {
        //Debug.Log("타깃을 놓쳐서 타깃을 다시 탐색시작한다");
        Vector3 center = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        Collider[] hitColliders = Physics.OverlapSphere(center, AggroAreaDistance);
        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].transform.tag == "Kart")//플레이어타깃
            {
                // Target = hitColliders[i].transform.gameObject;
                Target = playerTransform.gameObject;//player캐릭터개체추적.
                //attacktarget = Target.transform;
                foundTarget = true;
            }
            i++;
        }
    }
    protected virtual void Attack()
    {
        StopCoroutine("AttackExe");//2차기능추가개선 기존에 실행되고있던 어택코루틴은 최소한 종료
        StartCoroutine("AttackExe");
    }
    protected virtual void AttackReset()
    {
        StopCoroutine("AttackExe");
        attacking = false;
    }
    protected virtual IEnumerator AttackExe()
    {
        while (true)
        {
            //타겟 방향 주시
            // LookRotationToTarget();
            if (Time.time - lastAttackTime > attackRate)
            {
                attacking = true;
                //공격주기가 되야 공격할 수 있도록 하기 위해 현재 시간 저장
                lastAttackTime = Time.time;

                if (targetHealth != null && targetHealth.currentHealthPoint <= 0)
                {
                    Debug.Log("캐릭터가 공격중에 죽었으면 공격을 중단!");
                    if (GetComponent<Animator>() != null)
                    {
                        GetComponent<Animator>().SetBool("CanAttack", false);
                    }
                    StopCoroutine("AttackExe");
                    AttackReset();
                    ReturnToSpawn();
                    yield break;
                }

                //공격 애니메이션 실행
                if (GetComponent<Animator>() != null)
                {
                    GetComponent<Animator>().SetTrigger("BasisAttack1");//근접밀리어택
                                                                        // Debug.Log("EnemyFSM 공격모션 히히 basicAttack1");
                }
            }

            yield return null;
        }
    }
    private IEnumerator UpdateAttackTarget()
    {
        playerInshootingRadius = Physics.CheckSphere(transform.position, attackRange, PlayerLayer);

        if (playerInshootingRadius)
        {
            UpdateAttackPlayer();
        }

        yield return new WaitForSeconds(1f);//3초마다실행.

        StartCoroutine(UpdateAttackTarget());
    }
    void FollowTheTarget()
    {
        Vector3 targetPosition = Target.transform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);

        float distanceToPlayer = Vector3.Distance(Target.transform.position, this.transform.position); //DISTANCE BETWEEN TARGET AND ENEMY
        if (distanceToPlayer < ChaseDistance && DistanceToPoint < DistanceSpawnPointReset)
        {
            if (distanceToPlayer < attackRange && returningToPoint == false)
            {
                if (targetHealth != null && targetHealth.currentHealthPoint > 0)
                {
                    //Debug.Log("EnemyFSM 타깃발견 타깃을 공격!");

                    if (GetComponent<Animator>() != null)
                    {
                        GetComponent<Animator>().SetBool("CanAttack", true);
                    }
                    aiMover.WithinRange();

                    //타겟 방향 주시
                    // LookRotationToTarget();

                    Attack();
                }
            }
            else
            {
                if (GetComponent<Animator>() != null)
                {
                    GetComponent<Animator>().SetBool("CanAttack", false);
                }

                AttackReset();

                //타겟 방향 주시
                if (targetHealth != null && targetHealth.currentHealthPoint > 0)
                {
                    //LookRotationToTarget();

                    navMeshAgent.speed = status.RunSpeed;
                    naviMeshSpeed = status.RunSpeed;
                    aiMover.NotWithinRange();
                    //Debug.Log("EnemyFSM 타깃발견 타깃을 쫓는다!");
                    aiMover.ChaseTarget();//근접적타입은 캐릭터만 쫓는다.공격한다.
                }
            }
        }
        else
        {
            AttackReset();
            ReturnToSpawn();
        }
    }
    private void UpdateAttackPlayer()//원거리Enemy용
    {
        Collider[] Perceptiontargets = Physics.OverlapSphere(transform.position, attackRange, PlayerLayer);
        List<Collider> filterPerceptions = new List<Collider>();
        if (Perceptiontargets.Length > 0)
        {
            for (int t = 0; t < Perceptiontargets.Length; t++)
            {
                Collider target = Perceptiontargets[t];
                //Debug.Log("현재 공격범위내에서 감지된 모든 player류 타깃들: " + t + "| " + target.transform.name);
                if (target.tag == "Kart")
                {
                    filterPerceptions.Add(target);
                }
                else if (target.tag == "PlayerAI")
                {
                    filterPerceptions.Add(target);
                }
                else if (target.tag == "PlayerAI2")
                {
                    filterPerceptions.Add(target);
                }
            }
            for (int r = 0; r < filterPerceptions.Count; r++)
            {
                Collider target_ = filterPerceptions[r];
                // Debug.Log("PlayerLayer>Player,PlayerAI Tag까지 만족 순수 감지타입들:" + r + "| " + target_.transform.name);
            }
            if (filterPerceptions.Count > 0)
            {
                int random_index = Random.Range(0, filterPerceptions.Count);
                // if(filterPerceptions[random_index])
                // Debug.Log("n개의 타깃 Players대상체들중 0~n인댁스중에서 선택index:" + filterPerceptions.Count
                // + "개" + 0 + "~" + (filterPerceptions.Count - 1) + "," + random_index);

                Collider pickRandomTarget = filterPerceptions[random_index];
                // Debug.Log("Enemy 공격범위감지된 players류타깃들중 랜덤한 개체 지정공격:" + random_index + "/" + (filterPerceptions.Count) + "명," + pickRandomTarget.transform);

                if (pickRandomTarget != null)
                {
                    //AttackplayerBody = pickRandomTarget.transform;
                    attacktarget = pickRandomTarget.transform;
                    //transform.LookAt(attacktarget);// 공격범위내에서 랜덤지정선택(3초마다변경) 선정한 공격타깃을 바라본다.
                    AttackDirection = new Vector3(attacktarget.position.x - transform.position.x, attacktarget.position.y - transform.position.y,
                       attacktarget.position.z - transform.position.z);

                    Debug.Log("최종지정 AttackplayerBody! 3초간격!:3초마다 어택할 플레이어류 랜덤선택변경" + attacktarget.name);
                }
            }
        }
    }

    protected void ReturnToSpawn()
    {
        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetBool("CanAttack", false);
        }
        returningToPoint = true;
        Debug.Log("EnemyFSM 타깃을 범위밖으로 놓침");
        navMeshAgent.speed = status.WalkSpeed;
        naviMeshSpeed = status.WalkSpeed;
        foundTarget = false;
        Target = null;
        //stop the chase
        aiMover.StopChaseTarget();
    }
    private void OnDrawGizmos()
    {
        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, AggroAreaDistance);
    }
    protected void LookRotationToTarget()
    {
        //목표 위치
        Vector3 to = new Vector3(attacktarget.position.x, 0, attacktarget.position.z);
        //내 위치
        Vector3 from = new Vector3(transform.position.x, 0, transform.position.z);

        //바로 돌기
        transform.rotation = Quaternion.LookRotation(to - from);
        //서서히 돌기
        //Quaternion rotation = Quaternion.LookRotation(to - from);
        //transform.rotation = Quaternion.Slerp(transform.rotation,rotation,0.01f);
    }
    private void PatrolBehaviour()
    {
        //nextPosition = aiMover.My_Position_Guard;
        if (patrolpattern != null)
        {
            if (AnyWayPoint())
            {
                CycleWayPoint();
            }
            nextPosition = GetCurrentWayPoint();
        }
        //if (GetComponent<NavMeshAgent>() != null)
        //{
        //Debug.Log("GetComponent<NavMeshAgent>():" + GetComponent<NavMeshAgent>());
        //GetComponent<NavMeshAgent>().destination = nextPosition;
        //Debug.Log("nextPosition:" + nextPosition);
        navMeshAgent.SetDestination(nextPosition);
        //}
    }
    private void CycleWayPoint()
    {
        currentWayPointIndex = patrolpattern.GetNextIndex(currentWayPointIndex);
    }
    private bool AnyWayPoint()
    {
        float distanceToWayPoint = Vector3.Distance(transform.position, GetCurrentWayPoint());
        return distanceToWayPoint < CloseToWayPoint;
    }
    private Vector3 GetCurrentWayPoint()
    {
        return patrolpattern.GetPosWayPoint(currentWayPointIndex);
    }
    public void TakeDamage(float damage)
    {
        bool isDie = status.DecreaseHP(damage);
        /*if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetTrigger("Hit");
        }*/

        //Debug.Log("EnemyFSM Enemy 체력감소:" + status.CurrentHP + "isDie:" + isDie);
        if (isDie == true)
        {
            impactmemorypool.OnSpawnImpact(removeImpact, transform.position, Quaternion.identity);

            // DropLoot();
            //CombatEvents.EnemyDied(this);
            Debug.Log("EnemyFSM(Senemy) 대상체삭제:" + transform.name + ",경험치:" + Experience);
            Debug.Log("EnemyFSM 메모리풀삭제");
            enemyMemoryPool.DeactivateEnemy(gameObject);
        }
    }
}
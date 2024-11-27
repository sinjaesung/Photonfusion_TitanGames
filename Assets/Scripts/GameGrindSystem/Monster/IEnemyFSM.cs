using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class IEnemyFSM : MonoBehaviour, IEnemy
{
    public LayerMask aggroLayerMask;
    //public int Experience { get; set; }
    //public DropTable DropTable { get; set; }
    public IEnemySpawner Spawner { get; set; }

    private Player playerControls;
    protected NavMeshAgent navAgent;
    private Collider[] withinAggroColliders;
    [SerializeField] protected float AggroAreaDistance = 12f;
    [SerializeField] protected int DistanceSpawnPointReset = 30;
    public bool returningToPoint = false;
    [SerializeField] protected Transform spawnPoint;
    protected float DistanceToPoint;
    [SerializeField] private float LeastDistance = 6.5f;


    [Header("Attack")]
    public Status status; //이동속도 등의 정보
    protected IEnemySpawner enemyMemoryPool; //적 메모리 풀 (적 오브젝트 비활성화에 사용)
    protected Player targetHealth;
    [SerializeField] float naviMeshSpeed;

    [SerializeField] IEnemyMeleeCollider[] enemymeleeColliders;
    [SerializeField] public bool attacking = false;

    [SerializeField]
    protected float attackRange = 5; //공격 범위 (이 범위 안에 들어오면 "Attack" 상태로 변경)
    [SerializeField]
    protected float attackRate = 1; //공격 속도
    protected float lastAttackTime = 0; //공격 주기 계산용 변수

    //PlayerAI WITH관련 2차기능추가
    //public GameObject ShootingRaycastArea;//발사체 발사기준origin raycasting 2차기능추가
    public Transform attacktarget; //적의 공격 대상 (플레이어류) 동적변경가능 2차기능추가
    public Transform playerTransform;//적의 추적 대상
    public LayerMask PlayerLayer;//탐색감지 checkSphere layer
    public LayerMask AttackLayer;//공격레이어
    public Vector3 AttackDirection;//공격방향 동적변경
    public bool playerInshootingRadius;//공격범위내에있는지여부

    public float AudioRate = 1;
    public float CalcTimer = 0;
    private void Awake()
    {
       // playerTransform = FindObjectOfType<Player>().transform;
       // targetHealth = playerTransform.GetComponent<Player>();

        status = GetComponent<Status>();
        navAgent = GetComponent<NavMeshAgent>();
        //Debug.Log("Awake IEnemyFSM셋업:" + navAgent);
        navAgent.speed = status.WalkSpeed;
        naviMeshSpeed = status.WalkSpeed;

        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetFloat("AttackSpeed", 5 * Mathf.Pow(attackRate, -1f));
        }

        for (int c = 0; c < enemymeleeColliders.Length; c++)
        {
            IEnemyMeleeCollider colliderTarget = enemymeleeColliders[c];
            //Debug.Log("IEnemyMeleeColliders 근접유닛인 경우에한해 awake시에 셋업:" + colliderTarget + "," + status.attackdamage);
            colliderTarget.attackDamage = status.attackdamage;
            colliderTarget.referMother = this;
        }
    }

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }
    public virtual void Setup(IEnemySpawner enemyMemoryPool)
    {
        status = GetComponent<Status>();
        status.CurrentHP = status.MaxHP;
        returningToPoint = false;

        navAgent = GetComponent<NavMeshAgent>();
        this.enemyMemoryPool = enemyMemoryPool;
       /* if (enemyProjectileMemoryPool != null)
        {
            this.enemyProjectileMemoryPool = enemyProjectileMemoryPool;
        }
*/
        //NavMeshAgent 컴포넌트에서 회전을 업데이트하지 않도록 설정
        //Debug.Log("IEnemyFSM setup pooing setup:" + navAgent);
    }
    private void OnEnable()
    {
        //적이 활성화될 때 적의 상태를 "대기"로 설정
        // ChangeState(EnemyState.Idle);
        //Debug.Log("IEnemyFSM OnEnable");

        navAgent.speed = status.WalkSpeed;
        naviMeshSpeed = status.WalkSpeed;

        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetFloat("AttackSpeed", 5 * Mathf.Pow(attackRate, -1f));
        }

        for (int c = 0; c < enemymeleeColliders.Length; c++)
        {
            IEnemyMeleeCollider colliderTarget = enemymeleeColliders[c];
            // Debug.Log("IEnemyMeleeColliders 근접유닛인 경우에한해 awake시에 셋업:" + colliderTarget + "," + status.attackdamage);
            colliderTarget.attackDamage = status.attackdamage;
            colliderTarget.referMother = this;
        }
        // Experience = experienceCond;

        StartCoroutine(UpdateAttackTarget());
    }
    private void OnDisable()
    {
        //적이 비활성화될 때 현재 재생중인 상태를 종료하고, 상태를 "None"으로 설정
        AttackReset();
        StopAllCoroutines();
        Debug.Log("IEnemyFSM OnDisable");
    }
    void UpdateAnim()
    {
        //Vector3 velocity = GetComponent<NavMeshAgent>().velocity;
        if (navAgent != null)
        {
            Vector3 velocity = navAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            //Debug.Log("몬스터navMesh의 속력:" + speed);
            if (GetComponent<Animator>() != null)
            {
                GetComponent<Animator>().SetFloat("ForwardSpeed", speed);
            }
        }
    }
    void FixedUpdate()
    {
        if (transform.position.y <= -999)
        {
            Debug.Log("IEnemyFsm 현재 transformY위치가 -999이하로 터무니없이 작게 나오면 자신삭제" + gameObject);
            enemyMemoryPool.DeactivateEnemy(gameObject);
        }
        float originfromDistance = Vector3.Magnitude(new Vector3(transform.position.x, transform.position.y, transform.position.z) - new Vector3(0, 0, 0));
        if (originfromDistance >= 99999)
        {
            Debug.Log("IEnemyFsm 현재 transform위치가 원점으로부터 터무니없이 멀면 자신삭제" + gameObject + "transformposition:" + transform.position);
            enemyMemoryPool.DeactivateEnemy(gameObject);
        }

        //Experience = experienceCond;

        withinAggroColliders = Physics.OverlapSphere(transform.position, AggroAreaDistance, aggroLayerMask);
        if (withinAggroColliders.Length > 0)
        {
            //Debug.Log("Found Player I think.");
            //attacktarget = withinAggroColliders[0].GetComponent<PlayerControls>().transform;
            //playerTransform = attacktarget;//주변에 플레이어본체 찾으면 스크립트로 지정.
           if(attacktarget)
             targetHealth = attacktarget.GetComponent<Player>();

            DistanceToPoint = Vector3.Distance(spawnPoint.position, transform.position);
            if (DistanceToPoint < LeastDistance)
            {
                returningToPoint = false;
            }
            //Debug.Log("DistanceTOpOINTS:" + DistanceToPoint);
            if (!returningToPoint)
            {
                if (targetHealth != null && targetHealth.Health > 0)
                {
                    //ChasePlayer(withinAggroColliders[0].GetComponent<PlayerControls>());
                    if(attacktarget)
                     ChasePlayer(attacktarget);
                }
            }
            else
            {
                // Debug.Log("IEnemyFSM 플레이어 아직 쫓을 수 없는 상황 DistanceToPoint:" + DistanceToPoint + ",returningToPoint:" + returningToPoint);
            }
        }
        else
        {
            navAgent.speed = status.WalkSpeed;
            naviMeshSpeed = status.WalkSpeed;
        }

        UpdateAnim();
    }
    protected virtual void Attack()
    {
        StopCoroutine("AttackExe");//2차기능 개선
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
            LookRotationToTarget();
            if (Time.time - lastAttackTime > attackRate)
            {
                attacking = true;
                //공격주기가 되야 공격할 수 있도록 하기 위해 현재 시간 저장
                lastAttackTime = Time.time;

                if (targetHealth != null && targetHealth.Health <= 0)
                {
                    Debug.Log("캐릭터가 공격중에 죽었으면 공격을 중단!");
                    if (GetComponent<Animator>() != null)
                    {
                        GetComponent<Animator>().SetBool("CanAttack", false);
                    }
                    StopCoroutine("AttackExe");
                    yield break;
                }

                //공격 애니메이션 실행
                if (GetComponent<Animator>() != null)
                {
                    GetComponent<Animator>().SetTrigger("BasisAttack1");//근접밀리어택
                    // Debug.Log("IEnemyFSM 공격모션 히히 basicAttack1");
                }
            }
            yield return null;
        }
    }
    public void WithinRange()
    {
        // GetComponent<NavMeshAgent>().isStopped = true;
        navAgent.isStopped = true;
    }
    public void NotWithinRange()
    {
        // GetComponent<NavMeshAgent>().isStopped = false;
        navAgent.isStopped = false;
    }
    void ChasePlayer(Transform player)
    {
        //this.playerControls = player;
        float distanceToPlayer = Vector3.Distance(player.transform.position, this.transform.position); //DISTANCE BETWEEN TARGET AND ENEMY
        // Debug.Log("IENFMYFMS CHASEPLAYER distanceToPlayer" + DistanceToPoint + "," + DistanceSpawnPointReset + "distanceToPlayer:" + distanceToPlayer);

        if (DistanceToPoint < DistanceSpawnPointReset)
        {
            if (distanceToPlayer <= attackRange && returningToPoint == false)
            {
                if (targetHealth != null && targetHealth.Health > 0)
                {
                    //Debug.Log("IEnemyFSM 타깃 공격범위내로발견 타깃을 공격!");

                    if (GetComponent<Animator>() != null)
                    {
                        GetComponent<Animator>().SetBool("CanAttack", true);
                    }
                    WithinRange();
                    //타겟 방향 주시
                    LookRotationToTarget();

                    Attack();
                }
            }
            else
            {//returningToPoint가 false여야만 chasePlayer는 실행되기에 returningToPoint=true인데 실행되는 경우는 없음.
                //Debug.Log("IEnemyFSM Not within distance"+ distanceToPlayer);//returningToPoint==false && navAgent.remaingDistance > attackRange(추적상황)

                if (GetComponent<Animator>() != null)
                {
                    GetComponent<Animator>().SetBool("CanAttack", false);
                }

                AttackReset();

                //타겟 방향 주시
                if (targetHealth != null && targetHealth.Health > 0)
                {
                    LookRotationToTarget();
                    CalcTimer += Time.deltaTime;
                    //MonsterChase.Play();
                    Debug.Log("CalcTimer" + (CalcTimer) + ">" + AudioRate);
                    if (CalcTimer > AudioRate)
                    {
                        Debug.Log("IEnemyFSM 타깃 추적 추적사운드>>");
                        AudioManager.PlayAndFollow("HugeManStamp", transform, AudioManager.MixerTarget.SFX);
                        CalcTimer = 0;
                    }
                    else
                    {
                        Debug.Log("IEnemyFSM 오디오 추적 타깃 사운드 쿨타임>>");
                    }

                    navAgent.speed = status.RunSpeed;
                    naviMeshSpeed = status.RunSpeed;
                    // Debug.Log("IEnemyFSM 타깃을 쫓는다!");
                    navAgent.SetDestination(player.position);
                }
                NotWithinRange();
            }
        }
        else
        {
            // Debug.Log("IEnemyFSM DistanceSpawnPointReset spawnPoint으로부터의 최대이동거리를 초과한경우 다시 첫 소환위치로 돌아가게한다");
            AttackReset();
            ReturnToSpawn();
        }
    }
    private IEnumerator UpdateAttackTarget()
    {
        playerInshootingRadius = Physics.CheckSphere(transform.position, AggroAreaDistance, PlayerLayer);

        if (playerInshootingRadius)
        {
            UpdateAttackPlayer();
        }

        yield return new WaitForSeconds(1f);//3초마다실행.

        StartCoroutine(UpdateAttackTarget());
    }
    private void UpdateAttackPlayer()//원거리Enemy용
    {
        Collider[] Perceptiontargets = Physics.OverlapSphere(transform.position, AggroAreaDistance, PlayerLayer);
        List<Collider> filterPerceptions = new List<Collider>();
        if (Perceptiontargets.Length > 0)
        {
            for (int t = 0; t < Perceptiontargets.Length; t++)
            {
                Collider target = Perceptiontargets[t];
                // Debug.Log("현재 공격범위내에서 감지된 모든 player류 타깃들: " + t + "| " + target.transform.name);
                if (target.tag == "Player")
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
                //   + "개" + 0 + "~" + (filterPerceptions.Count - 1) + "," + random_index);

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
        Debug.Log("IEnemyFSM 타깃을 범위밖으로 놓치면서,DistancetoSpawnPoint최대이동거리초과하여 다시 첫소환지로 돌아감");
        navAgent.speed = status.WalkSpeed;
        naviMeshSpeed = status.WalkSpeed;

        StopChaseTarget();
    }
    public void StopChaseTarget()
    {
        Debug.Log("타깃을 놓쳐서 DistancetoSpawnPoint최대이동거리량 초과하여 원래 spawnPoint로 돌아간다");
        // GetComponent<NavMeshAgent>().isStopped = true;
        //GetComponent<NavMeshAgent>().isStopped = false;
        navAgent.isStopped = true;
        navAgent.isStopped = false;

        /*float RandomXRange = Random.Range(-6, 6);//-6~5.999범위숫자
        float RandomZRange = Random.Range(-6, 6);//-6~5.999범위숫자
        float RandomYRange = Random.Range(-6, 6);//-6~5.999범위숫자

        Debug.Log("ReturnToSpawn spawnPoint주변 랜덤지역범위로 돌아감"
            + new Vector3(RandomXRange, RandomYRange, RandomZRange));
        navAgent.SetDestination(spawnPoint.position + new Vector3(RandomXRange, RandomYRange, RandomZRange));*/
        navAgent.SetDestination(spawnPoint.position);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, AggroAreaDistance);
    }
    protected void LookRotationToTarget()
    {
        //공격,추적시 본 캐릭터를 바라보게
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
    public void TakeDamage(float damage)
    {
        bool isDie = status.DecreaseHP(damage);
        /*if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetTrigger("Hit");
        }*/

        //Debug.Log("IEnemyFSM Enemy 체력감소:" + status.CurrentHP + "isDie:" + isDie);
        if (isDie == true)
        {
            //impactmemorypool.OnSpawnImpact(removeImpact, transform.position, Quaternion.identity);

            //DropLoot();
            // CombatEvents.EnemyDied(this);
            //Debug.Log("IEnemyFSM(Senemy) 대상체삭제:" + transform.name + ",경험치:" + Experience);
            Debug.Log("IEnemyFSM 메모리풀삭제");
            enemyMemoryPool.DeactivateEnemy(gameObject);
            if (!enemyMemoryPool.isDestroyed)
            {
                enemyMemoryPool.Respawn();
            }
            else
            {
                Debug.Log("IEnemyFSM IEnemySpawner Destroy상태에선 리스폰되지않음");
            }
        }
    }
}
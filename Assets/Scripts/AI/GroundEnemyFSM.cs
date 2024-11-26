using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class GroundEnemyFSM : EnemyFSM
{
    protected override void AttackReset()
    {
        Debug.Log("GroundEnemyFSM override method AttackReset");
        StopCoroutine("AttackExe");
        attacking = false;
    }
    /*protected override IEnumerator AttackExe()
    {
        while (true)
        {
            //타겟 방향 주시
            //LookRotationToTarget();
            if (Time.time - lastAttackTime > attackRate && AttackDirection != null && attacktarget != null)
            {
                attacking = true;

                RaycastHit hit;

                //공격주기가 되야 공격할 수 있도록 하기 위해 현재 시간 저장
                lastAttackTime = Time.time;

                if (Physics.Raycast(transform.position, AttackDirection, out hit, attackRange, AttackLayer))//water,env,player
                {
                    // Debug.Log("GroundEnemyFSM| ShootingAttack 레이케스트 발사충돌개체,공격타깃" + hit.transform.name + "," +
                    // attacktarget.name + ":AttackDirection" + AttackDirection);

                    if (hit.transform.GetComponent<Player>() != null)
                    {
                        // Debug.Log("GroundEnemyFSM|장애물등을 모두 피하고 순수 타깃PlayerControls에게 다가가 명중 성공:공격명중|공격코루틴while실행" + hit.transform.name);

                        //발사체 생성
                        // GameObject clone = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                        Bounds bounds = attacktarget.GetComponent<Collider>().bounds;
                        //Debug.Log("player Collider boundss:" + bounds.size.y / 2 + "," + attacktarget.position);

                        if (targetHealth != null && targetHealth.currentHealthPoint <= 0)
                        {
                            Debug.Log("캐릭터가 공격중에 죽었으면 공격을 중단!");
                            if (GetComponent<Animator>() != null)
                            {
                                GetComponent<Animator>().SetBool("CanAttack", false);
                            }
                            StopCoroutine("AttackExe");
                            yield break;
                        }

                        //Debug.Log("enemy공격 원거리공격 기능addon AttackExe 상속코루틴 overrides");
                        //공격 애니메이션 실행
                        if (GetComponent<Animator>() != null)
                        {
                            GetComponent<Animator>().SetTrigger("BasisAttack1");//근접밀리어택
                             // Debug.Log("enemy공격 원거리공격모션 히히 basicAttack1");
                        }

                        // Debug.Log("enemyProjectileMemoryPool??:" + enemyProjectileMemoryPool);
                        GameObject poolclone = enemyProjectileMemoryPool.SpawnProjectile(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                        if (poolclone)
                            poolclone.GetComponent<EnemyProjectile>().Setup((attacktarget.position) + new Vector3(0, bounds.size.y / 2, 0), status, status.attack_distance);
                    }
                    else
                    {
                        //Debug.Log("GroundEnemyFSM|충돌개체가 player류가 아닌 다른 대상체인경우(사물,지형 등)|공격코루틴while break(한번실행이후코루틴종료)" + hit.transform.name);

                        if (targetHealth != null && targetHealth.currentHealthPoint <= 0)
                        {
                            Debug.Log("캐릭터가 공격중에 죽었으면 공격을 중단!");
                            if (GetComponent<Animator>() != null)
                            {
                                GetComponent<Animator>().SetBool("CanAttack", false);
                            }
                            StopCoroutine("AttackExe");
                            yield break;
                        }

                        //Debug.Log("enemy공격 원거리공격 기능addon AttackExe 상속코루틴 overrides");
                        //공격 애니메이션 실행
                        if (GetComponent<Animator>() != null)
                        {
                            GetComponent<Animator>().SetTrigger("BasisAttack1");//근접밀리어택
                            //Debug.Log("enemy공격 원거리공격모션 히히 basicAttack1");
                        }
                        //발사체 생성
                        // GameObject clone = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

                        //Debug.Log("enemyProjectileMemoryPool??:" + enemyProjectileMemoryPool);
                        GameObject poolclone = enemyProjectileMemoryPool.SpawnProjectile(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                        if (poolclone)
                            poolclone.GetComponent<EnemyProjectile>().Setup((attacktarget.position), status, status.attack_distance);

                        //Debug.Log("GroundEnemyFSM 원거리공격발사적중 레이케스트개체가 사물,지형등인 경우 한번만 명중시키고," +
                        // "공격코루틴모두종료시키고,기존nextPoint로 돌아가게한다");
                        AttackReset();
                        ReturnToSpawn();

                        yield break;
                    }

                }
                else
                {
                    //Debug.Log("만족RaycastHit개체가 없는경우 EnemyFSM에 한해서 로직상 attacktarget,AttackDirection으로 " +
                    // "항상 발사하게끔 처리(레이케스터 미검출때도 항상발사처리)" + attacktarget.name + ":AttackDirection" + AttackDirection);

                    if (targetHealth != null && targetHealth.currentHealthPoint <= 0)
                    {
                        Debug.Log("캐릭터가 공격중에 죽었으면 공격을 중단!");
                        if (GetComponent<Animator>() != null)
                        {
                            GetComponent<Animator>().SetBool("CanAttack", false);
                        }
                        StopCoroutine("AttackExe");
                    }

                    // Debug.Log("enemy공격 원거리공격 기능addon AttackExe 상속코루틴 overrides");
                    //공격 애니메이션 실행
                    if (GetComponent<Animator>() != null)
                    {
                        GetComponent<Animator>().SetTrigger("BasisAttack1");//근접밀리어택
                        // Debug.Log("enemy공격 원거리공격모션 히히 basicAttack1");
                    }
                    //발사체 생성
                    GameObject poolclone = enemyProjectileMemoryPool.SpawnProjectile(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                    if (poolclone)
                        poolclone.GetComponent<EnemyProjectile>().Setup((attacktarget.position), status, status.attack_distance);

                }
            }

            //Debug.Log("지형사물방향 충돌발사로 인한 공격while코루틴 yield break종료!! GroundEnemyFSM");
            yield return null;
        }
    }*/
}
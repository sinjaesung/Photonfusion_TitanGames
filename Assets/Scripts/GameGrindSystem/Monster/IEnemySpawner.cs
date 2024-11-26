using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Progress;

public class IEnemySpawner : MonoBehaviour
{
    public GameObject monster;//관리할 몬스터 프리팹타깃(IEnemySpawner하나당 한개씩의 몬스터관리)
    public bool respawn;
    public float spawnDelay;
    private float currentTime;
    private bool spawning;
    public MemoryPool enemyMemoryPool;

    //[SerializeField] private EnemyProjectileMemoryPool enemyProjectileMemoryPool;

    public bool isDestroyed = false;
    private void Awake()
    {
        enemyMemoryPool = new MemoryPool(monster, 5);//5개 이상씩은 넘치지 않게 관리
    }
    public void DeactivateEnemy(GameObject enemy)
    {
        enemyMemoryPool.DeactivatePoolItem(enemy);
    }
    public void DestroySpawner()
    {
        isDestroyed = true;
    }
    private void Start()
    {
        StartCoroutine(firstStartSpawn());
        currentTime = spawnDelay;
    }
    private IEnumerator firstStartSpawn()
    {
        yield return new WaitForSeconds(2f);
        Spawn();
        yield return null;
    }

    private void Update()
    {
        if (spawning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                Spawn();
            }
        }
    }

    public void Respawn()
    {
        Debug.Log("몬스터Respawn!: memoryPool pooling 생성");
        spawning = true;
        currentTime = spawnDelay;
    }

    public void Spawn()
    {
        Debug.Log("몬스터Spawn: memoryPool pooling 생성");
        //IEnemy instance = Instantiate(monster, transform.position, Quaternion.identity).GetComponent<IEnemy>();
        GameObject monsterPrefab = enemyMemoryPool.ActivatePoolItem();
        if (monsterPrefab != null)
        {
            monsterPrefab.transform.position = transform.position;
            IEnemy instance = monsterPrefab.transform.GetComponent<IEnemy>();
            instance.Spawner = this;
            monsterPrefab.GetComponent<IEnemyFSM>().Setup(this);

            spawning = false;
        }
    }
}
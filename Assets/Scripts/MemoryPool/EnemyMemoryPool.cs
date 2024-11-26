using System.Collections;
using UnityEngine;

public class EnemyMemoryPool : MonoBehaviour
{
    [SerializeField]
    public Transform target; //적의 목표 (플레이어)
    [SerializeField]
    private GameObject enemySpawnPointPrefab; //적이 등장하기 전 적의 등장 위치 알려주는 프리팹
    [SerializeField]
    private GameObject enemyPrefab; //생성되는 적 프리팹
    [SerializeField]
    private float enemySpawnTime = 6; //적 생성 주기
    [SerializeField]
    private float enemySpawnLatency = 3; //타일 생성 후 적이 등장하기까지 대기 시간

    private MemoryPool spawnPointMemoryPool; //적 등장 위치를 알려주는 오브젝트 생성, 활성&비활성 관리
    private MemoryPool enemyMemoryPool; //적 생성, 활성&비활성 관리

    private int numberOfEnemiesSpawnedAtOnce = 1; //동시에 생성되는 적의 숫자

    [SerializeField] private Vector2Int mapAreaSize = new Vector2Int(20, 20); //맵 크기
    [SerializeField] private float OriginPosX = 0;
    [SerializeField] private float OriginPosY = 0;
    [SerializeField] private float OriginPosZ = 0;


    //[SerializeField] private EnemyProjectileMemoryPool enemyProjectileMemoryPool;

    [SerializeField]
    private int createAmount = 16;

    private void Awake()
    {
        spawnPointMemoryPool = new MemoryPool(enemySpawnPointPrefab, createAmount);
        enemyMemoryPool = new MemoryPool(enemyPrefab, createAmount);

        StartCoroutine("SpawnTile");//이 존재를 감싸고있는 건물형container오브젝트가 제거되면,
                                    //그와 형제인 이요소의 spawnTile공정을 중지
    }
    public void SpawnTilingCoroutineStop()
    {
        Debug.Log("EnemyBuilding파괴된경우에 한해서 SpawnTilingCoroutineStop|EnemyMemoryPool");
        StopCoroutine("SpawnTile");
    }
    private IEnumerator SpawnTile()
    {
        int currentNumber = 0;
        int maximumNumber = 50;

        while (true)
        {
            //동시에 numberOfEnemiesSpawnedAtOnce 숫자만큼 적이 생성되도록 반복문 사용
            for (int i = 0; i < numberOfEnemiesSpawnedAtOnce; ++i)
            {
                GameObject item = spawnPointMemoryPool.ActivatePoolItem();

                //0,0맵 중심좌표기준 -50~50x범위, -50~50z범위, y축은 항상1의 위치에 지정 결과적-50,-50~50,50에 해당 맵범위에 모두 속함 > -49,-49 ~ 49,49
                item.transform.position = new Vector3(OriginPosX + Random.Range(-mapAreaSize.x * 0.49f, mapAreaSize.x * 0.49f), OriginPosY + 1,
                   OriginPosZ + Random.Range(-mapAreaSize.y * 0.49f, mapAreaSize.y * 0.49f));

                StartCoroutine("SpawnEnemy", item);//적생성 위치 기둥을 GameObject인자로 넘긴다.
            }

            currentNumber++;

            if (currentNumber >= maximumNumber)
            {
                currentNumber = 0;
                //numberOfEnemiesSpawnedAtOnce++;
            }

            yield return new WaitForSeconds(enemySpawnTime);
        }
    }

    private IEnumerator SpawnEnemy(GameObject point)
    {
        yield return new WaitForSeconds(enemySpawnLatency);

        //적 오브젝트 생성하고, s 위치를 point의 위치로 설정
        GameObject item = enemyMemoryPool.ActivatePoolItem();
        if (item)
        {
            item.transform.position = point.transform.position;
            // Debug.Log("Enemy생성당시의위치!!:" + item.transform.position + "," + point.transform.position);
            item.GetComponent<EnemyFSM>().Setup(target, this);
        }

        //타일 오브젝트를 비활성화
        spawnPointMemoryPool.DeactivatePoolItem(point);
    }

    public void DeactivateEnemy(GameObject enemy)
    {
        enemyMemoryPool.DeactivatePoolItem(enemy);
    }
}
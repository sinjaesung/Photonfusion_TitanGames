using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public enum GameState
{
    Waiting,
    MediaPlaying,
    Playing,
    Completed,
}

public class GameLogic : NetworkBehaviour, IPlayerLeft,IPlayerJoined
{
    [SerializeField] public NetworkPrefabRef[] playerPrefabs;
    [SerializeField] private Transform spawnpoint;
    [SerializeField] private Transform spawnpointPivot;

    [Networked] private Player Winner { get; set; }
    [Networked, OnChangedRender(nameof(GameStateChanged))] private GameState State { get; set; }
    public GameState gameState => State;
    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, Player> Players => default;
    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, int> CharacterIndexes => default;
    public NetworkBehaviour gamemanager;
    public GameManager gamemanagerObj;
    public PlayerSpawner CharacterSpawner;

    public bool isSpawned = false;

    public bool IsJackAlive = false;
    public int isFairyAliveCnt = 0;
    void Start()
    {
        CharacterSpawner = FindObjectOfType<PlayerSpawner>();

        Debug.Log("GameLogic NetworkBehaviour Start");
        
        Debug.Log("NetworkBehaviour StartSceneMenu CharacterSubmit CharacterSpawner.StartSpawn" + CharacterSpawner);

        //Invoke(nameof(SetUpStart), 0.8f);
    }
    /*public void SetUpStart()
    {
        Debug.Log("GameLogic SetUpStart>>");
        //if (HasStateAuthority)
        //{
        var gamemanagerObj_ = Runner.Spawn(gamemanager, transform.position, Quaternion.identity);
            gamemanagerObj = gamemanagerObj_.GetComponent<GameManager>();
        //}
    }*/
    void Update()
    {
        CharacterSpawner = FindObjectOfType<PlayerSpawner>();

        //if (HasStateAuthority)
        //{
            CharacterSpawner.SetData(gamemanagerObj);
        //}

        if (isSpawned)
        {
            int e = 0;
            foreach (KeyValuePair<PlayerRef, int> player in CharacterIndexes)
            {
                Debug.Log(e + " | GameLogic]] 플레이어 선택 인댁스 현황:" + player.Value);
            }
        }  
    }
    public override void Spawned()
    {
        Winner = null;
        State = GameState.Waiting;
        UIManager.Singleton.SetWaitUI(State, Winner);
        Debug.Log("GameLogic Spawned>>" + Runner);
        Runner.SetIsSimulated(Object, true);
        isSpawned = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect when a player enters the finish platform's trigger collider
        if (Runner.IsServer && Winner == null && other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out Player player))
        {
            //UnreadyAll();
            //Winner = player;
            //State = GameState.Waiting;
            player.IsArrive = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Players.Count < 1)
            return;

        if (Runner.IsServer && State == GameState.Waiting)
        {
            bool areAllReady = true;
            foreach (KeyValuePair<PlayerRef, Player> player in Players)
            {
                if (!player.Value.IsReady)
                {
                    areAllReady = false;
                    break;
                }
            }

            if (areAllReady)
            {
                Winner = null;
                State = GameState.MediaPlaying;
                //Waiting->MediaPlaying
               //PreparePlayers();
            }
        }

        if (IsAllStartRequested())
        {
            Debug.Log("GameLogic 모든 플레이어 시작영상 재생완료한 경우 q키>>");
            State = GameState.Playing;
        }
        if (IsAnyoneArrived())
        {
            Debug.Log("GameLogic 임의 플레이어 도착점 완료한 경우>>");
            UnreadyAll();
            State = GameState.Completed;
        }
        if (IsAllDied())
        {
            State = GameState.Completed;
            CharacterEndingStatus();
        }

        if (State == GameState.Playing && !Runner.IsResimulation)
            UIManager.Singleton.UpdateLeaderboard(Players.OrderByDescending(p => p.Value.Score).ToArray());
    }

    private void GameStateChanged()
    {
        UIManager.Singleton.SetWaitUI(State, Winner);
    }

    private void PreparePlayers()
    {
        float spacingAngle = 360f / Players.Count;
        spawnpointPivot.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        foreach (KeyValuePair<PlayerRef, Player> player in Players)
        {
            GetNextSpawnpoint(spacingAngle, out Vector3 position, out Quaternion rotation);
            player.Value.Teleport(position, rotation);
            player.Value.ResetCooldowns();
        }
    }

    public int CharacterEndingStatus()
    {
        foreach (KeyValuePair<PlayerRef, Player> player in Players)
        {
            if(player.Value.CharacterType == "Jack")
            {
                if (!player.Value.IsDie)
                    IsJackAlive = true;
                else
                    IsJackAlive = false;
            }
            else
            {
                if (!player.Value.IsDie)
                {
                    isFairyAliveCnt++;
                }
            }
        }
        Debug.Log("CharacterEndingStatus>>");
        Debug.Log("isJackAlive,isFairyAliveCnt>" + IsJackAlive + "," + isFairyAliveCnt);
        if(IsJackAlive == true && isFairyAliveCnt >= 3)
        {
            Debug.Log("모두 생존");
            return 0;
        }else if(IsJackAlive == true && (0 <isFairyAliveCnt && isFairyAliveCnt <= 2)){
            Debug.Log("잭은 생존,요정 일부만 생존");
            return 1;
        } 
        else if(IsJackAlive == false && isFairyAliveCnt >0)
        {
            Debug.Log("잭은 죽고,요정만 생존");
            return 2;
        }
        else if(IsJackAlive == true && isFairyAliveCnt <= 0)
        {
            Debug.Log("잭만 생존");
            return 3;
        }
        else if(IsJackAlive == false && isFairyAliveCnt <= 0)
        {
            Debug.Log("모두 죽음");
            return 4;
        }
        return 5;
    }
    private void UnreadyAll()
    {
        foreach (KeyValuePair<PlayerRef, Player> player in Players)
            player.Value.IsReady = false;
    }
    private void UnArriveAll()
    {
        foreach (KeyValuePair<PlayerRef, Player> player in Players)
            player.Value.IsArrive = false;
    }
    private bool IsAllStartRequested()
    {
        bool allRequested = true;
        foreach(KeyValuePair<PlayerRef,Player> player in Players)
        {
            if (!player.Value.IsStartRequest)
                allRequested = false;
        }
        return allRequested;
    }
    private bool IsAnyoneArrived()
    {
        bool AnyoneArrived = false;
        foreach (KeyValuePair<PlayerRef, Player> player in Players)
        {
            if (player.Value.IsArrive)
                AnyoneArrived = true;
        }
        return AnyoneArrived;
    }
    private bool IsAllDied()
    {
        Debug.Log("모든 캐릭터가 죽은경우>>");
        bool AllDied = true;
        foreach(KeyValuePair<PlayerRef,Player> player in Players){
            if (!player.Value.IsDie)
                AllDied = false;
        }
        return AllDied;
    }

    private void GetNextSpawnpoint(float spacingAngle, out Vector3 position, out Quaternion rotation)
    {
        position = spawnpoint.position;
        rotation = spawnpoint.rotation;
        spawnpointPivot.Rotate(0f, spacingAngle, 0f);
    }

    void IPlayerJoined.PlayerJoined(PlayerRef player)
    {
        Debug.Log("GameLogic PlayerJoined>>");
       if (HasStateAuthority)
        {
            GetNextSpawnpoint(90f, out Vector3 position, out Quaternion rotation);
            Debug.Log("GameLogic PlayerJoined HasStateAuthority PlayerJoined>>");

            var IsValid = false;
            PlayerRef random_key;
            int random_indexUse=0;
            int safeCnt = 800;
            int cnt = 0;
            while (!IsValid)
            {
                var random_index = Random.Range(0, playerPrefabs.Length);

                random_key = CharacterIndexes.FirstOrDefault(x => x.Value == random_index).Key;
                if (!CharacterIndexes.ContainsKey(random_key))
                {
                    IsValid = true;
                    random_indexUse = random_index;
                }
                else
                {
                    Debug.Log(random_index + "는 이미 존재하는 선택캐릭터> cnt:"+ cnt);
                    //IsValid = true;
                    //random_indexUse = random_index;
                }
  
                if (cnt >= safeCnt)
                {
                    break;
                }

                cnt++;
            }
            if (IsValid)
            {
                Debug.Log("PlayerJoined Random_index>>" + random_indexUse + "," + playerPrefabs[random_indexUse]);
                NetworkObject playerObject = Runner.Spawn(playerPrefabs[random_indexUse], position, rotation, player);

                Players.Add(player, playerObject.GetComponent<Player>());

                if(random_indexUse == 0)
                {
                    playerObject.GetComponent<Player>().CharacterType = "Jack";
                }
                else
                {
                    Debug.Log("Genie Character Spawn>>");
                    playerObject.GetComponent<Player>().CharacterType = "Genie";
                }

                CharacterIndexes.Add(player, random_indexUse);
            }      
        }
    }
    public void PlayerAdd(PlayerRef player, NetworkObject playerObject)
    {
        Debug.Log("GameLogic PlayerAdd>>"+ playerObject.transform.name);
        Players.Add(player, playerObject.GetComponent<Player>());
    }
    void IPlayerLeft.PlayerLeft(PlayerRef player)
    {
        Debug.Log("GameLogic PlayerLeft>>");
        if (!HasStateAuthority)
             return;
       
        if (Players.TryGet(player, out Player playerBehaviour))
        {
            Players.Remove(player);
            Runner.Despawn(playerBehaviour.Object);
        }
    }
}

using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class StartSceneMenu : SimulationBehaviour
{

    // Start is called before the first frame update
    [SerializeField] NetworkPrefabRef[] characters;
    [SerializeField] public int selectIndex=0;

    public NetworkPrefabRef character;
   
    public PlayerSpawner CharacterSpawner;
    void Start()
    {
        //CharacterSpawner = FindObjectOfType<PlayerSpawner>();
        Debug.Log("StartSceneMenu Start Runner>>" + Runner);
    }

    // Update is called once per frame
    void Update()
    {
        //CharacterSpawner = FindObjectOfType<PlayerSpawner>();
    }


   /* public void ChooseCharacter(int index)
    {
        GameManager gamemanager = FindObjectOfType<GameManager>();
        Debug.Log("StartSceneMenu ChooseCharacter>>" + index);
        selectIndex = index;
        NetworkPrefabRef character_ = characters[index];
        gamemanager.SelectedCharacter = character_;
        character = character_;
    }*/
    /*public void CharacterSubmit()
    {
        Debug.Log("NetworkBehaviour StartSceneMenu gameManagerObj:" + character);
        
        if (CharacterSpawner)
        {
            CharacterSpawner.StartSpawn();
        }
    }*/
    public void SceneMove()
    {
        SceneManager.LoadSceneAsync("Game");
    }
}

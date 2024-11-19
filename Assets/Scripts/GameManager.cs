using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public NetworkPrefabRef SelectedCharacter;

    private void Start()
    {
        instance = this;

        DontDestroyOnLoad(this);
    }

}

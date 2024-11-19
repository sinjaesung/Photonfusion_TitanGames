using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEndTest : MonoBehaviour
{
    public static GameEndTest Singleton
    {
        get => _singleton;
        set
        {
            if (value == null)
                _singleton = null;
            else if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(GameEndTest)}!");
            }
        }
    }
    private static GameEndTest _singleton;

    [SerializeField] private Animator EndingObject;
    [SerializeField] AudioSource audiosource;
   
    private void Awake()
    {
        Singleton = this;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    public void EndingAction()
    {
        Debug.Log("GameEndingAction MonoBehaviour>>");
        audiosource.Play();
        EndingObject.SetBool("EndAction",true);
    }
}

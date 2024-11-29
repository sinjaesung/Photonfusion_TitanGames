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
    [SerializeField] private EndVideoTest EndvideoTest;
    public UIManager uimanager;
    private void Awake()
    {
        Singleton = this;
    }
    private void Update()
    {
        uimanager = FindObjectOfType<UIManager>();
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    public void EndingAction(int result)
    {
        Debug.Log("GameEndingAction MonoBehaviour>>"+ result);
        audiosource.Play();

        if(result == 0)
        {
            Debug.Log("GameEndingAction End Result 0>>");
            uimanager.CompleteMissionTextStatus("All Alive");
            EndvideoTest.SetEnding(0);
        }
        else if(result == 1)
        {
            Debug.Log("GameEndingAction End Result 1>>");
            uimanager.CompleteMissionTextStatus("Jack Alive,genie part Alive");
            EndvideoTest.SetEnding(1);
        }
        else if (result == 2)
        {
            Debug.Log("GameEndingAction End Result 2>>");
            uimanager.CompleteMissionTextStatus("Jack Died,genie all Alive");
            EndvideoTest.SetEnding(2);
        }
        else if (result == 3)
        {         
            Debug.Log("GameEndingAction End Result 3>>");
            uimanager.CompleteMissionTextStatus("Only Jack Alive");
            EndvideoTest.SetEnding(3);
        }
       /* else if (result == 4)
        {       
            Debug.Log("GameEndingAction End Result 4>>");
            uimanager.CompleteMissionTextStatus("All Die");
            EndvideoTest.SetEnding(4);
        }*/
        
        EndingObject.SetBool("EndAction",true);
    }
}

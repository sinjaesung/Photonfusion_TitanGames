using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndVideoTest : MonoBehaviour
{
    public static EndVideoTest Singleton
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
                Debug.LogError($"There should only ever be one instance of {nameof(StartVideoTest)}!");
            }
        }
    }
    private static EndVideoTest _singleton;

    [SerializeField] AdvancedPlayer[] EndVideoList;

    public int ending;
    private void Awake()
    {
        Singleton = this;
    }
    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }
    public void SetEnding(int ending_)
    {
        Debug.Log("SetEnding>>" + ending_);
        ending = ending_;
        StartMediaVideo(ending_);
    }

    public void StartMediaVideo(int ending_)
    {
        Debug.Log("EndVideoTest MonoBehaviour>>"+ ending_);
        EndVideoList[ending_].gameObject.SetActive(true);
        EndVideoList[ending_].SetVideoPlayer(ending_);
        //video.SetPlayEvent();
    }
    public void StopMediaVideo(int ending_)
    {
        Debug.Log("EndVideoTest MonoBehaviour>>");
        EndVideoList[ending_].SetStopEvent();
        EndVideoList[ending_].gameObject.gameObject.SetActive(false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartVideoTest : MonoBehaviour
{
    public static StartVideoTest Singleton
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
    private static StartVideoTest _singleton;

    [SerializeField] AdvancedPlayer video;

    private void Awake()
    {
        Singleton = this;
    }
    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    public void StartMediaVideo()
    {
        Debug.Log("StartMediaVideo MonoBehaviour>>");
        video.gameObject.SetActive(true);
        video.SetPlayEvent();
    }
    public void StopMediaVideo()
    {
        Debug.Log("StopMediaVideo MonoBehaviour>>");
        video.SetStopEvent();
        video.gameObject.SetActive(false);
    }
}

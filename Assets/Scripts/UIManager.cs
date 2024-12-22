using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton
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
                Debug.LogError($"There should only ever be one instance of {nameof(UIManager)}!");
            }
        }
    }
    private static UIManager _singleton;

    //[SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] public GameObject instructionTextWrap;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI completeText;
    [SerializeField] private TextMeshProUGUI completeMissionText;
    [SerializeField] private Slider grappleCD;
    [SerializeField] private Slider glideCD;
    [SerializeField] private Image glideActive;
    [SerializeField] private Slider doubleJumpCD;
    [SerializeField] private LeaderboardItem[] leaderboardItems;

    public Player LocalPlayer;
    public GameEndTest gameEndTest;

    public GameLogic gamelogic;
    public GameObject GameMenuObj;
    public bool GameMenuOn = false;

    private void Awake()
    {
        gamelogic = FindObjectOfType<GameLogic>();

        Singleton = this;

        grappleCD.value = 0f;
        glideCD.value = 0f;
        doubleJumpCD.value = 0f;
    }
    public void CallGameMenu(bool _val)
    {
        if (_val==true)//false->true
        {
            GameMenuObj.SetActive(true);
            GameMenuOn = true;
        }
        else if(_val==false)//true ->false
        {
            GameMenuObj.SetActive(false);
            GameMenuOn = false;
        }
    }
    private void Update()
    {
        gamelogic = FindObjectOfType<GameLogic>();

        gameEndTest = FindObjectOfType<GameEndTest>();

        if (LocalPlayer == null)
            return;

        grappleCD.value = LocalPlayer.GrappleCDFactor;
        doubleJumpCD.value = LocalPlayer.DoubleJumpCDFactor;

        glideActive.enabled = LocalPlayer.IsGliding;
        glideCD.value = LocalPlayer.IsGliding ? LocalPlayer.GlideCharge : LocalPlayer.GlideCDFactor;
    }

    public void CompleteMissionTextStatus(string txt)
    {
        Debug.Log("CompleteMissionTextStatus>>" + txt);
        completeMissionText.text = txt;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    public void DidSetReady()
    {
        instructionText.text = "Waiting for other players to be ready...";
    }

    public void SetWaitUI(GameState newState, Player winner)
    {
        Debug.Log("UIManager SetGameState newState>" + newState);
        if (newState == GameState.Waiting)
        {
            if (winner == null)
            {
                Debug.Log("SetWait WaitingMode");
               // gameStateText.text = "Waiting to Start";
                instructionText.text = "Press R when you're ready to begin!";
            }
            else
            {
                Debug.Log("SetWait WaitingMode");
                // gameStateText.text = $"{winner.Name} Wins";
                instructionText.text = "Press R when you're ready to play again!";
            }
        }/*else if(newState == GameState.MediaPlaying)
        {
            startmediaTest.StartMediaVideo();
        }else if(newState == GameState.Playing)
        {
            startmediaTest.StopMediaVideo();
        }*/
        else if (newState == GameState.Completed)
        {
            completeText.text = "";

            int result = gamelogic.CharacterEndingStatus();
            Debug.Log("UiManager Completed Result>>" + result);
            gameEndTest.EndingAction(result);
        }

        //gameStateText.enabled = newState == GameState.Waiting;
        instructionText.enabled = newState == GameState.Waiting;
        completeText.enabled = newState == GameState.Completed;
    }

    public void UpdateLeaderboard(KeyValuePair<Fusion.PlayerRef, Player>[] players)
    {
        for (int i = 0; i < leaderboardItems.Length; i++)
        {
            LeaderboardItem item = leaderboardItems[i];
            if (i < players.Length)
            {
                item.nameText.text = players[i].Value.Name;
                item.heightText.text = $"{players[i].Value.Score}m";
            }
            else
            {
                item.nameText.text = "";
                item.heightText.text = "";
            }
        }
    }

    [Serializable]
    private struct LeaderboardItem
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI heightText;
    }
}

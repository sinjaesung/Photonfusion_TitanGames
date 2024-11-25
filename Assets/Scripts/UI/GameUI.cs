using System.Collections;
using System.Resources;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameUI : MonoBehaviour
{
    public interface IGameUIComponent
    {
        void Init(PlayerEntity entity);
    }

    public CanvasGroup fader;
    public Animator introAnimator;
    //public Animator countdownAnimator;
    public Animator itemAnimator;
    public GameObject timesContainer;
    public GameObject coinCountContainer;
    //public GameObject lapCountContainer;
    public GameObject pickupContainer;
    //public EndRaceUI endRaceScreen;
    public Image pickupDisplay;
    public Image boostBar;
    public Image HealthBar;
    public Text coinCount;
    //public Text lapCount;
    //public Text raceTimeText;
    //public Text[] lapTimeTexts;
    public Text introGameModeText;
    public Text introTrackNameText;
    public Button continueEndButton;
    //private bool _startedCountdown;

    public PlayerEntity playerEntity { get; private set; }
    //private KartController KartController => Kart.Controller;

    private void Start()
    {
        FadeIn();
    }
    public void Init(PlayerEntity player_)
    {
        playerEntity = player_;

        var uis = GetComponentsInChildren<IGameUIComponent>(true);
        foreach (var ui in uis) ui.Init(player_);

        //kart.LapController.OnLapChanged += SetLapCount;

        /* var track = Track.Current;

         if (track == null)
             Debug.LogWarning($"You need to initialize the GameUI on a track for track-specific values to be updated!");
         else
         {
             introGameModeText.text = GameManager.Instance.GameType.modeName;
             introTrackNameText.text = track.definition.trackName;
         }*/

        /* GameType gameType = GameManager.Instance.GameType;

         if (gameType.IsPracticeMode())
         {
             timesContainer.SetActive(false);
             lapCountContainer.SetActive(false);
         }*/

        /*  if (gameType.hasPickups == false)
          {
              pickupContainer.SetActive(false);
          }
          else
          {*/
        ClearPickupDisplay();
        //}

        /* if (gameType.hasCoins == false)
         {
             coinCountContainer.SetActive(false);
         }*/

        //continueEndButton.gameObject.SetActive(kart.Object.HasStateAuthority);

        player_.OnHeldItemChanged += index =>
        {
            if (index == -1)
            {
                ClearPickupDisplay();
            }
            else
            {
                StartSpinItem();
            }
        };

        player_.OnCoinCountChanged += count =>
        {
            AudioManager.Play("coinSFX", AudioManager.MixerTarget.SFX);
            Debug.Log("_OnCoinCountChanged>>" + count);
            coinCount.text = $"{count:00}";
        };
    }

    private void OnDestroy()
    {
        //Kart.LapController.OnLapChanged -= SetLapCount;
    }

    public void FinishCountdown()
    {
        // Kart.OnRaceStart();
    }

    public void HideIntro()
    {
        introAnimator.SetTrigger("Exit");
    }

    private void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float t = 1;
        while (t > 0)
        {
            fader.alpha = 1 - t;
            t -= Time.deltaTime;
            yield return null;
        }
    }

    private void Update()
    {
        /*if (!Kart || !Kart.LapController.Object || !Kart.LapController.Object.IsValid)
            return;*/

       /* if (!_startedCountdown && Track.Current != null && Track.Current.StartRaceTimer.IsRunning)
        {
            var remainingTime = Track.Current.StartRaceTimer.RemainingTime(Kart.Runner);
            if (remainingTime != null && remainingTime <= 3.0f)
            {
                _startedCountdown = true;
                HideIntro();
                FadeIn();
                countdownAnimator.SetTrigger("StartCountdown");
            }
        }*/

        UpdateBoostBar();
        UpdateHealthBar();

        //if (Kart.LapController.enabled) UpdateLapTimes();

        var controller = playerEntity.Controller;
        if (controller.BoostTime > 0f)
        {
            if (controller.BoostTierIndex == -1) return;

            Color color = controller.driftTiers[controller.BoostTierIndex].color;
            SetBoostBarColor(color);
        }
        /*else
        {
            if (!controller.IsDrifting) return;

            SetBoostBarColor(controller.DriftTierIndex < controller.driftTiers.Length - 1
                ? controller.driftTiers[controller.DriftTierIndex + 1].color
                : controller.driftTiers[controller.DriftTierIndex].color);
        }*/
    }

    private void UpdateBoostBar()
    {
        if (!playerEntity.Object || !playerEntity.Object.IsValid)
            return;

        //var driftIndex = playerEntity.Controller.DriftTierIndex;
        var boostIndex = playerEntity.Controller.BoostTierIndex;

        /*if (playerEntity.Controller.IsDrifting)
        {
            if (driftIndex < playerEntity.Controller.driftTiers.Length - 1)
                SetBoostBar((playerEntity.Controller.DriftTime - playerEntity.Controller.driftTiers[driftIndex].startTime) /
                            (playerEntity.Controller.driftTiers[driftIndex + 1].startTime - playerEntity.Controller.driftTiers[driftIndex].startTime));
            else
                SetBoostBar(1);
        }*/
        //else
        //{
            SetBoostBar(boostIndex == -1
                ? 0f
                : playerEntity.Controller.BoostTime / playerEntity.Controller.driftTiers[boostIndex].boostDuration);
        //}
    }
    private void UpdateHealthBar()
    {
        if (!playerEntity.Object || !playerEntity.Object.IsValid)
            return;

        Debug.Log("UpdateHealthBar>>" + playerEntity.Controller.Health /playerEntity.Controller.maxHealth);
        HealthBar.fillAmount = playerEntity.Controller.Health / playerEntity.Controller.maxHealth;
    }
    /* private void UpdateLapTimes()
     {
         if (!Kart.LapController.Object || !Kart.LapController.Object.IsValid)
             return;
         var lapTimes = Kart.LapController.LapTicks;
         for (var i = 0; i < Mathf.Min(lapTimes.Length, lapTimeTexts.Length); i++)
         {
             var lapTicks = lapTimes.Get(i);

             if (lapTicks == 0)
             {
                 lapTimeTexts[i].text = "";
             }
             else
             {
                 var previousTicks = i == 0
                     ? Kart.LapController.StartRaceTick
                     : lapTimes.Get(i - 1);

                 var deltaTicks = lapTicks - previousTicks;
                 var time = TickHelper.TickToSeconds(Kart.Runner, deltaTicks);

                 SetLapTimeText(time, i);
             }
         }

         SetRaceTimeText(Kart.LapController.GetTotalRaceTime());
     }*/

    public void SetBoostBar(float amount)
    {
        boostBar.fillAmount = amount;
    }

    public void SetBoostBarColor(Color color)
    {
        boostBar.color = color;
    }

    public void SetCoinCount(int count)
    {
        coinCount.text = $"{count:00}";
    }

  /*  private void SetLapCount(int lap, int maxLaps)
    {
        var text = $"{(lap > maxLaps ? maxLaps : lap)}/{maxLaps}";
        lapCount.text = text;
    }

    public void SetRaceTimeText(float time)
    {
        raceTimeText.text = $"{(int)(time / 60):00}:{time % 60:00.000}";
    }

    public void SetLapTimeText(float time, int index)
    {
        lapTimeTexts[index].text = $"<color=#FFC600>L{index + 1}</color> {(int)(time / 60):00}:{time % 60:00.000}";
    }*/

    public void StartSpinItem()
    {
        StartCoroutine(SpinItemRoutine());
    }

    private IEnumerator SpinItemRoutine()
    {
        itemAnimator.SetBool("Ticking", true);
        float dur = 3;
        float spd = Random.Range(9f, 11f); // variation, for flavor.
        float x = 0;
        while (x < dur)
        {
            x += Time.deltaTime;

            itemAnimator.speed = (spd - 1) / (dur * dur) * (x - dur) * (x - dur) + 1;
            yield return null;
        }

        itemAnimator.SetBool("Ticking", false);
        SetPickupDisplay(playerEntity.HeldItem);
        // Kart.canUseItem = true;
    }

    public void SetPickupDisplay(Powerup item)
    {
        if (item)
            pickupDisplay.sprite = item.itemIcon;
        else
            pickupDisplay.sprite = null;
    }

    public void ClearPickupDisplay()
    {
        SetPickupDisplay(ResourceManager.Instance.noPowerup);
    }

  /*  public void ShowEndRaceScreen()
    {
        endRaceScreen.gameObject.SetActive(true);
    }

    // UI Hook

    public void OpenPauseMenu()
    {
        InterfaceManager.Instance.OpenPauseMenu();
    }*/
}
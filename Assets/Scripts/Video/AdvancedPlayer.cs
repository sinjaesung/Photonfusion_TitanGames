using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class AdvancedPlayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Components")]
    [SerializeField] private VideoPlayer videoPlayer;

    [SerializeField] private RectTransform rtWindow;


    [Header("Information")]
    [SerializeField] private Vector2 prevWindowSize = new Vector2(0, 0);
    [SerializeField] private Vector2 prevWindowSize2 = Vector2.zero;

    [SerializeField] private Image imgUIFrame;

    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text textTime;

    public bool IsEnd = false;

    private void Awake()
    {

    }
    private void OnEnable()
    {

    }

    private void Start()
    {
        slider.onValueChanged.AddListener(SetSliderValue);
        //초기화 작업을 해준다.
        prevWindowSize = rtWindow.sizeDelta;

        //재생시간 확인하기 위해 필요한 것들이 있다.
        //영상의 총시간(max), 재생되고있는 현재 시간(cur)
        slider.maxValue = (float)videoPlayer.length;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void WindowModeEvent()
    {
        Debug.Log("창모드");

        rtWindow.anchorMin = new Vector2(0.5f, 0.5f);
        rtWindow.anchorMax = new Vector2(0.5f, 0.5f);

        //rtWindow.sizeDelta = new Vector2(1280f, 720f);
        rtWindow.sizeDelta = prevWindowSize;
    }

    public void FullModeEvent()
    {
        Debug.Log("전체화면");
        //=======================
        //1.anchor값을 0,0,1,1 형태로 할당
        //2.offsetSize를 0,0,0,0 으로 초기화

        rtWindow.anchorMin = new Vector2(0, 0);
        rtWindow.anchorMax = new Vector2(1, 1);

        rtWindow.offsetMin = new Vector2(0, 0);
        rtWindow.offsetMax = new Vector2(0, 0);
    }

    public void SetPlayEvent()
    {
        Debug.Log("재생");
        videoPlayer.Play();
    }

    public void SetPauseEvent()
    {
        Debug.Log("일시정지");
        videoPlayer.Pause();
    }

    public void SetStopEvent()
    {
        Debug.Log("리셋");
        videoPlayer.Stop();
        videoPlayer.playbackSpeed = 1.0f;
        IsEnd = true;
    }

    #region 인터페이스를 사용했다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("호버");
        imgUIFrame.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("언호버");
        imgUIFrame.gameObject.SetActive(false);
    }
    #endregion

    private void SetSliderValue(float _value)
    {
        Debug.Log("SetSliderValue>:" + _value);
        videoPlayer.time = _value;
    }
    private string CalcTime(double _time)
    {
        string strTime = string.Empty;
        //현재 재생되고 있는 영상의 시간값을 받아온다.
        float time = (float)_time;

        int hour = (int)time / 3600;
        int min = (int)(time % 3600) / 60;
        int sec = (int)(time % 60);

        if (hour > 0)
        {
            //영상의 재생시간이 1시간 이상이면 시간을 표기
            strTime = hour.ToString("D2") + ":" + min.ToString("D2") + ":" + sec.ToString("D2");
        }
        else
        {
            strTime = min.ToString("D2") + ":" + sec.ToString("D2");
        }

        return strTime;
    }

    void Update()
    {
        Debug.Log("now playtime:" + videoPlayer.time + "/" + videoPlayer.length);

        slider.value = (float)videoPlayer.time;

        //textTime.text = string.Format("{0} / {1}", CalcTime(videoPlayer.time), CalcTime(videoPlayer.length));
        textTime.text = CalcTime(videoPlayer.time) + " / " + CalcTime(videoPlayer.length);
    }
}

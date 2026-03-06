using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Video;
using UnityEngine.Playables;
using UnityEngine.EventSystems;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

/*
 * if (PlayAudio)
                    if (PlayVideo)
                    if (PlayTimeLine)
 */

public class BasicPlayerInteraction : MonoBehaviour
{
    //PlayObjects
    public bool PlayAudio;
    public AudioSource audioSource;
    public bool PlayVideo;
    public VideoPlayer videoPlayer;
    public bool PlayTimeLine;
    public PlayableDirector playableDirector;
    //MenuElements
    [SerializeField] private Button playButton,fastForwardButton,rewindButton;
    [SerializeField] private Slider ProgressSlider;
    [SerializeField] private PlayInformation[] informations;

    private Image playIcon;
    //Datas
    [SerializeField]private int _playState = 0;
    /// <summary>
    /// <list type="table">
    /// <item>Paused =0</item>
    /// <item>Playing=1</item>
    /// <item>Stopped=2</item>
    /// </list>
    /// </summary>
    public int PlayState
    {
        get
        {
            return _playState;
        }
        set
        {
            _playState = value;
            switch (value)
            {
                case 0:
                    if (PlayAudio)
                        audioSource.Pause();
                    if (PlayVideo)
                        videoPlayer.Pause();
                    if (PlayTimeLine)
                        playableDirector.Pause();
                    break;
                case 1:
                    PlayProgress = PlayProgress;
                    if (PlayAudio)
                        audioSource.Play();
                    if (PlayVideo)
                        videoPlayer.Play();
                    if (PlayTimeLine)
                        playableDirector.Play();
                    break;
                case 2:
                    if (PlayAudio)
                        audioSource.Stop();
                    if (PlayVideo)
                        videoPlayer.Stop();
                    if (PlayTimeLine)
                        playableDirector.Stop();
                    break;
            }
        }
    }
    public double PlayProgress
    {
        get
        {
            if (PlayAudio)
                return audioSource.time;
            else if (PlayVideo)
                return videoPlayer.time;
            else if (PlayTimeLine)
                return playableDirector.time;
            else
                return 0;
        }
        set
        {
            if (value < 0)
                value = 0;
            else if (value > Duration)
                value = Duration - 3;
            if (PlayAudio)
                audioSource.time = (float)value;
            if (PlayVideo)
            {
                videoPlayer.time = value;
            }
            if (PlayTimeLine)
            {
                playableDirector.initialTime = value;
                playableDirector.time = value;
                playableDirector.Evaluate();
            }
        }
    }
    public double Duration
    {
        get
        {
            if (PlayAudio)
                return audioSource.clip.length;
            else if (PlayVideo)
                return videoPlayer.length;
            else if (PlayTimeLine)
                return playableDirector.duration;
            else
                return 0;
        }
    }
    public float PlaySpeed = 1;

    public int BPM, BitLength;
    public double SPN//Second Per Note
    {
        get
        {
            return 60 / (BPM*BitLength);
        }
    }
    public int Bit
    {
        get
        {
            return (int)(PlayProgress * BPM / 60);
        }
    }
    public double BitDouble
    {
        get
        {
            return (PlayProgress * BPM / 60);
        }
    }
    public int NoteIndex
    {
        get
        {
            return Mathf.RoundToInt((float)(PlayProgress * BPM* BitLength / 60));
        }
    }
    [ReadOnly]public bool DraggingSlider = false;
    //Resources
    [PreviewField(ObjectFieldAlignment.Center, Height = 50),HorizontalGroup]
    [SerializeField] private Sprite playSprite, pauseSprite;
    void Awake()
    {
        //数据
        playButton.onClick.AddListener(playButtonFunction);
        ProgressSlider.maxValue = (float)Duration;
        //获取一下slider上面EventTrigger组件
        EventTrigger sliderEventTrigger = ProgressSlider.GetComponent<EventTrigger>();
        //首先，需要初始化一个类，这个类里面你需要指定：1.你要添加哪种事件 2.事件发生时，你要执行什么方法
        EventTrigger.Entry entry = new EventTrigger.Entry();
        //这里先添加，当按下时的事件
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { DraggingSlider = true;  });
        //把这个类加到eventTrigger里的类列表里
        sliderEventTrigger.triggers.Add(entry);

        if (PlayTimeLine)
            playableDirector?.RebuildGraph();

        //外观
        playIcon = playButton.transform.GetChild(0).GetComponent<Image>();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (DraggingSlider)
        {
            PlayProgress = ProgressSlider.value;
            DraggingSlider = false;
        }
        else
            ProgressSlider.value = (float)PlayProgress;

        foreach(PlayInformation i in informations)
        {
            i.UpdateData(PlayProgress,Bit,NoteIndex);
        }

        switch (PlayState)
        {
            case 1:
                playIcon.sprite = pauseSprite;
                break;
            default:
                playIcon.sprite = playSprite;
                break;
        }
    }
    private void playButtonFunction()
    {
        switch (PlayState)
        {
            case 0:
                PlayState = 1;
                break;
            case 1:
                 PlayState = 0;
                break;
        }
    }
    public void ResetPlayer()
    {
        PlayState = 2;
        PlayState = 0;
        PlayProgress = 0;
        playIcon.sprite = pauseSprite;
    }
}

[Serializable]
public class PlayInformation
{
    public TextMeshProUGUI informationText;
    public string informations;

    private double progress;
    private int bit;
    private int node;
    private string PlayProgressbyTime
    {
        get
        {
            int Minuite = (int)(progress / 60);
            int Second = (int)(progress % 60);
            int mSecond = (int)((progress % 1) * 60);
            return Covert(Minuite) + ":" + Covert(Second) + ":" + Covert(mSecond);
        }
    }
    private string Covert(int Input)
    {
        if (Input >= 10)
        {
            return (Input.ToString());
        }
        else
        {
            return ("0" + Input.ToString());
        }
    }
    public string FormatString()
    {
        return Regex.Replace(informations, @"\{(\w+)\}", match =>
        {
            string varName = match.Groups[1].Value;
            return GetVariableValue(varName);
        });
    }

    private string GetVariableValue(string variableName)
    {
        switch (variableName.ToLower())
        {
            case "progressbytime":
                return PlayProgressbyTime;
            case "bit":
                return bit.ToString();
            case "node":
                return node.ToString();
            // 添加更多需要支持的变量...
            default:
                return $"<undefined:{variableName}>"; // 处理未定义的变量
        }
    }
    public void UpdateData(double playProgress, int Bit,int Node)
    {
        progress = playProgress;
        bit = Bit;
        node = Node;
        informationText.text = FormatString();
    }
}

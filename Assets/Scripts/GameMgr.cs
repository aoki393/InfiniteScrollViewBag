using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
    [Header("预加载设置")]
    public bool preloadWeaponData = true;
    public bool showLoadingPanel = true;    

    [Header("status显示")]
    public Text statusText; // 在Inspector中拖拽Text控件到这里
    public float updateInterval = 1.0f; // 更新间隔（秒）
    [Header("Cat动画")]
    public Animator catAnimator;
    public event UnityAction EventBagOpen = null;
    public event UnityAction EventBagClose = null;
    [Header("背包开关音效")]
    public AudioSource sound;
    // public AudioClip openClip;
    // public AudioClip closeClip;

    [Header("音量条")]
    public Slider volumeSlider;
    public AudioSource bgm;

    private float accum = 0.0f; // 累计帧时间
    private int frames = 0; // 帧数计数

    private static GameMgr _instance;

    public static GameMgr Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
        // DontDestroyOnLoad(gameObject);

        // 初始化音量条
        if (volumeSlider == null || bgm == null || sound == null)
        {
            Debug.LogError("volumeSlider or bgm or Bag Open Sound is not assigned in GameMgr.");
            return;
        }
        else
        {
            bgm.volume = 0.3f;
            sound.volume = 0.6f;

            volumeSlider.value = bgm.volume * 2;

            // 监听音量条值改变事件
            volumeSlider.onValueChanged.AddListener((value) =>
            {
                bgm.volume = value * 0.5f;
                sound.volume = value;
            });
        }

        // 初始化
        Debug.Log($"初始化游戏系统（主面板显示、bgm及音效挂载、FPS显示）……\n realtimeSinceStartup:{Time.realtimeSinceStartup:F2}秒");
        InitializeGameSystems();
        Debug.Log($"初始化游戏系统完成！\nrealtimeSinceStartup:{Time.realtimeSinceStartup:F2}秒");
    }
    void Start()
    {
        // 预加载游戏数据
        StartCoroutine(PreloadGameData());        
    }
    private IEnumerator PreloadGameData()
    {
        if (!preloadWeaponData) yield break;

        if (showLoadingPanel)
        {
            LoadingPanel.Show("游戏数据初始化中……");
            yield return null; // 让UI先更新一帧
        }

        // 预加载武器数据
        Debug.Log($"开始预加载武器数据……\n realtimeSinceStartup:{Time.realtimeSinceStartup:F2}秒");
        float startTime = Time.realtimeSinceStartup;

        // PackageWeaponData.Preload();
        // yield return PackageWeaponData.PreloadAsync();
        // 使用带进度条信息的异步预加载
        yield return PackageWeaponData.Preload03Async(progress =>
        {
            if (showLoadingPanel)
            {
                // LoadingPanel.SetProgress(progress);
                LoadingPanel.SetProgress(progress,$"加载武器数据:");
            }
        });

        float loadTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"武器数据加载耗时: {loadTime:F2}秒");

        if (showLoadingPanel)
        {
            LoadingPanel.Hide();
        }
        Debug.Log("数据预加载完成！LoadingPanel已关闭");
    }
    private void InitializeGameSystems()
    {       
        MainPanel.ShowMe();

        EventBagOpen += SetCatDanceAnimation;
        EventBagOpen += PlayOpenBagSound;

        EventBagClose += SetCatIdleAnimation;
        EventBagClose += PlayCloseBagSound;

        if (statusText == null)
        {
            Debug.LogWarning("statsText is not assigned in GameMgr.");
            return;
        }
        StartCoroutine(UpdateFPS());
    }

    /// <summary>
    /// G: 开关 GM面板
    /// B：开关 背包面板
    /// </summary>
    void Update()
    {
        // 用于计算FPS
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // 按键监测
        if (Input.GetKeyDown(KeyCode.G))
        {
            OpenCloseGMPanel();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            OpenClosePackageRX();
        }

    }
    void OnDestroy()
    {
        // 当对象被销毁时停止所有协程
        StopAllCoroutines();
    }
    void UpdateDisplay(float fps)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"FPS: {fps:F2}");
        // sb.AppendLine($"Draw Calls: {drawCalls}");
        // sb.AppendLine($"Memory: {usedMemory}MB / {totalMemory}MB");

        if(statusText != null)
        {
            statusText.text = sb.ToString();
        }
    }
    IEnumerator UpdateFPS()
    {
        while (true)
        {
            // 等待指定的更新间隔
            yield return new WaitForSeconds(updateInterval);

            // 计算平均FPS
            float fps = accum / frames;

            // 重置计数器和累计值
            accum = 0.0f;
            frames = 0;

            UpdateDisplay(fps);
        }
    }
    private void SetCatDanceAnimation()
    {
        if (catAnimator == null)
        {
            Debug.LogWarning("catAnimator is not assigned in GameMgr.");
            return;
        }
        catAnimator.SetBool("isBagOpen", true);
    }
    private void SetCatIdleAnimation()
    {
        if (catAnimator == null)
        {
            Debug.LogWarning("catAnimator is not assigned in GameMgr.");
            return;
        }
        catAnimator.SetBool("isBagOpen", false);
    }
    private void PlayOpenBagSound()
    {
        if (sound != null)
        {
            sound.Play();
        }
        else
        {
            Debug.LogWarning("Bag Open Sound is not assigned in GameMgr.");
        }
    }
    private void PlayCloseBagSound()
    {
        if (sound != null)
        {
            sound.Play();
        }
        else
        {
            Debug.LogWarning("Bag Open Sound is not assigned in GameMgr.");
        }
    }

    // 开启或关闭面板，public给外部调用
    // public void OpenClosePackage()
    // {        
    //     OpenClosePackageRX();        
    // }
    public void OpenCloseGMPanel()
    {
        if (GMPanelController.Instance == null || !GMPanelController.Instance.gameObject.activeSelf)
        {
            GMPanelController.ShowMe();
        }
        else
        {
            GMPanelController.HideMe();
        }
    }
    public void OpenClosePackageRX()
    {
        if (PackageRXController.Instance == null || !PackageRXController.Instance.gameObject.activeSelf)
        {
            PackageRXController.ShowMe();
            EventBagOpen?.Invoke();
        }
        else
        {
            PackageRXController.HideMe();
            EventBagClose?.Invoke();
        }
    }

}

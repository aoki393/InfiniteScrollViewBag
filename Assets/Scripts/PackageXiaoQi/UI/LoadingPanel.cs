using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    // [SerializeField] private Slider progressBar;
    // [SerializeField] private Text progressText;
    // // 进度条相关
    // private float currentProgress = 0f;
    // private float targetProgress = 0f;
    // private const float progressSmoothSpeed = 2f;
    // private static LoadingPanel currentInstance;

    // public static void Show(string message = "加载中...")
    // {
    //     // 如果已有实例，先销毁
    //     if (currentInstance != null)
    //     {
    //         Destroy(currentInstance.gameObject);
    //     }

    //     // 从资源加载预制体
    //     GameObject panelPrefab = Resources.Load<GameObject>("PackageXiaoQi/UI/LoadingNotify");
    //     if (panelPrefab == null)
    //     {
    //         Debug.LogError("LoadingNotify预制体未找到！请确保预制体路径为 Resources/PackageXiaoQi/UI/LoadingNotify");
    //     }

    //     // 创建实例
    //     GameObject notifyObject = Instantiate(panelPrefab, GameObject.Find("Canvas").transform);

    //     // 获取组件并设置消息
    //     currentInstance = notifyObject.GetComponent<LoadingPanel>();
    //     currentInstance.SetMessage(message);    
    // }

    // public static void Hide()
    // {
    //     if (currentInstance != null)
    //     {
    //         Destroy(currentInstance.gameObject);
    //         currentInstance = null;
    //     }
    // }

    // private void SetMessage(string message)
    // {
    //     // 设置提示文本
    //     Text textComponent = GetComponentInChildren<Text>();
    //     if (textComponent != null)
    //     {
    //         textComponent.text = message;
    //     }
    // }
    // // 设置加载进度（0-1）
    // public static void SetProgress(float progress)
    // {
    //     if (currentInstance != null)
    //     {
    //         currentInstance.SetProgressInternal(progress);
    //     }
    // }
    // private void SetProgressInternal(float progress)
    // {
    //     targetProgress = Mathf.Clamp01(progress);
    // }

    // private void OnDestroy()
    // {
    //     if (currentInstance == this)
    //     {
    //         currentInstance = null;
    //     }
    // }
    
    private static LoadingPanel currentInstance;
    
    // UI组件引用
    [SerializeField] private Text mainMessageText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private Text progressMessageText;
    [SerializeField] private Image loadingIcon;
    
    // 进度条相关
    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private const float progressSmoothSpeed = 2f;
    
    public static void Show(string message = "加载中...")
    {
        // 如果已有实例，先销毁
        if (currentInstance != null)
        {
            Destroy(currentInstance.gameObject);
        }
        
        // 从资源加载预制体
        GameObject panelPrefab = Resources.Load<GameObject>("PackageXiaoQi/UI/LoadingNotify");
        if (panelPrefab == null)
        {
            Debug.LogError("LoadingNotify预制体未找到！请确保预制体路径为 Resources/PackageXiaoQi/UI/LoadingNotify");
            return;
        }
        
        // 创建实例
        GameObject notifyObject = Instantiate(panelPrefab, GameObject.Find("Canvas").transform);
        
        // 获取组件并设置消息
        currentInstance = notifyObject.GetComponent<LoadingPanel>();      
        currentInstance.SetMainMessageInternal(message);
        currentInstance.SetProgressInternal(0f); // 初始进度为0
    }
    
    public static void Hide()
    {
        if (currentInstance != null)
        {
            Destroy(currentInstance.gameObject);
            currentInstance = null;
        }
    }
    
    // 设置加载消息（静态方法）
    public static void SetMainMessage(string message)
    {
        if (currentInstance != null)
        {
            currentInstance.SetMainMessageInternal(message);
        }
    }
    
    // 设置加载进度（0-1）（静态方法）
    public static void SetProgress(float progress)
    {
        if (currentInstance != null)
        {
            currentInstance.SetProgressInternal(progress);
        }
    }
    
    // 设置加载进度带文本描述（静态方法）
    public static void SetProgress(float progress, string message)
    {
        if (currentInstance != null)
        {
            currentInstance.SetProgressInternal(progress);
            currentInstance.SetProgressMessageInternal(message);
        }
    }

    private void Awake()
    {
        // 确保组件引用正确
        if (mainMessageText == null)
            Debug.LogError("LoadingPanel: mainMessageText 未赋值！");

        if (progressBar == null)
            Debug.LogError("LoadingPanel: progressBar 未赋值！");
        if (progressMessageText == null)
        {
            Debug.LogError("LoadingPanel: progressMessageText 未赋值！");
        }
        if (progressText == null)
        {
            Debug.LogError("LoadingPanel: progressText 未赋值！");
        }
        if (loadingIcon == null)
            Debug.LogError("LoadingPanel: loadingIcon 未赋值！");
    }
    
    private void Update()
    {
        // 平滑更新进度条
        if (currentProgress != targetProgress)
        {
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, progressSmoothSpeed * Time.deltaTime);
            
            // 更新UI
            if (progressBar != null)
                progressBar.value = currentProgress;
                
            if (progressText != null)
                progressText.text = $"{(currentProgress * 100):F0}%";
                
            // 如果非常接近目标值，直接设置为目标值
            if (Mathf.Abs(currentProgress - targetProgress) < 0.01f)
            {
                currentProgress = targetProgress;
            }
        }
        
        // 旋转加载图标（可选）
        if (loadingIcon != null)
        {
            loadingIcon.transform.Rotate(0, 0, -180 * Time.deltaTime);
        }
    }
    
    // 实例方法：设置消息（内部使用）
    private void SetMainMessageInternal(string message)
    {
        if (mainMessageText != null)
        {
            mainMessageText.text = message;
        }
    }
    private void SetProgressMessageInternal(string message)
    {
        if (progressMessageText != null)
        {
            progressMessageText.text = message;
        }
    }
    
    // 实例方法：设置进度（内部使用）
    private void SetProgressInternal(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }
    
    private void OnDestroy()
    {
        if (currentInstance == this)
        {
            currentInstance = null;
        }
    }
}

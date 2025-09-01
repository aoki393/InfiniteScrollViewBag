// ConfigLoader.cs
using System.Collections;
using UnityEngine;

public static class ConfigLoader
{
    public static IEnumerator LoadConfigData<T>(string configName, System.Action<T> onComplete = null) where T : class, new() // 添加 new() 约束
    {
        T configData = default(T);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL平台使用WebGLConfigReader
        var reader = new WebGLConfigReader(configName);
        yield return reader.ReadCoroutine();
        configData = reader.LoadConfig<T>();
#else
        // 其他平台使用JsonMgr
        configData = JsonMgr.Instance.LoadData<T>(configName);
        yield return null; // 保持协程特性
#endif

        onComplete?.Invoke(configData);
        
        // 同时将数据存储在Result属性中
        Result = configData;
    }

    public static object Result { get; private set; }
}
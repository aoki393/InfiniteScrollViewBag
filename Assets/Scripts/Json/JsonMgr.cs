using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 序列化和反序列化Json时  使用的是哪种方案
/// </summary>
public enum JsonType
{
    JsonUtlity,
    LitJson,
}

/// <summary>
/// Json数据管理类 主要用于进行 Json的序列化存储到硬盘 和 反序列化从硬盘中读取到内存中
/// </summary>
public class JsonMgr
{
    private static JsonMgr instance = new JsonMgr();
    public static JsonMgr Instance => instance;

    private JsonMgr() { }

    //存储Json数据 序列化
    public void SaveData(object data, string fileName, JsonType type = JsonType.LitJson)
    {
        //确定存储路径
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        //序列化 得到Json字符串
        string jsonStr = "";
        switch (type)
        {
            case JsonType.JsonUtlity:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.LitJson:
                jsonStr = JsonMapper.ToJson(data);
                break;
        }
        //把序列化的Json字符串 存储到指定路径的文件中
        File.WriteAllText(path, jsonStr);
    }

    //读取指定文件中的 Json数据 反序列化
    public T LoadData<T>(string fileName, JsonType type = JsonType.LitJson) where T : new()
    {
        //确定从哪个路径读取
        //首先先判断 默认数据文件夹中是否有我们想要的数据 如果有 就从中获取
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //先判断 是否存在这个文件
        //如果不存在默认文件 就从 读写文件夹中去寻找
        if (!File.Exists(path))
            path = Application.persistentDataPath + "/" + fileName + ".json";
        //如果读写文件夹中都还没有 那就返回一个默认对象
        if (!File.Exists(path))
            return new T();

        //进行反序列化
        string jsonStr = File.ReadAllText(path);
        //数据对象
        T data = default(T);
        switch (type)
        {
            case JsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.LitJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
        }

        //把对象返回出去
        return data;
    }
    
    // 新增：异步加载方法
    public IEnumerator LoadDataAsync<T>(string fileName, Action<T> onComplete, Action<float> onProgress = null) where T : new()
    {
        string path = GetFilePath(fileName);
        
        if (!File.Exists(path))
        {
            onComplete?.Invoke(new T());
            yield break;
        }

        // 第一阶段：读取文件（异步）
        Task<string> readTask = Task.Run(() =>
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"文件读取错误: {e.Message}");
                return null;
            }
        });

        // 等待文件读取完成，同时更新进度
        float readProgress = 0f;
        while (!readTask.IsCompleted)
        {
            // 模拟读取进度（实际文件读取进度很难获取，这里用时间模拟）
            readProgress = Mathf.Min(readProgress + 0.1f, 0.7f);
            onProgress?.Invoke(readProgress);
            yield return null;
        }

        onProgress?.Invoke(0.7f);
        yield return null;

        string json = readTask.Result;
        if (string.IsNullOrEmpty(json))
        {
            onComplete?.Invoke(new T());
            yield break;
        }

        // 第二阶段：JSON解析（可以在主线程进行）
        T result = default(T);
        bool parseCompleted = false;
        
        // 在小数据量时直接解析，大数据量时分帧解析
        if (json.Length < 10000) // 小于10KB直接解析
        {
            try
            {
                result = JsonMapper.ToObject<T>(json);
                parseCompleted = true;
                onProgress?.Invoke(1f);
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON解析错误: {e.Message}");
                result = new T();
                parseCompleted = true;
            }
        }
        else
        {
            // 大数据量时分帧解析
            yield return ParseJsonInFrames<T>(json, parsedResult => 
            {
                result = parsedResult;
                parseCompleted = true;
            }, progress =>
            {
                float overallProgress = 0.7f + progress * 0.3f;
                onProgress?.Invoke(overallProgress);
            });
        }

        // 等待解析完成
        while (!parseCompleted)
        {
            yield return null;
        }

        onComplete?.Invoke(result);
    }

    // 分帧解析JSON（用于大数据量）
    private IEnumerator ParseJsonInFrames<T>(string json, Action<T> onComplete, Action<float> onProgress = null) where T : new()
    {
        T result = default(T);
        bool isParsing = true;

        // 在后台线程解析
        Thread parseThread = new Thread(() =>
        {
            try
            {
                result = JsonMapper.ToObject<T>(json); // 默认使用LitJson
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON解析错误: {e.Message}");
                result = new T();
            }
            finally
            {
                isParsing = false;
            }
        });

        parseThread.Start();

        // 等待解析完成，同时更新进度
        float startTime = Time.realtimeSinceStartup;
        const float expectedParseTime = 1f; // 预计解析时间1秒

        while (isParsing)
        {
            float elapsed = Time.realtimeSinceStartup - startTime;
            float progress = Mathf.Clamp01(elapsed / expectedParseTime);
            onProgress?.Invoke(progress);
            yield return null;
        }

        parseThread.Join(); // 确保线程完成
        onComplete?.Invoke(result);
    }

    private string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName + ".json");
    }

    // 保存方法（可选异步）
    public IEnumerator SaveDataAsync(object data, string fileName, Action<bool> onComplete = null)
    {
        string json = JsonMapper.ToJson(data);
        string path = GetFilePath(fileName);

        Task<bool> saveTask = Task.Run(() =>
        {
            try
            {
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"文件保存错误: {e.Message}");
                return false;
            }
        });

        while (!saveTask.IsCompleted)
        {
            yield return null;
        }

        onComplete?.Invoke(saveTask.Result);
    }
}

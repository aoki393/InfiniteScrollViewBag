using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

public class WebGLConfigReader : MonoBehaviour
{
    public string PartPath;
    public string ReadResult { get; private set; }
    public WebGLConfigReader(string partPath) { PartPath = partPath; }
    public IEnumerator ReadCoroutine()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, PartPath + ".json");
        UnityWebRequest request = UnityWebRequest.Get(configPath);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            ReadResult = request.downloadHandler.text;
        }
        else
        {
            Debug.LogError("读取配置文件失败: " + request.error);
        }
    }
    public T LoadConfig<T>() where T : class, new()
    {
        // Debug.Log("WebGLConfigReader LoadConfig读取到的配置文件内容：" + ReadResult);
        return JsonMapper.ToObject<T>(ReadResult);
    }
}

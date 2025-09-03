using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageWeaponData
{
    private static PackageWeaponData instance = null;
    private static bool isPreloaded = false;
    public static PackageWeaponData Instance
    {
        get
        {
            if (instance == null)
            {
                if (!isPreloaded)
                {
                    Debug.LogWarning("未进行PackageWeaponData预加载，可能会有卡顿！");
                }
                instance = new PackageWeaponData();
                instance.LoadPackageWeapon(); // 漏了这个WeaponList就会因为没有初始化报错
            }
            return instance;
        }
    }
    public static void Preload()
    {
        if (isPreloaded) return;
        instance = new PackageWeaponData();
        instance.LoadPackageWeapon();
        isPreloaded = true;
    }
    public static IEnumerator PreloadAsync(Action<float> onProgress = null)
    {
        if (isPreloaded) yield break;
        instance = new PackageWeaponData();

        onProgress?.Invoke(0.3f);
        // 分帧加载
        yield return null;

        // instance.LoadPackageWeapon();
        var weaponData = JsonMgr.Instance.LoadData<List<PackageWeaponItem>>("PackageWeaponData");

        onProgress?.Invoke(0.6f);
        yield return null;

        if (weaponData == null)
        {
            instance.WeaponList = new List<PackageWeaponItem>();
            onProgress?.Invoke(1f);
            yield return null;
        }

        instance.WeaponList = weaponData;

        isPreloaded = true;
        onProgress?.Invoke(1f);
        yield return null;


    }
    // 分块加载，卡完了六七秒之后动画才能动，但加载耗时60s+
    public static IEnumerator Preload01Async(Action<float> onProgress = null)
    {
        if (isPreloaded) yield break;

        instance = new PackageWeaponData();
        onProgress?.Invoke(0.1f);
        yield return null;

        // 加载原始数据
        var weaponData = JsonMgr.Instance.LoadData<List<PackageWeaponItem>>("PackageWeaponData");
        onProgress?.Invoke(0.3f);
        yield return null;

        if (weaponData == null)
        {
            instance.WeaponList = new List<PackageWeaponItem>();
            isPreloaded = true;
            onProgress?.Invoke(1f);
            yield break;
        }

        // 分块处理数据
        instance.WeaponList = new List<PackageWeaponItem>();
        int totalCount = weaponData.Count;
        int processed = 0;
        const int chunkSize = 10; // 每帧处理10个物品

        for (int i = 0; i < weaponData.Count; i += chunkSize)
        {
            int endIndex = Mathf.Min(i + chunkSize, weaponData.Count);
            for (int j = i; j < endIndex; j++)
            {
                instance.WeaponList.Add(weaponData[j]);
                processed++;
            }

            // 更新进度
            float progress = 0.3f + 0.7f * (processed / (float)totalCount);
            onProgress?.Invoke(progress);

            // 每处理完一个块就让UI更新一帧
            yield return null;
        }

        isPreloaded = true;
        onProgress?.Invoke(1f);
        yield return null;
    }
    // 基于时间的分帧，总耗时8s，但最后一瞬间从0到70%到100%，0到70%的时候也没有动画效果
    // 终于确定了时间都花在Json加载上了，而且由于是同步加载会把主线程阻塞住
    public static IEnumerator Preload02Async(Action<float> onProgress = null)
    {
        if (isPreloaded) yield break;

        instance = new PackageWeaponData();
        onProgress?.Invoke(0.1f);
        yield return null;

        // 先完成可能耗时的JSON加载
        var weaponData = JsonMgr.Instance.LoadData<List<PackageWeaponItem>>("PackageWeaponData");
        onProgress?.Invoke(0.7f); // JSON加载完成70%进度
        yield return null;

        if (weaponData == null)
        {
            instance.WeaponList = new List<PackageWeaponItem>();
            isPreloaded = true;
            onProgress?.Invoke(1f);
            yield break;
        }

        instance.WeaponList = new List<PackageWeaponItem>(weaponData.Count);
        int totalCount = weaponData.Count;

        // 基于时间的分帧处理 - 每帧最多处理16ms
        float frameTimeBudget = 0.016f; // 16ms per frame
        int index = 0;

        while (index < weaponData.Count)
        {
            float startTime = Time.realtimeSinceStartup;
            int processedThisFrame = 0;

            // 在当前帧预算内处理尽可能多的数据
            while (index < weaponData.Count &&
                   (Time.realtimeSinceStartup - startTime) < frameTimeBudget)
            {
                instance.WeaponList.Add(weaponData[index]);
                index++;
                processedThisFrame++;
            }

            // 更新进度
            float progress = 0.7f + 0.3f * (index / (float)totalCount);
            onProgress?.Invoke(progress);

            yield return null;
        }

        isPreloaded = true;
        onProgress?.Invoke(1f);
        yield return null;
    }
    // 真正的异步，在JsonMgr里加了LoadDataAsyn()，感天动地成功了
    public static IEnumerator Preload03Async(Action<float> onProgress = null)
    {
        if (isPreloaded) yield break;

        instance = new PackageWeaponData();
        onProgress?.Invoke(0.1f);
        yield return null;

        // 使用异步加载
        List<PackageWeaponItem> weaponData = null;
        yield return JsonMgr.Instance.LoadDataAsync<List<PackageWeaponItem>>(
            "PackageWeaponData",
            result => weaponData = result,
            progress => onProgress?.Invoke(0.1f + progress * 0.9f)
        );

        if (weaponData == null)
        {
            instance.WeaponList = new List<PackageWeaponItem>();
        }
        else
        {
            instance.WeaponList = weaponData;
        }

        isPreloaded = true;
        onProgress?.Invoke(1f);
        yield return null;
    }

    public List<PackageWeaponItem> WeaponList;
    private void SavePackageWeapon()
    {
        // JsonMgr.Instance.SaveData(this, "PackageWeaponData"); // 这个会保存整个类，导致Json序列化失败
        JsonMgr.Instance.SaveData(WeaponList, "PackageWeaponData");
    }
    public List<PackageWeaponItem> LoadPackageWeapon()
    {
        WeaponList = JsonMgr.Instance.LoadData<List<PackageWeaponItem>>("PackageWeaponData");
        if (WeaponList == null)
        {
            WeaponList = new List<PackageWeaponItem>();
        }
        return WeaponList;
    }
    // 仅用于调试！！！
    public void ClearPackageWeapon()
    {
        WeaponList.Clear();
        SavePackageWeapon();
    }
    // 添加武器
    public void AddOneWeapon(WeaponConfigItem weapon, WeaponQuality quality)
    {
        PackageWeaponItem item = new PackageWeaponItem();
        item.uid = Guid.NewGuid().GetHashCode();
        item.isEquipped = false; // 初始时武器未穿戴
        item.quality = quality;
        item.WeaponConfig = weapon; // 武器配置信息也存一下吧，也许不必要之后再删

        // 武器实际数值根据品质在一定范围浮动
        // 最好在配置表里定这个规则然后写专门的函数处理，这里简单模拟下
        item.weaponAtk = weapon.atk + UnityEngine.Random.Range(-2 * (int)quality, 3 * (int)quality);
        item.weaponCrit = weapon.crit + UnityEngine.Random.Range(-1 * (int)quality, 2 * (int)quality);

        WeaponList.Add(item);
        SavePackageWeapon();
    }
    public void AddSomeWeapon(List<PackageWeaponItem> weapons)
    {
        WeaponList.AddRange(weapons);
        SavePackageWeapon();
    }
    // 穿戴武器，暂时先不做这个功能
    public void EquipWeapon(int uid)
    {
        PackageWeaponItem item = WeaponList.Find(x => x.uid == uid);
        if (item != null)
        {
            item.isEquipped = true;
            SavePackageWeapon();
        }
    }

}
public class PackageWeaponItem : IPackageItem
{
    // 拥有物品的唯一id，注意与配置id不一样
    public int uid;
    // 是否穿戴
    public bool isEquipped;
    // 武器品质
    public WeaponQuality quality;
    // 配置信息
    public WeaponConfigItem WeaponConfig;
    public int weaponAtk;
    public int weaponCrit;
    // 以下在Package中显示需要
    public string Name => WeaponConfig.name_cn;
    public string Description => WeaponConfig.desc;
    public string IconPath => WeaponConfig.icon_path;
    public ItemType Type => ItemType.Weapon;
    public int ConfigID => WeaponConfig.id;
    override public string ToString() // 仅用于测试时输出到控制台
    {
        return $"PackageWeaponItem: uid: {uid}, 武器名称：{WeaponConfig.name_cn}, 品质：{quality}";
    }
}
public enum WeaponQuality
{
    Common = 1,      // 普通（白装）
    Good = 2,    // 优秀（绿装）
    Fine = 3,        // 精良（蓝装）
    Epic = 4,        // 史诗（紫装）
    Legendary = 5    // 传说（橙装）
}

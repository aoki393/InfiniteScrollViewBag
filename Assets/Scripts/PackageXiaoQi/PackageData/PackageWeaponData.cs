using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageWeaponData
{
    private static PackageWeaponData instance = null;
    public static PackageWeaponData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PackageWeaponData();
                instance.LoadPackageWeapon(); // 漏了这个WeaponList就会因为没有初始化报错
            }
            return instance;
        }
    }
    public List<PackageWeaponItem> WeaponList;
    public void SavePackageWeapon()
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
    public void AddWeapon(WeaponConfigItem weapon,WeaponQuality quality)
    {
        PackageWeaponItem item = new PackageWeaponItem();
        item.uid = Guid.NewGuid().GetHashCode();
        item.isEquipped = false; // 初始时武器未穿戴
        item.quality = quality;
        item.WeaponConfig = weapon; // 武器配置信息也存一下吧，也许不必要之后再删

        // 武器实际数值根据品质在一定范围浮动
        // 最好在配置表里定这个规则然后写专门的函数处理，这里简单模拟下
        item.weaponAtk = weapon.atk+UnityEngine.Random.Range(-2*(int)quality,3*(int)quality);
        item.weaponCrit = weapon.crit+UnityEngine.Random.Range(-1*(int)quality,2*(int)quality);

        WeaponList.Add(item);
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
}
public enum WeaponQuality
{
    Common=1,      // 普通（白装）
    Good=2,    // 优秀（绿装）
    Fine=3,        // 精良（蓝装）
    Epic=4,        // 史诗（紫装）
    Legendary=5    // 传说（橙装）
}

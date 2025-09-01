using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GMcmd
{
    [MenuItem("GMTools/清空/PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("清空PlayerPrefs完成");
    }
    [MenuItem("GMTools/清空/PackageWeaponData")]
    public static void ClearPackageWeaponData()
    {
        PackageWeaponData.Instance.ClearPackageWeapon();
    }
    [MenuItem("GMTools/打印到控制台/Weapon配置")]
    public static void PrintWeaponConfig()
    {
        List<WeaponConfigItem> weapons = JsonMgr.Instance.LoadData<List<WeaponConfigItem>>("tbweapon");
        foreach (WeaponConfigItem weapon in weapons)
        {
            Debug.Log($"[id]:{weapon.id}, " +
            $"[name]:{weapon.name}, " +
            $"[name_cn]:{weapon.name_cn}, " +
            $"[atk]:{weapon.atk}, " +
            $"[crit]:{weapon.crit}, " +
            $"[desc]:{weapon.desc}, " +
            $"[icon_path]:{weapon.icon_path}");
        }
    }
    [MenuItem("GMTools/打印到控制台/Food配置")]
    public static void PrintFoodConfig()
    {
        List<FoodConfigItem> items = JsonMgr.Instance.LoadData<List<FoodConfigItem>>("tbfood");
        foreach (FoodConfigItem item in items)
        {
            Debug.Log(
                $"[id]:{item.id}, " +
                $"[name]:{item.name}, " +
                $"[name_cn]:{item.name_cn}, " +
                $"[hpbuff]:{item.hpbuff}, " +
                $"[mpbuff]:{item.mpbuff}, " +
                $"[desc]:{item.desc}, " +
                $"[icon_path]:{item.icon_path}"
            );
        }
    }
    [MenuItem("GMTools/打印到控制台/背包武器PackageWeaponData数量")]
    public static void PrintPackageWeaponDataCount()
    {
        int count = PackageWeaponData.Instance.WeaponList.Count;
        Debug.Log($"[PackageWeaponData] 武器数量: {count}");
    }
    [MenuItem("GMTools/打印到控制台/背包武器数据PackageWeaponData")]
    public static void PrintPackageWeaponData()
    {
        List<PackageWeaponItem> weapons = PackageWeaponData.Instance.WeaponList;
        if (weapons == null)
        {
            Debug.Log("[PackageWeaponData] 武器列表为空");
            return;
        }
        if (weapons.Count > 0)
        {
            foreach (PackageWeaponItem weapon in weapons)
            {
                Debug.Log($"[uid]: {weapon.uid}," +
                $" [name]: {weapon.WeaponConfig.name_cn}, " +
                $"[quality]: {weapon.quality}, " +
                $"[atk]: {weapon.weaponAtk}, " +
                $"[crit]: {weapon.weaponCrit}");
            }
        }
        else
        {
            Debug.Log("[PackageWeaponData] 武器数量为0");
        }
    }
    [MenuItem("GMTools/打印到控制台/背包食物数据PackageFoodData")]
    public static void PrintPackageFoodData()
    {
        List<PackageFoodItem> foods = PackageFoodData.Instance.FoodList;
        if (foods == null)
        {
            Debug.Log("[PackageFoodData] 食物列表为空");
            return;
        }
        if (foods.Count > 0)
        {
            foreach (PackageFoodItem food in foods)
            {
                Debug.Log($"[id]: {food.id}, " +
                $"[name]: {food.FoodConfig.name_cn}, " +
                $"[count]: {food.count}, " +
                $"[hpbuff]: {food.FoodConfig.hpbuff}, " +
                $"[mpbuff]: {food.FoodConfig.mpbuff}");
            }
        }
        else
        {
            Debug.Log("[PackageFoodData] 食物数量为0");
        }
    }
}

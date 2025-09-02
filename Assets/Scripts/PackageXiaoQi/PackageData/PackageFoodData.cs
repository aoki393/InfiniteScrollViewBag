using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageFoodData
{
    private static PackageFoodData instance = null;
    public static PackageFoodData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PackageFoodData();
                instance.LoadPackageFood(); // Initialize FoodList
            }
            return instance;
        }
    }
    public List<PackageFoodItem> FoodList;

    public void SavePackageFood()
    {
        JsonMgr.Instance.SaveData(FoodList, "PackageFoodData");
    }

    public List<PackageFoodItem> LoadPackageFood()
    {
        FoodList = JsonMgr.Instance.LoadData<List<PackageFoodItem>>("PackageFoodData");
        if (FoodList == null)
        {
            FoodList = new List<PackageFoodItem>();
        }
        return FoodList;
    }

    // For debugging purposes
    public void ClearPackageFood()
    {
        FoodList.Clear();
        SavePackageFood();
    }

    // Add food item
    public void AddFood(FoodConfigItem food, int count = 1)
    {
        // Check if this food already exists in the package
        PackageFoodItem existingItem = FoodList.Find(x => x.id == food.id);
        if (existingItem != null)
        {
            // If exists, just increase the count
            existingItem.count += count;
            // 限制物品最大数量为9999
            if (existingItem.count > 999)
            {
                // 警告
                Debug.LogWarning("物品数量超过 999 ！已限制为 999");
                existingItem.count = 999;
            }
        }
        else
        {
            // If not exists, create new entry
            PackageFoodItem item = new PackageFoodItem();
            item.id = food.id;
            item.count = count;
            item.FoodConfig = food; // Store the config reference

            FoodList.Add(item);
        }
        SavePackageFood();
    }

    // Remove food item
    public void RemoveFood(int id, int count = 1)
    {
        PackageFoodItem item = FoodList.Find(x => x.id == id);
        if (item != null)
        {
            item.count -= count;
            if (item.count <= 0)
            {
                FoodList.Remove(item);
            }
            SavePackageFood();
        }
    }

    // Get count of specific food
    public int GetFoodCount(int id)
    {
        PackageFoodItem item = FoodList.Find(x => x.id == id);
        return item != null ? item.count : 0;
    }
}

public class PackageFoodItem : IPackageItem
{
    // Using the same id as in food config
    public int id;
    // Quantity of this food item
    public int count;
    // Reference to the config data
    public FoodConfigItem FoodConfig;
    // 以下在Package中显示需要
    public int HpBuff => FoodConfig.hpbuff;
    public int MPBuff => FoodConfig.mpbuff;
    public string Name => FoodConfig.name_cn;
    public string Description => FoodConfig.desc;
    public string IconPath => FoodConfig.icon_path;
    public ItemType Type => ItemType.Food;

    public int ConfigID => FoodConfig.id;
    // public List<PackageFoodItem> SplitByMaxCount(int maxCount)
    // {
    //     List<PackageFoodItem> result = new List<PackageFoodItem>();
    //     int remaining = this.count;

    //     while (remaining > 0)
    //     {
    //         int splitCount = Mathf.Min(remaining, maxCount);
    //         var newItem = this.MemberwiseClone() as PackageFoodItem;
    //         newItem.count = splitCount;
    //         result.Add(newItem);
    //         remaining -= splitCount;
    //     }

    //     return result;
    // }
}
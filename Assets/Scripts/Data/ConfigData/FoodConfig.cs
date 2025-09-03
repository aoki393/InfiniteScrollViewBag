using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoodConfig
{
    public List<FoodConfigItem> items = new List<FoodConfigItem>();
}

[System.Serializable]
public class FoodConfigItem
{
    public FoodConfigItem() {} //ConfigLoader的要求
    public int id;
    public string name;
    public string name_cn;
    public int hpbuff;
    public int mpbuff;
    public string desc;
    public string icon_path;
}

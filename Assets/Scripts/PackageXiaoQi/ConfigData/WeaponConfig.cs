using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponConfig
{
    public List<WeaponConfigItem> weapons= new List<WeaponConfigItem>();
}
public class WeaponConfigItem
{
    public WeaponConfigItem() {} //ConfigLoader的要求
    public int id;
    public string name;
    public string name_cn;
    public int atk;
    public int crit;
    public string desc;
    public string icon_path;
}

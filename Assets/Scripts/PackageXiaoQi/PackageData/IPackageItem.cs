using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPackageItem 
{
    string Name { get; }
    string Description { get; }
    string IconPath { get; }
    ItemType Type { get; } // 可选，用于区分物品类型
}

// 可选：物品类型枚举
public enum ItemType
{
    Weapon,
    Food,
    // 其他类型...
}

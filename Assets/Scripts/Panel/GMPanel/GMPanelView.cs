using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMPanelView : MonoBehaviour
{
    // 获取Weapon
    [Header("获取Weapon")]
    public Button btnWeapon;
    public InputField inputWeaponId;
    public Dropdown ddWeaponQuality;

    // 获取Food
    [Header("获取Food")]
    public Button btnFood;
    public InputField inputFoodId;
    public InputField inputFoodNum;

    // 生成测试数据
    [Header("生成测试数据")]
    public Button btnCreateConfirm;
    public Dropdown ddCreateType;
    public InputField WeaponCreateNum;
    public Button btnWeaponCreateEach;
    public Button btnFoodCreateEach;
    // 清空数据
    [Header("清空数据")]
    public Button btnClearWeaponData;
    public Button btnClearFoodData;

    // GM面板消息
    [Header("GM面板消息")]
    public GameObject MsgArea;
    public Text txtMsg; 

}

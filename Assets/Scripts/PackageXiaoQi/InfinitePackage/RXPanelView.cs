using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RXPanelView : MonoBehaviour
{
    // Detail栏
    public Text txtConfigID;
    public Text txtName;
    public Text txtWeaponQulity;
    public Text txtBuff;
    public Text txtDesc;

    // 物品数量显示
    public Text txtTotalNum;

    // 背景显示
    public Transform bgHaveNothing;
    // 物品种类页签（武器/食物……）
    public TabGroup tabGroup;

    // 滚动列表相关
    public RectTransform BagRect;
    public RectTransform contentRect;
    public ScrollRect scrollRect;

    void Awake()
    {
        // 挂载检查:物品数量显示
        if (txtTotalNum == null)
        {
            txtTotalNum = transform.Find("txtTotalNum")?.GetComponent<Text>();
        }
        if (txtTotalNum == null)
        {
            Debug.LogError("txtTotalNum 未挂载且未找到");
        }

        // 挂载检查:Detail栏
        if (txtConfigID == null)
        {
            txtConfigID = transform.Find("Desc/txtConfigID")?.GetComponent<Text>();
        }
        if (txtConfigID == null)
        {
            Debug.LogError("txtConfigID 未挂载且未找到");
        }
        if (txtName == null)
        {
            txtName = transform.Find("Desc/txtName")?.GetComponent<Text>();
        }
        if (txtName == null)
        {
            Debug.LogError("txtName 未挂载且未找到");
        }
        if (txtWeaponQulity == null)
        {
            txtWeaponQulity = transform.Find("Desc/txtWeaponQulity")?.GetComponent<Text>();
        }
        if (txtWeaponQulity == null)
        {
            Debug.LogError("txtWeaponQulity 未挂载且未找到");
        }
        if (txtBuff == null)
        {
            txtBuff = transform.Find("Desc/txtBuff")?.GetComponent<Text>();
        }
        if (txtBuff == null)
        {
            Debug.LogError("txtBuff 未挂载且未找到");
        }
        if (txtDesc == null)
        {
            txtDesc = transform.Find("Desc/DescArea/txtDesc")?.GetComponent<Text>();
        }
        if (txtDesc == null)
        {
            Debug.LogError("txtDesc 未挂载且未找到");
        }

        // 挂载检查:滚动列表相关
        //     路径太长懒得Find了，记得手动挂上
        if (scrollRect == null)
        {
            Debug.LogError("scrollRect 未挂载");
        }
        if (contentRect == null)
        {
            Debug.LogError("contentRect 未挂载");
        }
        if (BagRect == null)
        {
            Debug.LogError("BagRect 未挂载");
        }
    }
}

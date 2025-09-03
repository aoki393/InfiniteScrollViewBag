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
        // 使用工具方法检查所有Text组件
        CheckComponent(ref txtTotalNum, "txtTotalNum");
        CheckComponent(ref txtConfigID, "Desc/txtConfigID");
        CheckComponent(ref txtName, "Desc/txtName");
        CheckComponent(ref txtWeaponQulity, "Desc/txtWeaponQulity");
        CheckComponent(ref txtBuff, "Desc/txtBuff");
        CheckComponent(ref txtDesc, "Desc/DescArea/txtDesc");

        // 检查其他重要组件
        // 挂载检查:滚动列表相关
        //     路径太长懒得Find了，记得手动挂上
        CheckCriticalComponent(scrollRect, "scrollRect");
        CheckCriticalComponent(contentRect, "contentRect");
        CheckCriticalComponent(BagRect, "BagRect");
    }
    // 通用的组件检查方法
    private void CheckComponent(ref Text component, string path)
    {
        if (component == null)
        {
            component = transform.Find(path)?.GetComponent<Text>();
            if (component == null)
            {
                Debug.LogWarning($"{path} 未挂载且未找到对应组件，请确认路径是否正确");
            }
        }
    }

    // 关键组件的严格检查
    private void CheckCriticalComponent<T>(T component, string componentName) where T : Component
    {
        if (component == null)
        {
            Debug.LogError($"{componentName} 未挂载，请在Inspector中手动挂载！");
        }
    }
}

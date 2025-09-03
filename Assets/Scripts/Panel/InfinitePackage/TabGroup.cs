using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    [System.Serializable]
    public class TabButton
    {
        public Button button;
        public GameObject selectedIndicator; // 选中状态图标
        public GameObject activeIcon;       // 激活状态的图标
        public GameObject inactiveIcon;     // 非激活状态的图标        
    }
    public UnityEvent<int> OnTabSelected;
    public UnityEvent<int, PackageItemsSortType> OnSortDropdownValueChanged;
    public TabButton[] tabs;
    public Dropdown[] ddSorts;
    private int selectedIndex = -1;
    public int SelectedIndex
    {
        get { return selectedIndex; }
    }

    void Start()
    {
        if (tabs.Length != ddSorts.Length)
        {
            Debug.LogError("tabs.Length != ddSorts.Length");// 每个tab对应一个dropdown控件
            return;
        }

        // 为每一个tab添加事件监听
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // 闭包问题
            tabs[i].button.onClick.AddListener(() => SelectTab(index));
            
            ddSorts[i].onValueChanged.AddListener((value) => OnSortDropdownValueChanged?.Invoke(index, ddvalue2sortType(index,value)));


        }

        // 默认选择第一个标签
        if (tabs.Length > 0)
        {
            SelectTab(0);
        }
    }

    public void SelectTab(int index)
    {
        // GameMgr.Instance.SetDebugMsg("Clicked the tab "+index);
        if (selectedIndex == index) return;

        // 取消之前选中的标签
        if (selectedIndex >= 0)
        {
            SetTabState(selectedIndex, false);
        }

        // 设置新选中的标签
        selectedIndex = index;
        SetTabState(selectedIndex, true);

        // 可以在这里触发内容切换事件
        OnTabSelected?.Invoke(index);
    }

    private void SetTabState(int index, bool isSelected)
    {
        var tab = tabs[index];
        if (tab.selectedIndicator != null)
            tab.selectedIndicator.SetActive(isSelected);
        if (tab.activeIcon != null)
            tab.activeIcon.SetActive(isSelected);
        if (tab.inactiveIcon != null)
            tab.inactiveIcon.SetActive(!isSelected);
        // 切换dropdown的显示状态
        ddSorts[index].gameObject.SetActive(isSelected);
        ddSorts[index].value = 0; // 重置排序状态显示
    }

    /*
            ddSorts
            武器
            0：最近获取（默认）
            1：品质
            2：配置表ID
            食物
            0：最近获取（默认）
            1：配置表ID
            */
    private PackageItemsSortType ddvalue2sortType(int index, int value) {
        // Weapon
        if (index == 0) {
            if (value == 0) return PackageItemsSortType.Default_AcquireTime;
            if (value == 1) return PackageItemsSortType.WeaponQuality;
            if (value == 2) return PackageItemsSortType.ConfigID;
        }
        // Food
        if (index == 1) {
            if (value == 0) return PackageItemsSortType.Default_AcquireTime;
            if (value == 1) return PackageItemsSortType.ConfigID;
        }
        // 其它
        return PackageItemsSortType.Default_AcquireTime;
    }

}
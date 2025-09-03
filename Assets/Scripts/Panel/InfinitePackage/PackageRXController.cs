using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum PackageItemsSortType
{
    Default_AcquireTime = 0,    // 按配置ID
    WeaponQuality = 1,      // 武器品质
    ConfigID = 2, // 按最新获取
    
}
public class PackageRXController : MonoBehaviour
{
    private static PackageRXController _instance;
    public static PackageRXController Instance => _instance;

    public RXPanelView view; // public 出来给GM调用
    [SerializeField]
    private float cellHeight;
    [SerializeField]
    private float cellWidth;
    [SerializeField]
    private float viewHeight;
    [SerializeField]
    private float viewWidth;

    [SerializeField]
    private float offsetX = 32f;
    [SerializeField]
    private float offsetY = 10f;

    private int row;
    private int column;
    private int maxShowCellNum;
    private List<BagItemVC> showCellItems;
    private List<IPackageItem> currentBagItemlist;
    private int totalRow;
    private int preStartIndex = 0;
    public int totalNum;

    [SerializeField]
    private int _curSelectIndex;    
    public int CurrentSelectIndex => _curSelectIndex;

    // 只有点击和初始页面时会更改 当前选中的Item的Index
    // 每当改变都需要刷新 1、选中状态显示 2、详情栏显示
    public void SetCurrentSelectIndex(int value)
    {
        if (value < 0 || value >= totalNum)
            throw new ArgumentOutOfRangeException(nameof(value));

        _curSelectIndex = value;

        // 点击时更新“选中”状态
        // UpdateCell(showCellItems[curSelectIndex], curSelectIndex);
        UpdateScrollView(true);
        // 点击时更新选中项的详情
        UpdateSelectItemDetailInfo();
    }

    public static void ShowMe()
    {
        if (_instance == null)
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>(UIPanelConst.PanelPaths[UIPanel.PackagePanel_RX]),
                            GameObject.Find("Canvas").transform);
            _instance = obj.GetComponent<PackageRXController>();

            // 确保背包面板显示在第二层，不会把第一层的GMPanel挡住
            _instance.transform.SetSiblingIndex(PanelSiblingIndex.PanelSiblingIndexDict[UIPanel.PackagePanel_RX]);
        }
        
        _instance.gameObject.SetActive(true);
    }
    public static void HideMe()
    {
        _instance.gameObject.SetActive(false);
    }
    /// <summary>
    /// InitXScrollView 检查组件挂载、设定设计好的常量（cell宽高、Viewport宽高）
    ///     因此只需要初始化一次
    /// ContentRect的高根据cell数量动态变化，所以每次刷新都 SetContentHeight
    /// </summary>
    void Awake()
    {
        view = GetComponent<RXPanelView>();
        view.tabGroup.OnTabSelected.AddListener(OnTabSelected);
        view.tabGroup.OnSortDropdownValueChanged.AddListener(OnSortChanged);

        InitXScrollView();
        SetContentHeight(maxShowCellNum);
        InitBagCell(view.contentRect);

        // LoadingPanel.Show("武器数据加载中...");
        // StartCoroutine(LoadingTest());
    }
    // Start is called before the first frame update
    void Start()
    {        
        // 刷新背包列表显示
        RefreshBagIniteView(PackageWeaponData.Instance.WeaponList);
        
    }
    IEnumerator LoadingTest()
    {
        yield return new WaitForSeconds(2);
        LoadingPanel.Hide();
    }

    private void OnSortChanged(int tabIndex, PackageItemsSortType sortType)
    {
        switch (tabIndex)
        {
            case 0:
                RefreshBagIniteView(PackageWeaponData.Instance.WeaponList, sortType);
                break;
            case 1:
                RefreshBagIniteView(PackageFoodData.Instance.FoodList, sortType);
                break;
            default:
                Debug.LogError("OnSortChanged 未处理的tabIndex");
                break;
        }
    }

    private void OnClickItemAction(int index)   // 传到BagItemVC里去Invoke的
    {
        SetCurrentSelectIndex(index);        
    }

    private void OnTabSelected(int arg0)
    {
        switch (arg0)
        {
            case 0:
                RefreshBagIniteView(PackageWeaponData.Instance.WeaponList);
                break;
            case 1:
                RefreshBagIniteView(PackageFoodData.Instance.FoodList);
                break;
        }
    }
    
    // public 出来只是给 GM面板用
    public void RefreshBagIniteView<T>(List<T> items, PackageItemsSortType sortType = PackageItemsSortType.Default_AcquireTime) where T : IPackageItem
    {
        // 先把所有cell隐藏
        for (int i = 0; i < showCellItems.Count; i++)
        {
            showCellItems[i].gameObject.SetActive(false);
        }

        // items 为空时，显示背景图
        if (items == null || items.Count == 0)
        {
            view.bgHaveNothing.gameObject.SetActive(true);           

            view.txtTotalNum.text = "";

            // 清空当前背包列表
            // currentBagItemlist.Clear();  //如果这里currentBagItemlist还没初始化，Clear会报错
            SetContentHeight(0);

            UpdateSelectItemDetailInfo(true);

            return;
        }
        else
        {
            view.bgHaveNothing.gameObject.SetActive(false);
            StringBuilder totalNumStr = new StringBuilder();
            if (items[0] is PackageFoodItem)
            {
                totalNumStr.Append("食物种类总数量: ");
            }
            else if (items[0] is PackageWeaponItem)
            {
                totalNumStr.Append("武器总数量: ");
            }
            view.txtTotalNum.text = totalNumStr.Append(items.Count.ToString()).ToString();
        }

        List<IPackageItem> packageItems = new List<IPackageItem>(items.Cast<IPackageItem>().ToList());
        SortBagItems(packageItems, sortType);
        currentBagItemlist = packageItems;

        SetContentHeight(currentBagItemlist.Count);
        // ShowBagCells(0);

        // 刷新背包显示、Detail栏显示，并将第一个 Cell 设置为“选中”状态        
        SetCurrentSelectIndex(0);

        // StartCoroutine(SelectFirstItemNextFrame());
    }

    private void SortBagItems(List<IPackageItem> packageItems, PackageItemsSortType sortType)
    {
        if (packageItems == null || packageItems.Count == 0)
        {
            throw new ArgumentException("SortBagItems时items为空");
        }

        if (packageItems[0] is PackageWeaponItem)
        {
            // List<PackageWeaponItem> sortWeapons = packageItems.Cast<PackageWeaponItem>().ToList();
            // SortWeapons(sortWeapons, sortType);

            // 上面的Cast转换后 sortWeapons的顺序变化不会改变 packageItems，无法完成排序
            // 必须直接对packageItems排序
            SortWeapons(packageItems, sortType);
        }
        else if (packageItems[0] is PackageFoodItem)
        {
            SortFoods(packageItems, sortType);
        }
        else
        {
            Debug.LogError("SortBagItems 未处理的类型");
        }
    }

    private void SortFoods(List<IPackageItem> foods, PackageItemsSortType sortType)
    {
        switch (sortType)
        {
            case PackageItemsSortType.ConfigID:
                foods.Sort((a, b) => (a as PackageFoodItem).id.CompareTo((b as PackageFoodItem).id));
                break;
            case PackageItemsSortType.Default_AcquireTime:
                // JSON中的顺序即为获取顺序，倒序倒序读取PackageFoodData
                foods.Clear();
                for (int i = PackageFoodData.Instance.FoodList.Count - 1; i >= 0; i--)
                {
                    foods.Add(PackageFoodData.Instance.FoodList[i]);
                }
                break;
            default:
                Debug.LogWarning("Food排序的SortType指定异常！");
                break;
        }
    }

    private void SortWeapons(List<IPackageItem> weapons, PackageItemsSortType sortType)
    {
        switch (sortType)
        {
            case PackageItemsSortType.ConfigID:
                weapons.Sort((a, b) =>
                {
                    int idCompare = (a as PackageWeaponItem).WeaponConfig.id.CompareTo((b as PackageWeaponItem).WeaponConfig.id);
                    return idCompare != 0 ? idCompare
                           : (a as PackageWeaponItem).quality.CompareTo((b as PackageWeaponItem).quality);
                });
                break;
            case PackageItemsSortType.Default_AcquireTime:
                // JSON中的顺序即为获取顺序，倒序读取PackageWeaponData
                weapons.Clear();
                for (int i = PackageWeaponData.Instance.WeaponList.Count - 1; i >= 0; i--)
                {
                    weapons.Add(PackageWeaponData.Instance.WeaponList[i]);
                }
                break;
            case PackageItemsSortType.WeaponQuality:
                weapons.Sort((a, b) =>
                {
                    int qualityCompare = (b as PackageWeaponItem).quality.CompareTo((a as PackageWeaponItem).quality); // 品质降序
                    return qualityCompare != 0 ? qualityCompare
                           : (a as PackageWeaponItem).WeaponConfig.id.CompareTo((b as PackageWeaponItem).WeaponConfig.id);
                });
                break;
            default:
                Debug.LogWarning("Weapon排序的SortType指定异常！");
                break;
        }
    }

    private void ShowBagCells(int startIndex)
    {
        for (int i = 0; i < maxShowCellNum; i++)
        {
            if (i + startIndex >= totalNum)
            {
                showCellItems[i].gameObject.SetActive(false);
            }
            else
            {
                showCellItems[i].UpdateCellInfo(i + startIndex, currentBagItemlist[i + startIndex]);
                UpdateCell(showCellItems[i], startIndex + i);
            }
        }
    }
    private void InitXScrollView()
    {        
        // 设置的可见范围
        // view.BagRect = view.scrollRect.GetComponent<RectTransform>();
        viewHeight = view.BagRect.rect.height;
        viewWidth = view.BagRect.rect.width;

        // 本项目中cell大小设计为统一的100*100
        cellHeight = 100;
        cellWidth = 100;

        // 由可见范围和cell大小、间距 计算 可见范围行数row 和 列数column
        row = Mathf.CeilToInt(viewHeight / (cellHeight + offsetY)) + 1;
        column = Mathf.FloorToInt((viewWidth + offsetX) / (cellWidth + offsetX));
        maxShowCellNum = row * column;

        view.scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

    }
    private void InitBagCell(RectTransform content)
    {
        showCellItems = new List<BagItemVC>(maxShowCellNum);

        for (int i = 0; i < maxShowCellNum; i++)
        {
            GameObject bagItemprefab = Resources.Load<GameObject>(UIPanelConst.PanelPaths[UIPanel.BagItemElement]);
            GameObject bagItem = Instantiate(bagItemprefab, content);
            bagItem.name = $"BagItemCell_{i}";
            var bagItemVC = bagItem.GetComponent<BagItemVC>();

            // 计算并移动到指定位置
            UpdateCell(bagItemVC, i);

            showCellItems.Add(bagItemVC);
            // bagItem.SetActive(false);

            bagItemVC.AddButtonClickListener(OnClickItemAction);
        }
    }
    public void SetContentHeight(int totalNum)
    {
        this.totalNum = totalNum;
        // content中逻辑上应该有的行数
        totalRow = Mathf.CeilToInt((float)totalNum / column);
        // content的高度
        view.contentRect.sizeDelta = new Vector2(view.contentRect.sizeDelta.x, (cellHeight + offsetY) * totalRow);
    }


    private void UpdateCell(BagItemVC bagItemVC, int index)
    {
        bagItemVC.UpdateCellPos(GetCellPos(index));

        // ！！！ 这里更新选中状态
        bagItemVC.UpdateCellSelect(CurrentSelectIndex);
    }

    private void OnScrollValueChanged(Vector2 arg0)
    {
        UpdateScrollView();
    }
    private void UpdateScrollView(bool forceUpdate = false)
    {
        var y = view.contentRect.anchoredPosition.y;
        y = Mathf.Max(0, y);
        var moveRow = Mathf.FloorToInt(y / (cellHeight + offsetY));

        // md下面 moveRow的范围限制一开始写成了下面这样：
        //   (moveRow+row)<totalNum 
        // 导致出了很久都没被发现的显示bug：物品数量大于等于7才能显示

        // 实测 moveRow在ScrollView设置为回弹模式时不需要判断moveRow也行
        // 这个if判断起什么作用没懂
        
        // if (moveRow >= 0 && moveRow < Mathf.Max(totalRow - row, 1))
        {
            var startIndex = moveRow * column;
            if (!forceUpdate && startIndex == preStartIndex)
            {
                return;
            }

            // 刷新BagCells的 数据、显示位置、选中状态
            ShowBagCells(startIndex);

            preStartIndex = startIndex;
        }
    }

    Vector2 GetCellPos(int index)
    {
        var curColumn = index % column;   //当前的列
        var curX = curColumn * (cellWidth + offsetX);    //当前的x坐标

        var curRow = index / column;   //当前的行
        var curY = -curRow * (cellHeight + offsetY); //当前的y坐标

        var pos = new Vector2(curX, curY);
        return pos;
    }

    private void UpdateSelectItemDetailInfo(bool isNothing=false)
    {
        if (isNothing)
        {
            view.txtConfigID.text = "";
            view.txtName.text = "";
            view.txtDesc.text = "";
            view.txtWeaponQulity.text = "";
            view.txtBuff.text = "";
            return;
        }
        // 显示选中的物品ConfigID
        view.txtConfigID.text = "配置表id: "+currentBagItemlist[CurrentSelectIndex].ConfigID.ToString();
        //显示选中的物品Name
        view.txtName.text = currentBagItemlist[CurrentSelectIndex].Name;
        //显示描述
        view.txtDesc.text = currentBagItemlist[CurrentSelectIndex].Description;

        //显示武器 品质、buff
        if (currentBagItemlist[CurrentSelectIndex] is PackageWeaponItem)
        {
            var weapon = (currentBagItemlist[CurrentSelectIndex] as PackageWeaponItem);
            switch (weapon.quality)
            {
                case WeaponQuality.Legendary:
                    view.txtWeaponQulity.text = "传说";
                    view.txtWeaponQulity.color = new Color(255 / 255f, 183 / 255f, 4 / 255f);
                    break;
                case WeaponQuality.Epic:
                    view.txtWeaponQulity.text = "史诗";
                    view.txtWeaponQulity.color = new Color(204 / 255f, 0 / 255f, 255 / 255f);
                    break;
                case WeaponQuality.Fine:
                    view.txtWeaponQulity.text = "精良";
                    view.txtWeaponQulity.color = Color.blue;
                    break;
                case WeaponQuality.Good:
                    view.txtWeaponQulity.text = "优秀";
                    view.txtWeaponQulity.color = new Color(79 / 255f, 203 / 255f, 28 / 255f);
                    break;
                case WeaponQuality.Common:
                    view.txtWeaponQulity.text = "普通";
                    view.txtWeaponQulity.color = Color.gray;
                    break;
            }

            // 显示武器 buff
            view.txtBuff.text = $"Atk: {weapon.weaponAtk}" +
                                $"\nCrit: {weapon.weaponCrit}";
        }
        else
        {
            view.txtWeaponQulity.text = "";
        }

        // 显示食物 buff
        if (currentBagItemlist[CurrentSelectIndex] is PackageFoodItem)
        {
            var food = (PackageFoodItem)currentBagItemlist[CurrentSelectIndex];
            view.txtBuff.text = $"Hp: {food.HpBuff}" +
                                $"\nMp: {food.MPBuff}";
        }
    }
}

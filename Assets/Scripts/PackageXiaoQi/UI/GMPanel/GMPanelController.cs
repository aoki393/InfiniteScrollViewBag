using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMPanelController : MonoBehaviour
{
    private GMPanelView view;
    private static GMPanelController _instance = null;
    public static GMPanelController Instance
    {
        get
        {
            return _instance;
        }
    }
    public static void ShowMe()
    {
        if (_instance == null)
        {
            GameObject res = Resources.Load<GameObject>(UIPanelConst.PanelPaths[UIPanel.GMPanel]);
            GameObject obj = Instantiate(res, GameObject.Find("Canvas").transform, false);

            _instance = obj.GetComponent<GMPanelController>();
            _instance.view = obj.GetComponent<GMPanelView>();
        }
        // 确保GM面板显示在最上层
        _instance.transform.SetSiblingIndex(PanelSiblingIndex.PanelSiblingIndexDict[UIPanel.GMPanel]);
        _instance.gameObject.SetActive(true);
    }
    public static void HideMe()
    {
        if (_instance != null)
        {
            _instance.gameObject.SetActive(false);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        view.btnWeapon.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(view.inputWeaponId.text))
            {
                StartCoroutine(GetWeapon()); // 由于GetWeapon里加载配置文件需要异步进行，所以需要StartCoroutine调用
                Debug.Log("确认获取武器");
            }
        });
        view.btnFood.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(view.inputFoodId.text) && !string.IsNullOrEmpty(view.inputFoodNum.text))
            {
                StartCoroutine(GetFood());
                Debug.Log("确认获取食物");
            }
        });
        view.btnCreateConfirm.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(view.WeaponCreateNum.text))
            {
                // GeneratorWeapon();
                StartCoroutine(CreateSomeWeapons()); // 异步调用
                Debug.Log("确认生成武器");
            }
        });
        view.btnClearWeaponData.onClick.AddListener(() =>
        {
            ClearWeaponData();
            Debug.Log("确认清空武器数据");
        });
        view.btnClearFoodData.onClick.AddListener(() =>
        {
            ClearFoodData();
            Debug.Log("确认清空食物数据");
        });
        view.btnFoodCreateEach.onClick.AddListener(() =>
        {
            StartCoroutine(CreateEachFood()); // 异步调用
            Debug.Log("确认生成食物");
        });
        view.btnWeaponCreateEach.onClick.AddListener(() =>
        {
            StartCoroutine(CreateEachWeapon()); // 异步调用
            Debug.Log("确认生成每种武器");
        });
    }   
    private IEnumerator GetWeapon()
    {
        // List<WeaponConfigItem> weapons = JsonMgr.Instance.LoadData<List<WeaponConfigItem>>("tbweapon");
        // 加载武器配置
        yield return ConfigLoader.LoadConfigData<List<WeaponConfigItem>>("tbweapon");
        List<WeaponConfigItem> weapons = ConfigLoader.Result as List<WeaponConfigItem>;

        if (weapons == null || weapons.Count == 0)
        {
            // tbweapon配置文件为空
            view.txtMsg.text = "tbweapon配置文件读取异常 或 文件内容有误";
            view.MsgArea.SetActive(true);
            // 延迟关闭消息区域
            yield return StartCoroutine(CloseMsgArea());
            yield break;
        }
        int weaponId = int.Parse(view.inputWeaponId.text);
        string qualitySelcted = view.ddWeaponQuality.options[view.ddWeaponQuality.value].text;
        WeaponQuality quality;
        switch (qualitySelcted)
        {
            case "传说":
                quality = WeaponQuality.Legendary;
                break;
            case "史诗":
                quality = WeaponQuality.Epic;
                break;
            case "精良":
                quality = WeaponQuality.Fine;
                break;
            case "优秀":
                quality = WeaponQuality.Good;
                break;
            case "普通":
                quality = WeaponQuality.Common;
                break;
            default:
                Debug.LogWarning("DropDown有问题！选择了无效的武器品质");
                quality = WeaponQuality.Common;
                break;
        }
        foreach (WeaponConfigItem weapon in weapons)
        {
            if (weapon.id == weaponId)
            {
                Debug.Log("武器id有效，名称：" + weapon.name_cn);
                // 将武器存入背包
                PackageWeaponData.Instance.AddWeapon(weapon, quality);
                // 刷新并跳转到背包Weapon视图
                // RefreshView(0);
                RefreshRXView(0);

                // 在GMPanel显示获取信息
                view.txtMsg.text = $"获得了 {qualitySelcted} {weapon.name_cn}（武器）";
                view.MsgArea.SetActive(true);
                // 延迟关闭消息区域
                yield return StartCoroutine(CloseMsgArea());
                yield break;
            }
        }
        // 武器id无效
        view.txtMsg.text = "武器id无效";
        view.MsgArea.SetActive(true);
        // 延迟关闭消息区域
        StartCoroutine(CloseMsgArea());
    }
    private IEnumerator GetFood()
    {
        // List<FoodConfigItem> foods = JsonMgr.Instance.LoadData<List<FoodConfigItem>>("tbfood");
        // 加载食物配置
        yield return ConfigLoader.LoadConfigData<List<FoodConfigItem>>("tbfood");
        var foods = ConfigLoader.Result as List<FoodConfigItem>;

        if (foods == null || foods.Count == 0)
        {
            // tbfood配置文件为空
            view.txtMsg.text = "tbfood配置文件读取异常 或 文件内容有误";
            view.MsgArea.SetActive(true);
            // 延迟关闭消息区域
            yield return StartCoroutine(CloseMsgArea());
            yield break;
        }

        int foodId = int.Parse(view.inputFoodId.text);
        int foodNum = int.Parse(view.inputFoodNum.text);
        if (foodNum > 999)
        {
            view.txtMsg.text = "食物数量不能超过999";
            view.MsgArea.SetActive(true);
            // 延迟关闭消息区域
            yield return StartCoroutine(CloseMsgArea());
            yield break;
        }

        foreach (FoodConfigItem food in foods)
        {
            if (food.id == foodId)
            {
                Debug.Log("食物id有效，名称：" + food.name_cn);
                // 将食物存入背包
                PackageFoodData.Instance.AddFood(food, foodNum);
                // 刷新并跳转到背包Food视图
                // RefreshView(1);
                RefreshRXView(1);

                // 在GMPanel显示获取信息
                view.txtMsg.text = $"获得了 {food.name_cn} {foodNum} 个";
                view.MsgArea.SetActive(true);
                // 延迟关闭消息区域
                yield return StartCoroutine(CloseMsgArea());
                yield break;
            }
        }
        // 道具id无效
        view.txtMsg.text = "食物id无效";
        view.MsgArea.SetActive(true);
        // 延迟关闭消息区域
        StartCoroutine(CloseMsgArea());
    }
    private IEnumerator CreateSomeWeapons()
    {
        // 验证输入数量
        if (!int.TryParse(view.WeaponCreateNum.text, out int weaponNum) || weaponNum <= 0)
        {
            view.txtMsg.text = "武器数量必须为正整数";
            view.MsgArea.SetActive(true);
            yield return StartCoroutine(CloseMsgArea());
            yield break;
        }
        
        // 加载武器配置
        yield return ConfigLoader.LoadConfigData<List<WeaponConfigItem>>("tbweapon");
        var weaponConfigs = ConfigLoader.Result as List<WeaponConfigItem>;

        // 验证配置
        if (weaponConfigs == null || weaponConfigs.Count == 0)
        {
            view.txtMsg.text = "tbweapon配置文件读取异常 或 文件内容有误";
            view.MsgArea.SetActive(true);
            // 延迟关闭消息区域
            yield return StartCoroutine(CloseMsgArea()); // 等待消息关闭
            yield break; // 终止协程          
        }
        // 处理品质选择
        bool randomQuality = false;
        string qualitySelcted = view.ddCreateType.options[view.ddCreateType.value].text;
        WeaponQuality fixedQuality = WeaponQuality.Common;
        switch (qualitySelcted)
        {
            case "随机品质":
                randomQuality = true;
                break;
            case "传说":
                fixedQuality = WeaponQuality.Legendary;
                break;
            case "史诗":
                fixedQuality = WeaponQuality.Epic;
                break;
            case "精良":
                fixedQuality = WeaponQuality.Fine;
                break;
            case "优秀":
                fixedQuality = WeaponQuality.Good;
                break;
            case "普通":
                fixedQuality = WeaponQuality.Common;
                break;
            default:
                Debug.LogWarning("DropDown有问题！选择了无效的武器品质");
                fixedQuality = WeaponQuality.Common;
                break;
        }
        // 生成武器
        for (int i = 0; i < int.Parse(view.WeaponCreateNum.text); i++)
        {
            // 随机选择武器配置
            int configIndex = UnityEngine.Random.Range(0, weaponConfigs.Count);
            WeaponConfigItem config = weaponConfigs[configIndex];

            // 确定品质
            WeaponQuality quality = randomQuality
                ? (WeaponQuality)UnityEngine.Random.Range(1, 6) // 1-5对应枚举值
                : fixedQuality;

            // 添加到背包
            PackageWeaponData.Instance.AddWeapon(config, quality);
        }
        // 刷新并跳转到背包Weapon视图
        // RefreshView(0);
        RefreshRXView(0);

        // 在GMPanel显示获取信息
        view.txtMsg.text = $"成功生成 {view.WeaponCreateNum.text} 把武器";
        view.MsgArea.SetActive(true);
        // 延迟关闭消息区域
        yield return StartCoroutine(CloseMsgArea());
    }
    private void ClearWeaponData()
    {
        PackageWeaponData.Instance.ClearPackageWeapon();
        // 刷新并跳转到背包Weapon视图
        // RefreshView(0);
        RefreshRXView(0);

        view.txtMsg.text = "已清空背包中的武器";
        view.MsgArea.SetActive(true);
        // 延迟关闭消息区域
        StartCoroutine(CloseMsgArea());

        Debug.Log("已清空背包中的武器");
    }
    private void ClearFoodData()
    {
        PackageFoodData.Instance.ClearPackageFood();
        // 刷新并跳转到背包Food视图
        // RefreshView(1);
        RefreshRXView(1);

        view.txtMsg.text = "已清空背包中的食物";
        view.MsgArea.SetActive(true);
        // 延迟关闭消息区域
        StartCoroutine(CloseMsgArea());

        Debug.Log("已清空背包中的食物");
    }
    private IEnumerator CreateEachWeapon()
    {
        // 加载武器配置
        yield return ConfigLoader.LoadConfigData<List<WeaponConfigItem>>("tbweapon");
        var weaponConfigs = ConfigLoader.Result as List<WeaponConfigItem>;
        // 验证配置
        if (weaponConfigs == null || weaponConfigs.Count == 0)
        {
            view.txtMsg.text = "tbweapon配置文件读取异常 或 文件内容有误";
            view.MsgArea.SetActive(true);
            // 延迟关闭消息区域
            yield return StartCoroutine(CloseMsgArea()); // 等待消息关闭
            yield break; // 终止协程          
        }
        // 生成武器
        foreach (WeaponConfigItem weapon in weaponConfigs)
        {
            PackageWeaponData.Instance.AddWeapon(weapon, WeaponQuality.Common);
        }
        // 刷新并跳转到背包Weapon视图
        RefreshRXView(0);

        // 在GMPanel显示获取信息
        view.txtMsg.text = $"成功生成 {weaponConfigs.Count} 把武器";
        view.MsgArea.SetActive(true);
        // 延迟关闭消息区域
        yield return StartCoroutine(CloseMsgArea());
    }
    /*
        食物配置表中每种生成1个
    */
    private IEnumerator CreateEachFood()
    {
        // 加载食物配置
        yield return ConfigLoader.LoadConfigData<List<FoodConfigItem>>("tbfood");
        var foodConfigs = ConfigLoader.Result as List<FoodConfigItem>;
        // 验证配置
        if (foodConfigs == null || foodConfigs.Count == 0)
        {
            view.txtMsg.text = "tbfood配置文件读取异常 或 文件内容有误";
            view.MsgArea.SetActive(true);
            // 延迟关闭消息区域
            yield return StartCoroutine(CloseMsgArea()); // 等待消息关闭
            yield break; // 终止协程          
        }

        // 生成食物
        foreach (FoodConfigItem food in foodConfigs)
        {
            PackageFoodData.Instance.AddFood(food, 1);
        }
        // 刷新并跳转到背包Food视图
        RefreshRXView(1);

        // 在GMPanel显示获取信息
        view.txtMsg.text = $"成功生成 {foodConfigs.Count} 个食物";
        view.MsgArea.SetActive(true);
        // 延迟关闭消息区域
        yield return StartCoroutine(CloseMsgArea());
    }

    private void RefreshRXView(int tabIndex)
    {
        if (tabIndex == 0)
        {
            if (PackageRXController.Instance == null)
            {
                // PackageRXController.ShowMe();
                return;
            }

            if (PackageRXController.Instance.view.tabGroup.SelectedIndex != 0)
            {
                PackageRXController.Instance.view.tabGroup.tabs[0].button.onClick.Invoke();
            }
            else
            {
                PackageRXController.Instance.RefreshBagIniteView(PackageWeaponData.Instance.WeaponList); // 强制刷新并按默认排序
                PackageRXController.Instance.view.tabGroup.ddSorts[0].value = 0; // 重置排序状态显示
            }
        }
        if (tabIndex == 1)
        {
            if (PackageRXController.Instance == null)
            {
                // PackageRXController.ShowMe();
                return;
            }

            if (PackageRXController.Instance.view.tabGroup.SelectedIndex != 1)
            {
                PackageRXController.Instance.view.tabGroup.tabs[1].button.onClick.Invoke();
            }
            else
            {
                PackageRXController.Instance.RefreshBagIniteView(PackageFoodData.Instance.FoodList); // 强制刷新并按默认排序
                PackageRXController.Instance.view.tabGroup.ddSorts[1].value = 0; // 重置排序状态显示
            }
        }
    }

    private IEnumerator CloseMsgArea()
    {
        yield return new WaitForSeconds(3);
        view.MsgArea.SetActive(false);
    }
}


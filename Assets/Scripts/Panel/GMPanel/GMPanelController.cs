using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        // 禁止背包中武器数量超过 114514
        if (PackageWeaponData.Instance.WeaponList.Count >= 114514)
        {
            StartCoroutine(ShowtxtMsg("背包中武器数量不能超过 114514 ！", true));
            yield break;
        }
        // List<WeaponConfigItem> weapons = JsonMgr.Instance.LoadData<List<WeaponConfigItem>>("tbweapon");
        // 加载武器配置
        yield return ConfigLoader.LoadConfigData<List<WeaponConfigItem>>("tbweapon");
        List<WeaponConfigItem> weapons = ConfigLoader.Result as List<WeaponConfigItem>;

        if (weapons == null || weapons.Count == 0)
        {
            StartCoroutine(ShowtxtMsg("tbweapon配置文件读取异常 或 文件内容有误", true));
            yield break;
        }
        int weaponId = int.Parse(view.inputWeaponId.text);

        foreach (WeaponConfigItem weapon in weapons)
        {
            if (weapon.id == weaponId)
            {
                Debug.Log("武器id有效，名称：" + weapon.name_cn);
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
                // 将武器存入背包
                PackageWeaponData.Instance.AddOneWeapon(weapon, quality);
                // 刷新并跳转到背包Weapon视图
                // RefreshView(0);
                RefreshRXView(0);

                // 在GMPanel显示获取信息
                StartCoroutine(ShowtxtMsg($"获得了 {qualitySelcted} {weapon.name_cn}（武器）"));
                yield break;
            }
        }
        // 武器id无效
        StartCoroutine(ShowtxtMsg("武器id无效", true));
    }
    private IEnumerator GetFood()
    {
        // 检查输入数量
        if (!int.TryParse(view.inputFoodNum.text, out int foodNum) || foodNum <= 0)
        {
            StartCoroutine(ShowtxtMsg("食物数量必须为正整数！",true));
            yield break;
        }
        // List<FoodConfigItem> foods = JsonMgr.Instance.LoadData<List<FoodConfigItem>>("tbfood");
        // 加载食物配置
        yield return ConfigLoader.LoadConfigData<List<FoodConfigItem>>("tbfood");
        var foods = ConfigLoader.Result as List<FoodConfigItem>;

        if (foods == null || foods.Count == 0)
        {
            // tbfood配置文件为空
            StartCoroutine(ShowtxtMsg("tbfood配置文件读取异常 或 文件内容有误", true));
            yield break;
        }

        int foodId = int.Parse(view.inputFoodId.text);

        foreach (FoodConfigItem food in foods)
        {
            if (food.id == foodId)
            {
                Debug.Log("食物id有效，名称：" + food.name_cn);

                // 检查该食物的已有数量（限制该食物的数量不能超过999个
                var foodExistCount = PackageFoodData.Instance.FoodList.Find(x => x.id == foodId)?.count ?? 0;
                if (foodExistCount+foodNum > 999)
                {
                    StartCoroutine(ShowtxtMsg("背包中一种食物的数量不能超过999个", true));
                    yield break;
                }

                // 将食物存入背包
                PackageFoodData.Instance.AddFood(food, foodNum);
                // 刷新并跳转到背包Food视图
                // RefreshView(1);
                RefreshRXView(1);

                // 在GMPanel显示获取信息
                yield return StartCoroutine(ShowtxtMsg($"获得了 {food.name_cn} {foodNum} 个"));
                yield break;
            }
        }
        // 食物id无效
        yield return StartCoroutine(ShowtxtMsg("食物id无效", true));

    }
    public IEnumerator CreateSomeWeapons()
    {
        // 验证输入数量
        if (!int.TryParse(view.WeaponCreateNum.text, out int weaponNum) || weaponNum <= 0)
        {
            StartCoroutine(ShowtxtMsg("武器数量必须为正整数！",true));
            yield break;
        }
        // 禁止背包中武器数量超过 114514
        if (weaponNum + PackageWeaponData.Instance.WeaponList.Count >= 114515)
        {
            StartCoroutine(ShowtxtMsg("背包中武器数量不能超过 114514 ！",true));
            yield break;
        }
        // 加载武器配置
        yield return ConfigLoader.LoadConfigData<List<WeaponConfigItem>>("tbweapon");
        var weaponConfigs = ConfigLoader.Result as List<WeaponConfigItem>;
        // 验证配置
        if (weaponConfigs == null || weaponConfigs.Count == 0)
        {
            StartCoroutine(ShowtxtMsg("tbweapon配置文件读取异常 或 文件内容有误", true));
            yield break; // 终止协程          
        }

        // 处理品质选择
        bool randomQuality = false;
        string qualitySelected = view.ddCreateType.options[view.ddCreateType.value].text;
        WeaponQuality fixedQuality = WeaponQuality.Common;
        switch (qualitySelected)
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
        }

        // 生成武器

        var weaponsToAdd = new List<PackageWeaponItem>();

        for (int i = 0; i < weaponNum; i++)
        {
            int configIndex = UnityEngine.Random.Range(0, weaponConfigs.Count);
            WeaponConfigItem config = weaponConfigs[configIndex];

            WeaponQuality quality = randomQuality
                ? (WeaponQuality)UnityEngine.Random.Range(1, 6)
                : fixedQuality;

            // 先收集到临时列表，不立即保存
            PackageWeaponItem item = new PackageWeaponItem
            {
                uid = Guid.NewGuid().GetHashCode(),
                isEquipped = false,
                quality = quality,
                WeaponConfig = config,
                weaponAtk = config.atk + UnityEngine.Random.Range(-2 * (int)quality, 3 * (int)quality),
                weaponCrit = config.crit + UnityEngine.Random.Range(-1 * (int)quality, 2 * (int)quality)
            };

            weaponsToAdd.Add(item);

            // 每1000个 yield 一下避免卡死
            if ((i + 1) % 1000 == 0)
            {
                Debug.Log($"已生成 {i + 1} 把武器");
                yield return null;
            }
        }

        // 批量添加到数据层
        PackageWeaponData.Instance.AddSomeWeapon(weaponsToAdd);
        Debug.Log($"批量生成 {weaponNum} 把武器完成");

        RefreshRXView(0);
        StartCoroutine(ShowtxtMsg($"成功批量生成{weaponNum}把武器！"));
    }
    private void ClearWeaponData()
    {
        PackageWeaponData.Instance.ClearPackageWeapon();
        // 刷新并跳转到背包Weapon视图
        // RefreshView(0);
        RefreshRXView(0);

        StartCoroutine(ShowtxtMsg("已清空背包中的武器"));
    }
    private void ClearFoodData()
    {
        PackageFoodData.Instance.ClearPackageFood();
        // 刷新并跳转到背包Food视图
        // RefreshView(1);
        RefreshRXView(1);

        StartCoroutine(ShowtxtMsg("已清空背包中的食物"));
    }
    private IEnumerator CreateEachWeapon()
    {
        // 加载武器配置
        yield return ConfigLoader.LoadConfigData<List<WeaponConfigItem>>("tbweapon");
        var weaponConfigs = ConfigLoader.Result as List<WeaponConfigItem>;
        // 验证配置
        if (weaponConfigs == null || weaponConfigs.Count == 0)
        {
            StartCoroutine(ShowtxtMsg("tbweapon配置文件读取异常 或 文件内容有误", true));
            yield break; // 终止协程          
        }
        // 生成武器
        foreach (WeaponConfigItem weapon in weaponConfigs)
        {
            // 检查背包中武器数量是否会超过 114514 
            if (PackageWeaponData.Instance.WeaponList.Count + 20 > 114514) // 读配置表数量会有点麻烦，直接硬编码了，目前配置里一共20种weapon
            {
                StartCoroutine(ShowtxtMsg("背包中武器数量不能超过 114514 ！", true));
                yield break;
            }
            PackageWeaponData.Instance.AddOneWeapon(weapon, WeaponQuality.Common);
        }
        // 刷新并跳转到背包Weapon视图
        RefreshRXView(0);

        StartCoroutine(ShowtxtMsg($"成功生成 {weaponConfigs.Count} 把武器"));
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
            StartCoroutine(ShowtxtMsg("tbfood配置文件读取异常 或 文件内容有误！", true));
            yield break; // 终止协程          
        }

        // 检查背包中是否有食物数量达到 999 
        foreach (FoodConfigItem food in foodConfigs)
        {
            // 检查该食物的已有数量（限制该食物的数量不能超过999个
            // var foodExistCount = PackageFoodData.Instance.FoodList.Find(x => x.id == food.id)?.count ?? 0;
            // 等效于以下传统写法
            var hasfood = PackageFoodData.Instance.FoodList.Find(x => x.id == food.id);
            var foodExistCount = (hasfood != null) ? hasfood.count : 0;
            if (foodExistCount >= 999)
            {
                StartCoroutine(ShowtxtMsg($"{food.name_cn} 数量已达到999个！",true));
                yield break;
            }
        }

        // 生成食物
        foreach (FoodConfigItem food in foodConfigs)
        {
            PackageFoodData.Instance.AddFood(food, 1);
        }
        // 刷新并跳转到背包Food视图
        RefreshRXView(1);

        // 在GMPanel显示获取信息
        StartCoroutine(ShowtxtMsg($"成功生成 {foodConfigs.Count} 个食物"));
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
    IEnumerator ShowtxtMsg(string msg, bool isWarning = false)
    {
        view.txtMsg.text = msg;
        if (isWarning)
        {
            view.txtMsg.color = Color.red;
        }
        else
        {
            view.txtMsg.color = Color.white;
        }
        view.MsgArea.SetActive(true);
        yield return StartCoroutine(CloseMsgArea());
    }
}


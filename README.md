# InfiniteScrollViewBag

背包demo练习，自用  
[itch在线演示（webGL）](https://aoki393.itch.io/packagedemo)  

>参考的教程  
[小棋的游戏背包系统讲解](https://www.bilibili.com/video/BV1cw411B7Z4/)  
[UGUI无限滚动列表](https://www.bilibili.com/video/BV1u2uCzVEwc/)

*仅参考基本框架和原理，代码已经和教程里的很不一样了

## 项目代码结构

```text
📦 Scripts
├── 📂 Data
│   ├── 📂 ConfigData
│   │   ├── 🍎 FoodConfig.cs
│   │   └── ⚔️ WeaponConfig.cs
│   └── 📂 PackageData
│       ├── 📄 IPackageItem.cs
│       ├── 🍔 PackageFoodData.cs
│       └── 🔫 PackageWeaponData.cs
├── 📂 Json                             # Json 管理与解析
│   ├── 🗂️ ConfigLoader.cs
│   ├── 🗂️ JsonMgr.cs
│   ├── 🗂️ WebGLConfigReader.cs
│   └── 📂 LitJson                        # LitJson 库相关文件
├── 📂 Panel
│   ├── 🖥️ LoadingPanel.cs
│   ├── 🖥️ MainPanel.cs
│   ├── 📂 GMPanel
│   │   ├── 🎮 GMPanelController.cs
│   │   └── 👀 GMPanelView.cs
│   └── 📂 InfinitePackage
│       ├── 🎮 PackageRXController.cs
│       ├── 👀 RXPanelView.cs
│       ├── 🎒 BagItemVC.cs
│       └── 📑 TabGroup.cs
├── 🔢 PanelSiblingIndex.cs             # Panel 层级索引相关
├── ⚙️ UIPanelConst.cs                  # UI Panel 常量定义
└── 👨‍💼 GameMgr.cs                       # 游戏主管理器
```

UML图
![alt text](deepseek_mermaid_20250902_44ebdd.png)
（绘图绘制：DeepSeek

数据流向

```text
配置文件 → 配置类 → 数据管理器 → 背包物品 → UI显示
(JSON)   (ConfigItem) (PackageData) (PackageItem) (BagItemVC)
```

其中JSON配置文件使用 [Luban](https://www.datable.cn/) 由Excel配置表生成

## 核心架构模式

**MVC模式:**  
控制器(PackageRXController, GMPanelController)  
+  
视图(RXPanelView, GMPanelView)  
+  
数据模型(PackageWeaponData, PackageFoodData)

**单例模式+观察者模式:**  
关键管理器类使用单例模式确保全局唯一访问  
通过UnityEvent实现事件订阅和发布机制

## 主要模块划分

**🎮 核心管理模块 (GameMgr)**  
游戏入口和总控制器  
管理FPS显示、动画状态、音效控制  
提供全局的背包和GM面板开关功能

**🎒 背包系统模块 (PackageRX)**  
PackageRXController: 背包核心逻辑，处理物品显示、排序、选择  
RXPanelView: 背包UI组件引用管理  
BagItemVC: 单个背包物品的视图控制器  
TabGroup: 标签页管理，支持武器/食物分类和排序

**🛠️ GM工具模块 (GMPanel)**  
GMPanelController: GM命令处理，物品生成和清除  
GMPanelView: GM面板UI组件管理  
支持按ID获取物品、批量生成、数据清除等功能

**📊 数据管理模块**  
配置数据: WeaponConfig/FoodConfig 定义物品基础属性  
背包数据: PackageWeaponData/PackageFoodData 管理玩家拥有的物品  
接口设计: IPackageItem 统一物品接口，支持多态处理  

**🗄️Json管理与解析模块**  
JsonMgr：单例，负责序列化和反序列化  
Json.ConfigLoader：配置文件加载器，支持异步加载和分帧解析。  
Json.LitJson：第三方JSON解析库

## 遇到的问题与解决

### 1、大量数据加载卡顿  

**虚拟列表技术**  
通过“少量 UI 复用 + 滚动时动态赋值”实现了大数据量背包的高效显示和操作

- 背包物品数量与显示区域分离  
  - 背包可能有成千上万个物品，但实际显示区域（ScrollView）只需要渲染可见的那几十个（如 maxShowCellNum）。  
- 滚动时动态复用 Cell  
  - 每次滚动时不会新建 UI，只是把已有的 Cell 重新赋值和定位。  
  - UpdateScrollView() 根据滚动位置计算当前应显示的物品索引区间（startIndex），然后调用 ShowBagCells(startIndex)。  
  - ShowBagCells 只更新可见的 Cell 内容和位置，隐藏多余的 Cell。
- Cell 位置和数据动态更新  
  - GetCellPos(index) 计算每个 Cell 的坐标，实现网格布局。
  - UpdateCell 更新 Cell 的选中状态和显示内容。

### 2、物品生成耗时过长

一开始的代码生成11w条数据的耗时达到3h以上  

**原因：**

``` C#
// PackageWeaponData.AddWeapon 的调用链
AddWeapon() → WeaponList.Add() → SavePackageWeapon() → Json序列化 → 文件写入
// 每次AddWeapon都触发完整保存！11万次文件IO！
```

**解决方案：**  
`PackageWeaponData`中增加批量添加武器到`WeaponList`的方法,只进行一次序列化和文件写入

| 指标 |优化前 | 优化后 |
| --- | --- | --- |
| 总耗时 | 3小时 | 3~5秒 |  
| 序列化次数 | 11万次 | 1次 |
| 文件写入次数 | 11万次 | 1次 |  
| 内存分配 | 频繁GC | 一次分配 |

## 关键技术点

- 异步加载: 使用协程处理配置文件的异步加载  
- 动态滚动: 实现虚拟列表优化，支持大量物品的高效显示  
- 多态处理: 通过IPackageItem接口统一处理武器和食物物品  
- 事件驱动: 使用UnityEvent实现组件间解耦通信  

## 后续优化  

- 增加删除功能：
  - 选中武器删除、多选武器批量删除
  - 选中食物删除指定数量（可能需要做一个包含数量输入框弹窗）

- GM面板生成过多武器（10w+）时会卡5s左右
  - 尝试在MsgArea加入加载进行icon提示或进度条
- 武器数量过多时再增加新武器会有几秒卡顿
  - List结构导致的
  - 可能的优化方案：使用字典索引 + 分页加载 + 异步操作
- 增加音效
  - 切换武器/食物页的音效
  - 点击选中武器/食物的音效
  - GM面板音效
  - 删除音效
- 弃用Resources资源加载方案
  - 改为Addressables资源加载

- 看看UI是否需要打图集，减少Batches数量
- 武器数量过多时进度条加载到99%会停一段时间
  - 先找出问题所在！

**暂不修复**  
获取食物时并不会把最近获取的食物放到顶部（当该食物已存在于背包中时）

- 因为代码仅进行了count数量的变化，没有调整json数据的排序
- fix思路：将该食物原有数量记下，删除该食物，再添加该食物，加上原有数量

# InfiniteScrollViewBag
背包demo练习

# 项目代码结构
```
📦 Scripts
├── 📂 Json
├── 📂 ConfigData
│   ├── 🍎 FoodConfig.cs
│   └── ⚔️ WeaponConfig.cs
├── 📂 PackageData
│   ├── 📄 IPackageItem.cs
│   ├── 🍔 PackageFoodData.cs
│   └── 🔫 PackageWeaponData.cs
├── 📂 InfinitePackage
│   ├── 🎮 PackageRXController.cs
│   └── 👀 RXPanelView.cs
│   ├── 🎒 BagItemVC.cs
│   └── 📑 TabGroup.cs
├── 📂 GMPanel
│   ├── 🎮 GMPanelController.cs
│   └── 👀 GMPanelView.cs
├── 🖥️ MainPanel.cs
├── 🔢 PanelSiblingIndex.cs
├── ⚙️ UIPanelConst.cs
└── 👨‍💼 GameMgr.cs
```
![alt text](deepseek_mermaid_20250902_44ebdd.png)
**数据流向**
```
配置文件 → 配置类 → 数据管理器 → 背包物品 → UI显示
(JSON)   (ConfigItem) (PackageData) (PackageItem) (BagItemVC)
```
# 核心架构模式
**MVC模式:**   
控制器(PackageRXController, GMPanelController)  
+  
视图(RXPanelView, GMPanelView)  
+  
数据模型(PackageWeaponData, PackageFoodData)

**单例模式+观察者模式:**  
关键管理器类使用单例模式确保全局唯一访问  
 通过UnityEvent实现事件订阅和发布机制

# 主要模块划分
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

# 关键技术点
- 泛型编程: RefreshBagIniteView<T> 方法支持不同类型的物品列表  
- 异步加载: 使用协程处理配置文件的异步加载  
- 动态滚动: 实现虚拟列表优化，支持大量物品的高效显示  
- 多态处理: 通过IPackageItem接口统一处理武器和食物物品  
- 事件驱动: 使用UnityEvent实现组件间解耦通信  
# 扩展性设计
- 配置化: 所有物品属性通过JSON配置文件管理  
- 模块化: 各功能模块高度独立，便于扩展新物品类型    
- 接口化: 通过接口定义契约，支持未来添加新功能


# 总结
这个架构设计体现了良好的软件工程原则，包括单一职责、开闭原则和依赖倒置原则，为后续功能扩展和维护提供了良好的基础。

# 潜在问题
1、我创建11w个武器，持久化的json文件大小达到了11MB，这个项目中使用PackageWeaponData单例存下了整整11w条武器数据，游戏能正常运行吗？  

对于中等配置电脑：  
✅ 15MB内存占用完全可以接受（只占0.24%）  
❌ 但11万数据的列表操作性能是致命问题  
✅ 需要优化数据结构和使用方式  
✅ 建议使用字典索引 + 分页加载 + 异步操作  

2、一般的RPG游戏在中等配置电脑中运行时会占用多少内存？  
![alt text](image.png)  

3、列表操作和序列化的开销具体会有多大影响  
你的11万数据量会导致：  
❌ 保存游戏：2秒卡顿（玩家无法接受）  
❌ 加载游戏：1.2秒卡顿（加载体验差）  
❌ 排序操作：100ms卡顿（界面卡顿）  
✅ 单个查找：3ms（勉强可用但需优化）  

4、为什么生成11w条数据的耗时达到3h以上？  
根本原因：  
❌ 每次AddWeapon都触发完整序列化  
❌ 每次AddWeapon都进行文件写入  
❌ 11万次不必要的IO操作
``` C#
// PackageWeaponData.AddWeapon 的调用链
AddWeapon() → WeaponList.Add() → SavePackageWeapon() → Json序列化 → 文件写入
// 每次AddWeapon都触发完整保存！11万次文件IO！
```
优化方案：批量处理模式（最重要！）、优化保存逻辑  

| 指标 |优化前 | 优化后 | 提升倍数 |
| --- | --- | --- | --- |
| 总耗时 | 3小时 | 2-3秒 | 3600倍 |
| 序列化次数 | 11万次 | 1次 | 11万倍 |
| 文件写入次数 | 11万次 | 1次 | 11万倍 |
| 内存分配 | 频繁GC | 一次分配 | 显著改善 |


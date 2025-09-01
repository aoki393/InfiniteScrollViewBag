using System.Collections.Generic;

public class UIPanelConst
{
    // 使用 Dictionary<UIPanelType, string> 存储配置
    public static readonly Dictionary<UIPanel, string> PanelPaths = new()
    {
        { UIPanel.MainPanel, "PackageXiaoQi/UI/MainPanel" },
        { UIPanel.PackagePanel, "PackageXiaoQi/UI/PackagePanel" },
        { UIPanel.PackagePanel_R, "PackageXiaoQi/UI/PackagePanel_R" },
        { UIPanel.PackagePanel_RX, "PackageXiaoQi/UI/PackagePanel_RX" },
        { UIPanel.BagItemElement, "PackageXiaoQi/UI/PackagePanel/BagItemElement" },
        { UIPanel.LotteryPanel, "PackageXiaoQi/UI/LotteryPanel" },
        { UIPanel.GMPanel, "PackageXiaoQi/UI/GMPanel" }
    };
}
public enum UIPanel
{
    MainPanel,
    PackagePanel,
    PackagePanel_R,
    PackagePanel_RX,
    BagItemElement,
    LotteryPanel,
    GMPanel
}

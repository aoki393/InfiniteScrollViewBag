using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSiblingIndex
{
    public static readonly Dictionary<UIPanel, int> PanelSiblingIndexDict = new()
    {
        { UIPanel.GMPanel, 1 },
        { UIPanel.PackagePanel_R, 0 },
        { UIPanel.PackagePanel_RX, 0 },

    };
}

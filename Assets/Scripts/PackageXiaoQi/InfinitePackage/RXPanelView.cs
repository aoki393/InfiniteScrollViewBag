using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RXPanelView : MonoBehaviour
{
    public ScrollRect scrollRect;

    // Detail
    public Text txtName;
    public Text txtWeaponQulity;
    public Text txtBuff;
    public Text txtDesc;

    public Transform bgHaveNothing;
    public TabGroup tabGroup;

    public RectTransform BagRect;
    public RectTransform contentRect;
}

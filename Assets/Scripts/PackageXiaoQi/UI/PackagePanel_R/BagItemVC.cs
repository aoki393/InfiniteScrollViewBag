using System;
using System.Collections;
using System.Collections.Generic;
// using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagItemVC : MonoBehaviour//, IPointerClickHandler
{
    public Button btnItem;
    public Transform Selected;
    public UnityEngine.UI.Image icon;
    public Text count;
    public Transform isEquipped;
    [HideInInspector]
    public PackageWeaponItem weaponItem;
    [HideInInspector]
    public PackageFoodItem foodItem;

    [SerializeField]
    private int index;
    public UnityAction<int> btnClickAction;
    public void Refresh(IPackageItem item)
    {
        if (item == null) return;

        // 统一处理
        // Debug.Log($"Refresh 的 [IconPath]:{item.IconPath}");

        Texture2D texture = (Texture2D)Resources.Load(item.IconPath);
        if (texture == null)
        {
            Debug.LogError($"IconPath 不存在: {item.IconPath}");
            return;
        }
        Sprite iconSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        icon.sprite = iconSprite;

        // 特殊处理
        if (item is PackageWeaponItem weapon)
        {
            this.weaponItem = weapon;
            this.foodItem = null; // 清除foodItem引用
            isEquipped.gameObject.SetActive(weapon.isEquipped);
            count.gameObject.SetActive(false); // 武器不显示数量
        }
        else if (item is PackageFoodItem food)
        {
            this.foodItem = food;
            this.weaponItem = null; // 清除weaponItem引用
                                    // 显示数量，但不超过99
            int displayCount = Mathf.Min(food.count, 99);
            count.text = displayCount.ToString();
            count.gameObject.SetActive(displayCount > 0);
        }

        gameObject.SetActive(true);
    }
    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     PackagePanelController.Instance.ChooseItem = this;
    // }

    // Start is called before the first frame update
    void Start()
    {
        if (btnItem == null)
        {
            btnItem = transform.GetComponent<Button>();
        }

        btnItem.onClick.AddListener(() =>
        {
            // PackagePanelController.Instance.ChooseItem = this;
            btnClickAction?.Invoke(index);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void AddButtonClickListener(UnityAction<int> btnClick)
    {
        btnClickAction += btnClick;
    }
    public void UpdateCellPos(Vector2 pos)
    {
        GetComponent<RectTransform>().anchoredPosition = pos;
    }
    public void UpdateCellInfo(int _index, IPackageItem item)
    {
        index = _index;
        Refresh(item);
    }    
    public void UpdateCellSelect(int curSelectIndex)
    {
        Selected.gameObject.SetActive(index == curSelectIndex); // Selected 选中框显示
    }
}

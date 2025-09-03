using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{    public Button btnOpenBag;
    public Button btnGM;
    public Button btnLoading;
    private static MainPanel _instance = null;
    public static MainPanel Instance
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
            GameObject prefab = Resources.Load<GameObject>(UIPanelConst.PanelPaths[UIPanel.MainPanel]);
            _instance = Instantiate(prefab, GameObject.Find("Canvas").transform, false).GetComponent<MainPanel>();
        }
        _instance.gameObject.SetActive(true);
    }
    public static void HideMe()
    {
        if (_instance != null)
        {
            _instance.gameObject.SetActive(false);
        }
    }
    void Start()
    {
        btnOpenBag.onClick.AddListener(() =>
        {
            GameMgr.Instance.OpenClosePackageRX();
        });
        btnGM.onClick.AddListener(() =>
        {
            GameMgr.Instance.OpenCloseGMPanel();
        });
        btnLoading.onClick.AddListener(() =>
        {            
            var data = PackageWeaponData.Instance;
            if (data.WeaponList.Count > 0)
            {
                Debug.Log(data.WeaponList[0].ToString());
            }
            else
            {
                Debug.Log("武器列表为空");
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

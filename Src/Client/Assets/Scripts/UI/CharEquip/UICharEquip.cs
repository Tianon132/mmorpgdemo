using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Managers;
using Models;
using System;
using SkillBridge.Message;

public class UICharEquip : UIWindow {

	public Text title;
	public Text money;

	public GameObject itemPrefab;//左侧装备栏即仓库的所有装备-滑动UI
	public GameObject itemEquipedPrefab;//中间要装备在角色身上的的装备-正方形UI

	public Transform itemListRoot;//角色 仓库装备 根节点

	public List<Transform> slots;//角色身上 的 多个装备槽位

	void Start()
    {
		RefreshUI();
		EquipManager.Instance.OnEquipChanged += RefreshUI;//实例化之后，就要去Manager去注册事件，只要装备系统发生变化，穿或脱都要刷新一下
    }

	private void OnDestroy()
    {
		EquipManager.Instance.OnEquipChanged -= RefreshUI;
    }

	void RefreshUI()//刷新UI
    {
		ClearAllEquipList();
		InitAllEquipItems();
		ClearEquipedList();
		InitEquipedItems();
		this.money.text = User.Instance.CurrentCharacter.Gold.ToString();//金币
	}

    /// <summary>
    /// 初始化所有装备列表
    /// </summary>
    void InitAllEquipItems()
    {
        foreach(var kv in ItemManager.Instance.Items)
        {
            if(kv.Value.Define.Type == ItemType.Equip)
            {
                //已经装备就不显示了
                if (EquipManager.Instance.Contains(kv.Key))
                    continue;
                //加 装备
                GameObject go = Instantiate(itemPrefab, itemListRoot);
                UIEquipItem ui = go.GetComponent<UIEquipItem>();
                ui.SetEquipItem(kv.Key, kv.Value, this, false);
            }
        }
    }

    /// <summary>
    /// 初始化已经装备的列表
    /// </summary>
    void InitEquipedItems()
    {
        for(int i = 0; i < (int)EquipSlot.SlotMax; i++)
        {
            var item = EquipManager.Instance.Equips[i];
            if(item != null)
            {
                GameObject go = Instantiate(itemEquipedPrefab, slots[i]);
                UIEquipItem ui = go.GetComponent<UIEquipItem>();
                ui.SetEquipItem(i, item, this, true);
            }
        }
    }

    //清楚所有装备列表
    void ClearAllEquipList()
    {
        foreach (var item in itemListRoot.GetComponentsInChildren<UIEquipItem>())
        {
            Destroy(item.gameObject);
        }
    }

    //清除已装备列表
    void ClearEquipedList()
    {
        foreach (var item in slots)
        {
            if (item.childCount > 0)
                Destroy(item.GetChild(0).gameObject);
        }
    }

    public void DoEquip(Item item)//穿装备
    {
        EquipManager.Instance.EquipItem(item);
    }

    public void UnEquip(Item item)//托装备
    {
        EquipManager.Instance.UnEquipItem(item);
    }

}

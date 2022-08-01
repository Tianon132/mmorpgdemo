using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Managers;
using Models;

public class UIBag : UIWindow {

	public Text money;

	public Transform[] pages;//几页，一般两页

	public GameObject bagItem;//一个格子放什么图标

	List<Image> slots;//槽，表示有几个格子，在Start里得到

	// Use this for initialization
	void Start () {
		if(slots == null)
        {
			slots = new List<Image>();
			for(int page = 0; page <this.pages.Length; page++)//2页
            {
				slots.AddRange(this.pages[page].GetComponentsInChildren<Image>(true));//得到有多少个曹
            }
        }
		StartCoroutine(InitBags());//携程初始化背包
	}
	
	//初始化Bag
	IEnumerator InitBags()
    {
		for (int i=0; i<BagManager.Instance.Items.Length; i++)
        {
			var item = BagManager.Instance.Items[i];
			if(item.ItemId > 0)
            {
				GameObject go = Instantiate(bagItem, slots[i].transform);//父节点设置为槽
				var ui = go.GetComponent<UIIconItem>();
				var def = ItemManager.Instance.Items[item.ItemId].Define;//配置表里的数据
				ui.SetMainIcon(def.Icon, item.Count.ToString());//图标和数量设置给格子ui
            }
        }
		for (int i = BagManager.Instance.Items.Length; i < slots.Count; i++)//背包里已经解锁的道具有多长
		{
			slots[i].color = Color.gray;//把没解锁的格子设置为灰色
		}

		this.SetGold();

		yield return null;
	}

	public void SetGold()
    {
		this.money.text = User.Instance.CurrentCharacter.Gold.ToString();
    }

	void Clear()
    {
		for (int i = 0; i < slots.Count; i++)
        {
			if (slots[i].transform.childCount > 0)
            {
				Destroy(slots[i].transform.GetChild(0).gameObject);
            }
        }
    }

	public void OnReset()
    {
		BagManager.Instance.Reset();
		this.Clear();
		StartCoroutine(InitBags());
    }
}

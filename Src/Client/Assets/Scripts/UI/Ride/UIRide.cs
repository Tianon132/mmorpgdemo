using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using SkillBridge.Message;
using Models;

public class UIRide : UIWindow {

	public Text descript;
	public GameObject itemPrefab;
	public ListView listMain;
	private UIRideItem selectedItem;

	// Use this for initialization
	void Start () {
		RefreshUI();
		this.listMain.onItemSelected += this.OnItemSelected;
	}
	
	public void OnDestroy()
    {

    }

	//被选中时右边获得被选中的信息
	public void OnItemSelected(ListView.ListViewItem item)
    {
		this.selectedItem = item as UIRideItem;
		this.descript.text = this.selectedItem.item.Define.Description;
    }

	void RefreshUI()
    {
		ClearItems();
		InitItems();
    }

	void InitItems()
    {
		foreach(var kv in ItemManager.Instance.Items)
        {
			if(kv.Value.Define.Type == ItemType.Ride && 
				(kv.Value.Define.LimitClass == CharacterClass.None || kv.Value.Define.LimitClass == User.Instance.CurrentCharacter.Class))//要么没职业，要么对应职业才显示
            {
				//马已经骑上就不显示了
				if (EquipManager.Instance.Contains(kv.Key))
					continue;

				GameObject go = Instantiate(itemPrefab, this.listMain.transform);
				UIRideItem ui = go.GetComponent<UIRideItem>();
				ui.SetRideItem(kv.Value, this);
				this.listMain.AddItem(ui);
            }
        }
    }

	void ClearItems()
    {
		this.listMain.RemoveAll();
    }

	//召唤坐骑按钮
	public void DoRide()
    {
		if(this.selectedItem == null)
        {
			MessageBox.Show("请选择召唤的坐骑", "坐骑");
			return;
        }
		User.Instance.Ride(this.selectedItem.item.Id);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Models;
using UnityEngine.EventSystems;
using System;
using Managers;

public class UIEquipItem : MonoBehaviour, IPointerClickHandler {//指针点击处理器

	public Image icon;
	public Text title;
	public Text level;
	public Text limitClass;
	public Text limitCategory;

	public Image background;
	public Sprite normalBg;
	public Sprite selectedBg;

	private bool selected;
	public bool Selected
    {
        get { return selected; }
        set
        {
			selected = value;
			this.background.overrideSprite = selected ? selectedBg : normalBg;
        }
    }

	public int index { get; set; }
	private UICharEquip owner;

	private Item item;

	bool isEquiped = false;

    //设定装备
	public void SetEquipItem(int id, Item item, UICharEquip owner, bool equiped)
    {
		this.owner = owner;
		this.index = index;
		this.item = item;
		this.isEquiped = equiped;

		if (this.title != null) this.title.text = this.item.Define.Name;
		if (this.level != null) this.level.text = item.Define.Level.ToString();
		if (this.limitClass != null) this.limitClass.text = item.Define.LimitClass.ToString();
		if (this.limitCategory != null) this.limitCategory.text = item.Define.Category;
		if (this.icon != null) this.icon.overrideSprite = Resloader.Load<Sprite>(this.item.Define.Icon);
	}

	//指针点击处理器（select是选中，click是点击，按下就执行）
	public void OnPointerClick(PointerEventData eventData)
    {
        if(this.isEquiped)
        {
			UnEquip();
        }
		else
        {
			if (this.Selected)//点击两次
			{
				DoEquip();
				this.Selected = false;
			}
			else
				this.Selected = true;
        }
    }

	//穿装备
    void DoEquip()
    {
		var msg = MessageBox.Show(string.Format("要装备[{0}]吗？", this.item.Define.Name), "确认", MessageBoxType.Confirm);
		msg.OnYes = () =>
		{
			var oldEquip = EquipManager.Instance.GetEquip(item.EquipInfo.slot);
			if (oldEquip != null)
			{
				var newmsg = MessageBox.Show(string.Format("要替换掉[{0}]吗？", oldEquip.Define.Name), "确认", MessageBoxType.Confirm);
				newmsg.OnYes = () =>
				{
					this.owner.DoEquip(this.item);
				};
			}
			else
			{
				this.owner.DoEquip(this.item);
			}
		};
    }

	//脱装备
    void UnEquip()
    {
		var msg = MessageBox.Show(string.Format("要取下装备[{0}]吗？", this.item.Define.Name), "确认", MessageBoxType.Confirm); ;
		msg.OnYes = () =>
		{
			this.owner.UnEquip(this.item);
        };
    }
}

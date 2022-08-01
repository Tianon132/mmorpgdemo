using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Common.Data;
using UnityEngine.EventSystems;
//using System.Collections.Generic;

public class UIShopItem : MonoBehaviour, ISelectHandler {

	public Image icon;
	public Text title;
	public Text price;
	public Text limitClass;//装备：职业限制
	public Text count;

	public Image background;
	public Sprite normalBg;
	public Sprite selectedBg;//上面都是UI的变量

	private bool selected;
	public bool Selected
    {
        get { return selected; }
        set
        {
			selected = value;
			this.background.overrideSprite = selected ? selectedBg : normalBg;
        }
    }//这个对象有没有被选中

	public int ShopItemID { get; set; }
	private UIShop shop;

	private ItemDefine item;
	private ShopItemDefine ShopItem { get; set; }

	// Use this for initialization
	void Start () {
		
	}
	
	public void SetShopItem(int id, ShopItemDefine shopItem, UIShop owner)
	{
		this.shop = owner;
		this.ShopItemID = id;
		this.ShopItem = shopItem;
		this.item = DataManager.Instance.Items[this.ShopItem.ItemID];

		this.title.text = this.item.Name;
		this.count.text = "x" + shopItem.Count.ToString();
		this.price.text = shopItem.Price.ToString();
		this.limitClass.text = this.item.LimitClass.ToString();
		this.icon.overrideSprite = Resloader.Load<Sprite>(item.Icon);
	}

	//这个函数的意思是，
	//如果这个对象是个可选择的对象，即有selectable插件
	//那么当你选择这个对象的时候，他会自然而然的调用这个函数中的内容方法
    public void OnSelect(BaseEventData eventData)//ISelectHandler接口，要求重写方法
	{
		this.Selected = true;//标记自己为被选中
		this.shop.SelectShopItem(this);
    }
}

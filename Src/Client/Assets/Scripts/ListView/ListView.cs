using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]//序列化？
public class ItemSelectEvent : UnityEvent<ListView.ListViewItem>
{ }

/// <summary>
/// ListView类（外围）
/// 作用：对所有的Item进行操作
/// </summary>
public class ListView : MonoBehaviour {

	public UnityAction<ListViewItem> onItemSelected;//委托：一对一

    /// <summary>
    /// ListViewItem类（内围）
    /// 作用：对单个Item进行操作
    /// </summary>
	public class ListViewItem : MonoBehaviour, IPointerClickHandler
    {
        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                onSelected(selected);
            }
        }

        public virtual void onSelected(bool selected)//Selected被set时，说明被点击，点击为true时，触发onSelected
        { }

        public ListView owner;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!this.selected)
            {
                this.Selected = true;//该Item的UI标记为被选择
            }
            if(owner != null && owner.SelectedItem != this)
            {
                owner.SelectedItem = this;//记录当前列表的被选择的Item
            }
        }
    }

    List<ListViewItem> items = new List<ListViewItem>();//记录列表的所有Item

    private ListViewItem selectedItem = null;
    public ListViewItem SelectedItem
    {
        get { return selectedItem; }
        set
        {
            if(selectedItem != null && selectedItem != value)
            {
                selectedItem.Selected = false;//首先关闭该Item的选择状态
            }
            selectedItem = value;//指向新的Item对象
            if(value != null && onItemSelected != null)
                 onItemSelected.Invoke((ListViewItem)value);//如果有委托，那么就回调
        }
    }

    //添加Item
    public void AddItem(ListViewItem item)
    {
        item.owner = this;//设该item 的 列表 为this
        this.items.Add(item);
    }

    //删除所有的Item
    public void RemoveAll()
    {
        foreach(var it in items)
        {
            Destroy(it.gameObject);
        }
        items.Clear();
    }
}

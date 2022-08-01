using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class TabButton : MonoBehaviour {

	public Sprite activeImage;
	private Sprite normalImage;

	public TabView tabView;

	public int tabIndex = 0;
	public bool selected = false;

	private Image tabImage;

	// Use this for initialization
	void Start () {
		tabImage = this.GetComponent<Image>();
		normalImage = tabImage.sprite;//启动时把当时页面给存下来

		this.GetComponent<Button>().onClick.AddListener(OnClick);//动态给按钮添加一个事件
	}
	
	public void Select(bool select)
    {
		tabImage.overrideSprite = select ? activeImage : normalImage;
    }

	void OnClick()//点击事件，切换标签
    {
		this.tabView.SelectTab(this.tabIndex);
    }
}

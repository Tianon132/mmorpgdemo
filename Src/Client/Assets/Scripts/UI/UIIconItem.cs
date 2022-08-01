using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UIIconItem : MonoBehaviour {

	public Image mainImage;
	public Image secondImage;

	public Text mainText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//因为是前两个定义是动态的，故用此方法来设置对象
	public void SetMainIcon(string iconName, string text)
    {
		this.mainImage.overrideSprite = Resloader.Load<Sprite>(iconName);
		this.mainText.text = text;
    }
}

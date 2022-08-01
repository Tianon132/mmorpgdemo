using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Entities;

[ExecuteInEditMode]
public class UINameBar : MonoBehaviour {

	public Text avaverName;

	public Character character;

	// Use this for initialization
	void Start () {
		if (this.character != null)
        {

        }
	}
	
	// Update is called once per frame
	void Update () {
		this.UpdateInfo();

		//this.transform.LookAt(Camera.main.transform, Vector3.up);
		//删除 this.transform.forward = Camera.main.transform.forward;//血条方向自主更新，反之加到UIworldElement
	}

	void UpdateInfo()
    {
		if(this.character != null)
        {
			string name = this.character.Name + "Lv." + this.character.Info.Level;
			if(name != this.avaverName.text)//每一次赋值都会引起UI的重绘，为了性能就没必要每次都赋值
            {
				this.avaverName.text = name;
            }
        }
    }
}

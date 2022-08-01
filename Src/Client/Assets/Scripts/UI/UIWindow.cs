using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//给所有的UI做父类
public class UIWindow : MonoBehaviour {

	public delegate void CloseHandler(UIWindow sender, WindowResult result);
	public event CloseHandler OnClose;

	public virtual System.Type Type { get { return this.GetType(); } }//获取类型的

	public GameObject Root;//代表这个脚本的父节点是谁，因为每个界面都有个Panel作为父节点，就是为了找它

	//内置了一个结果类型
	public enum WindowResult 
	{
		None = 0,
		Yes,
		No,
	}

	//调用UIManager关闭自己的类型，对任何一个窗口进行Close()都会调用UIManager
	public void Close(WindowResult result = WindowResult.None)
    {
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);
		UIManager.Instance.Close(this.Type);
		if (this.OnClose != null)
			this.OnClose(this, result);
		this.OnClose = null;
    }

	//大部分对话框上都有两个按钮，确定或取消，所以在这里用虚函数内置了两个函数，都是关闭作用
	public virtual void OnCloseClick()
    {
		this.Close();
    }
	public virtual void OnYesClick()
    {
		this.Close(WindowResult.Yes);
    }

	public virtual void OnNoClick()
	{
		this.Close(WindowResult.No);//任务系统时补充
	}

	//临时加上为了测试
	void OnMouseDown()
    {
		Debug.LogFormat(this.name + " Clicked");
    }
}

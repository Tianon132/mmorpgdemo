using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services;

//创建公会
public class UIGuildPopCreate : UIWindow {

	public InputField inputName;
	public InputField inputNotice;

	// Use this for initialization
	public void Start () {
		GuildService.Instance.OnGuildCreateResult = OnGuildCreated;
	}
	
	public void OnDestory()
    {
		GuildService.Instance.OnGuildCreateResult = null;
	}

	public override void OnYesClick ()//创建点击之后是不能关闭的，所以override	//服务器返回之来之后才关闭
	{
		if(string.IsNullOrEmpty(inputName.text))
        {
			MessageBox.Show("请输入公会名称", "错误", MessageBoxType.Error);
			return;
        }
		if(inputName.text.Length < 4 || inputName.text.Length > 10)
        {
			MessageBox.Show("公会名称为4-10个字符", "错误", MessageBoxType.Error);
			return;
        }

		if(string.IsNullOrEmpty(inputNotice.text))
        {
			MessageBox.Show("请输入公会宣言", "错误", MessageBoxType.Error);
			return;
		}
		if (inputNotice.text.Length < 3 || inputNotice.text.Length > 50)
		{
			MessageBox.Show("公会宣言为3-50个字符", "错误", MessageBoxType.Error);
			return;
		}

		GuildService.Instance.SendGuildCreate(inputName.text, inputNotice.text);
	}	

	void OnGuildCreated(bool result)//服务器返回关闭
    {
		if (result)
			this.Close(WindowResult.Yes);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services;
using SkillBridge.Message;

public class UILogin : MonoBehaviour {

	public InputField username;
	public InputField password;
	public Button buttonLogin;

	// Use this for initialization
	void Start () {
		UserService.Instance.OnLogin = this.OnLogin;
	}

	void OnLogin(Result result, string msg)
	{
		if (result == Result.Success)
		{
			//登录成功，进入角色选择
			SceneManager.Instance.LoadScene("CharSelect");
			SoundManager.Instance.PlayMusic(SoundDefine.Music_Select);
		}
		else
			MessageBox.Show(msg, "错误", MessageBoxType.Error);
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void OnClickLogin()
    {
		//UI层
		if (string.IsNullOrEmpty(this.username.text))
		{
			MessageBox.Show("请输入账号");
			return;
		}
		if (string.IsNullOrEmpty(this.password.text))
		{
			MessageBox.Show("请输入密码");
			return;
		}
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);

		//服务业务层，与UI层互不干涉
		UserService.Instance.SendLogin(this.username.text, this.password.text);
	}
}

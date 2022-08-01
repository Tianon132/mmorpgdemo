using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services;
using SkillBridge.Message;

/// <summary>
/// UI层可以直接调用逻辑层，但逻辑层不能直接调用UI层，必须用事件代替，即Onregister等等
/// </summary>

public class UIRegister : MonoBehaviour {

	public InputField username;
	public InputField password;
	public InputField passwordConfirm;
	public Button buttonRegister;

	// Use this for initialization
	void Start () {
		UserService.Instance.OnRegister = this.OnRegister;//收集从服务端发来的消息
	}

	void OnRegister(Result result, string msg)
    {
		if (result == Result.Success)
		{
			//注册成功，进入角色选择
			MessageBox.Show(msg, "成功", MessageBoxType.Information);
		}
		else
			MessageBox.Show(msg, "错误", MessageBoxType.Error);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	//一般来说用户校验是在服务端的，但是为了避免服务端压力过大，那么一些基本的校验客户端来就可以，这是保障了用户的使用体验，但服务端还是要做必要的校验的，这里为了用户的安全性
	public void OnClickRegister()
    {
		//UI层
		if(string.IsNullOrEmpty(this.username.text))
        {
			MessageBox.Show("请输入账号");
			return;
        }
		if(string.IsNullOrEmpty(this.password.text))
        {
			MessageBox.Show("请输入密码");
			return;
        }
		if(string.IsNullOrEmpty(this.passwordConfirm.text))
        {
			MessageBox.Show("请输入确认密码");
			return;
        }
		if(this.password.text != this.passwordConfirm.text)
        {
			MessageBox.Show("两次输入密码不一致");
			return;
        }

		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);

		//服务业务层，与UI层互不干涉
		UserService.Instance.SendRegister(this.username.text, this.password.text);
    }
}

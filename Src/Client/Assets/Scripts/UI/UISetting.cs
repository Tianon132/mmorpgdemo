using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Services;

public class UISetting : UIWindow {

    /// <summary>
    /// 返回角色选择界面
    /// </summary>
	public void ExitToCharSelect()
    {
        SceneManager.Instance.LoadScene("CharSelect");
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Select);
        UserService.Instance.SendGameLeave();
    }

    /// <summary>
    /// 打开系统设置
    /// </summary>
    public void SystemConfig()
    {
        UIManager.Instance.Show<UISystemConfig>();
        this.Close();
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
        Services.UserService.Instance.SendGameLeave(true);
    }
}

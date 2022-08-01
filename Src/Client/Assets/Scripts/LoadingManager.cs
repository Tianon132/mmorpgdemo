using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Services;
using Managers;

public class LoadingManager : MonoBehaviour {

	public GameObject UITips;
	public GameObject UILoading;
	public GameObject UILogin;

	public Slider progressBar;
	public Text progressText;
	public Text progressNumber;

	// Use this for initialization
	IEnumerator Start () {
		//初始化日志
		log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.xml"));
		UnityLogger.Init();
		Common.Log.Init("Unity");
		Common.Log.Info("LoadingManager start");

		//实现刚开始黑屏字幕到加载界面的过程
		UITips.SetActive(true);
		UILoading.SetActive(false);
		UILogin.SetActive(false);

		yield return new WaitForSeconds(2f);
		UILoading.SetActive(true);
		yield return new WaitForSeconds(1f);
		UITips.SetActive(false);

		yield return DataManager.Instance.LoadData();//运行到这里时候，短点都有了

		MapService.Instance.Init();
		UserService.Instance.Init();

		//TestManager.Instance.Init();  //测试NPC系统使用的
		StatusService.Instance.Init();
		FriendService.Instance.Init();
		TeamService.Instance.Init();
		GuildService.Instance.Init();
		ChatService.Instance.Init();
		ShopManager.Instance.Init();
		SoundManager.Instance.PlayMusic(SoundDefine.Music_Login);

		//实现加载界面的加载效果
		for (float i = 50; i < 100;)
		{
			i += Random.Range(0.1f, 1.5f);
			progressBar.value = i;
			yield return new WaitForEndOfFrame();
		}

		//关闭加载界面。打开登录界面
		UILoading.SetActive(false);
		UILogin.SetActive(true);
		yield return null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

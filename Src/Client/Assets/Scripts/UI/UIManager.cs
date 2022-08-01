using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager> {

	//Class
	class UIElement
    {
		public string Resources;
		public bool Cache;
		public GameObject Instance;
    }

	private Dictionary<Type, UIElement> UIResources = new Dictionary<Type, UIElement>();//用来保存定义的UI信息

    public UIManager()
    {
        //放在构造函数里是为了第一时间初始化出来
        this.UIResources.Add(typeof(UITest), new UIElement() { Resources = "UI/UITest", Cache = true });
        this.UIResources.Add(typeof(UIBag), new UIElement() { Resources = "UI/UIBag", Cache = false });//cache为true就不会销毁，只会隐藏，元素不会刷新，
                                                                                                       //这里为了省下写刷新逻辑，所以改为false，直接销毁，起到刷新的作用
        this.UIResources.Add(typeof(UIShop), new UIElement() { Resources = "UI/UIShop", Cache = false });
        this.UIResources.Add(typeof(UICharEquip), new UIElement() { Resources = "UI/UICharEquip", Cache = false });

        this.UIResources.Add(typeof(UIQuestSystem), new UIElement() { Resources = "UI/UIQuestSystem", Cache = false });
        this.UIResources.Add(typeof(UIQuestDialog), new UIElement() { Resources = "UI/UIQuestDialog", Cache = false });

        this.UIResources.Add(typeof(UIFriends), new UIElement() { Resources = "UI/UIFriends", Cache = false });

		this.UIResources.Add(typeof(UIGuild), new UIElement() { Resources = "UI/Guild/UIGuild", Cache = false });
		this.UIResources.Add(typeof(UIGuildList), new UIElement() { Resources = "UI/Guild/UIGuildJoin", Cache = false });
		this.UIResources.Add(typeof(UIGuildPopNoGuild), new UIElement() { Resources = "UI/Guild/UIGuildPopNoGuild", Cache = false });
		this.UIResources.Add(typeof(UIGuildPopCreate), new UIElement() { Resources = "UI/Guild/UIGuildCreate", Cache = false });
		this.UIResources.Add(typeof(UIGuildApplyList), new UIElement() { Resources = "UI/Guild/UIGuildApplyList", Cache = false });
		
		this.UIResources.Add(typeof(UISetting), new UIElement() { Resources = "UI/UISetting", Cache = true });
		this.UIResources.Add(typeof(UIPopCharMenu), new UIElement() { Resources = "UI/UIPopCharMenu", Cache = false });
		this.UIResources.Add(typeof(UIRide), new UIElement() { Resources = "UI/UIRide", Cache = false });
		this.UIResources.Add(typeof(UISystemConfig), new UIElement() { Resources = "UI/UISystemConfig", Cache = false });
	}

    ~UIManager()
    {
    }

	/// <summary>
	/// Show UI
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T Show<T>()
    {
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Open);
		Type type = typeof(T);
		if (this.UIResources.ContainsKey(type))//先判断有没有，有就直接拿出来
		{
			UIElement info = this.UIResources[type];
			if(info.Instance != null)//在判断实例有没有，有就直接激活
            {
				info.Instance.SetActive(true);
            }
			else//没有就读取并且实例化
            {
				UnityEngine.Object prefab = Resources.Load(info.Resources);//得到这个资源，空就实例化
				if (prefab == null)
                {
					return default(T);
                }
				info.Instance = (GameObject)GameObject.Instantiate(prefab);
            }
			return info.Instance.GetComponent<T>();
        }
		return default(T);
    }

	public void Close(Type type)
    {
		//SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);
		if(this.UIResources.ContainsKey(type))
        {
			UIElement info = this.UIResources[type];
			if(info.Cache)//如果在缓存，就关闭
            {
				info.Instance.SetActive(false);
            }
			else//否则就直接销毁
            {
				GameObject.Destroy(info.Instance);
				info.Instance = null;
            }
        }
    }

	public void Close<T>()//根据窗口名关掉指定窗口
    {
		this.Close(typeof(T));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;
using Managers;

public class UIWorldElementManager :  MonoSingleton<UIWorldElementManager>{//单例模式，为了更好的使用该类

	public GameObject nameBarPrefab;
	public GameObject npcStatusPrefab;

	private Dictionary<Transform, GameObject> elementNames = new Dictionary<Transform, GameObject>();
	private Dictionary<Transform, GameObject> elementStatus = new Dictionary<Transform, GameObject>();

    protected override void OnStart()
    {
		nameBarPrefab.SetActive(false);
    }

    public void AddCharacterNameBar(Transform owner, Character character)
    {
		GameObject goNamrBar = Instantiate(nameBarPrefab, this.transform);
		goNamrBar.name = "NameBar" + character.EntityId;
		goNamrBar.GetComponent<UIWorldElement>().owner = owner;
		goNamrBar.GetComponent<UINameBar>().character = character;
		goNamrBar.SetActive(true);
		this.elementNames[owner] = goNamrBar;
    }

	public void RemoveCharacterNameBar(Transform owner)
    {
		if(this.elementNames.ContainsKey(owner))
        {
			Destroy(this.elementNames[owner]);
			this.elementNames.Remove(owner);
        }
    }

	//Npc状态
	public void AddNpcQuestStatus(Transform owner, NpcQuestStatus status)
    {
		if(this.elementStatus.ContainsKey(owner))
        {
			elementStatus[owner].GetComponent<UIQuestStatus>().SetQuestStatus(status);
        }
		else
        {
			GameObject go = Instantiate(npcStatusPrefab, this.transform);
			go.name = "NpcQuestStatus" + owner.name;
			go.GetComponent<UIWorldElement>().owner = owner;
			go.GetComponent<UIQuestStatus>().SetQuestStatus(status);
			go.SetActive(true);
			this.elementStatus[owner] = go;
        }
    }

	public void RemoveNpcQuestStatus(Transform owner)
    {
		if(this.elementStatus.ContainsKey(owner))
        {
			Destroy(this.elementStatus[owner]);
			this.elementStatus.Remove(owner);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Managers;
using Entities;
using Models;

public class GameObjectManager : MonoSingleton<GameObjectManager> {
    //使用单例的目的是为了防止场景切换的时候角色的摧毁
    //使用单例的目的是为了防止场景切换的时候角色的摧毁
    //使用单例的目的是为了防止场景切换的时候角色的摧毁
    //使用单例的目的是为了防止场景切换的时候角色的摧毁
    //使用单例的目的是为了防止场景切换的时候角色的摧毁（需删除Start()）

    Dictionary<int, GameObject> Characters = new Dictionary<int, GameObject>();
    //用字典把一个ID和一个对象绑定

    // Use this for initialization
    //void Start () {   //单例中是不能直接使用Start()

    //	}

    protected override void OnStart()//覆盖
    {
        StartCoroutine(InitGameObjects());//创建角色

        CharacterManager.Instance.OnCharacterEnter += OnCharacterEnter;//注册了一个角色进入的事件
                            //如果有角色进入，就会触发，然后进入管理器的函数，再出发创建对象
        CharacterManager.Instance.OnCharacterLeave += OnCharacterLeave;
    }
	
	private void OnDestroy()
    {
        CharacterManager.Instance.OnCharacterEnter -= OnCharacterEnter;
        CharacterManager.Instance.OnCharacterLeave -= OnCharacterLeave;
    }

    void OnCharacterEnter(Character cha)
    {
        CreateCharacterObject(cha);//创建角色
    }

    void OnCharacterLeave(Character cha)//销毁角色
    {
        if (!Characters.ContainsKey(cha.EntityId))//DBID数据库，entityID，TID职业
            return;

        if(Characters[cha.EntityId] != null)//entityId作为游戏中的主要标识
        {
            Destroy(Characters[cha.EntityId]);
            this.Characters.Remove(cha.EntityId);
        }
    }

    IEnumerator InitGameObjects()
    {
        foreach(var cha in CharacterManager.Instance.Characters.Values)
        {
            CreateCharacterObject(cha);
            yield return null;
        }
    }

    private void CreateCharacterObject(Character character)//创建单个角色
    {
        if (!Characters.ContainsKey(character.EntityId) || Characters[character.EntityId] == null)
        {
            Object obj = Resloader.Load<Object>(character.Define.Resource);//资源加载
            if(obj == null)
            {
                Debug.LogErrorFormat("Character[{0}] Resource[{1}] not existed.", character.Define.TID, character.Define.Resource);
                return;
            }

            GameObject go = (GameObject)Instantiate(obj, this.transform);//创建于该目录下
            go.name = "Character_" + character.Id+ "_" + character.Name;
            
            Characters[character.EntityId] = go;

            UIWorldElementManager.Instance.AddCharacterNameBar(go.transform, character);
        }
        this.InitGameObject(Characters[character.EntityId], character);
    }

    
    private void InitGameObject(GameObject go,Character character)//方便之后的复用
    {
        go.transform.position = GameObjectTool.LogicToWorld(character.position);//角色实体的坐标（服务器返回的坐标）转化为世界坐标
        go.transform.forward = GameObjectTool.LogicToWorld(character.direction);
        //Characters[character.entityId] = go;

        EntityController ec = go.GetComponent<EntityController>();
        if (ec != null)
        {
            ec.entity = character;
            ec.isPlayer = character.isCurrentPlayer;
            ec.Ride(character.Info.Ride);//0就没马
        }//从绑定的脚本取下来获得值

        PlayerInputController pc = go.GetComponent<PlayerInputController>();
        if (pc != null)//如果有这个组件，判断是不是当前控制角色，不是就关掉
        {
            if (character.isCurrentPlayer)//摄像头，控制等关键部分,之前使用currentCharacter的Id判断是不是当前的玩家
            {
                User.Instance.CurrentCharacterObject = pc;//12节：小地图创建，赋予当前角色对象
                MainPlayerCamera.Instance.player = go;
                pc.enabled = true;
                pc.character = character;
                pc.entityController = ec;
            }
            else
            {
                pc.enabled = false;
            }
        }
    }

    /// <summary>
    /// 加载坐骑
    /// </summary>
    /// <param name="rideId"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public RideController LoadRide(int rideId, Transform parent)
    {
        var rideDefine = DataManager.Instance.Rides[rideId];
        Object obj = Resloader.Load<Object>(rideDefine.Resource);
        if(obj == null)
        {
            Debug.LogErrorFormat("Ride[{0}] Resource[{1}] not existed.", rideDefine.ID, rideDefine.Resource);
            return null;
        }
        GameObject go = (GameObject)Instantiate(obj, parent);
        go.name = "Ride_" + rideDefine.ID + "_" + rideDefine.Name;
        return go.GetComponent<RideController>();
    }
}

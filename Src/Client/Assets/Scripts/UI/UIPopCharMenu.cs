using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Managers;
using Services;

public class UIPopCharMenu : UIWindow, IDeselectHandler
{
    public int targetId;

    public string targetName;

    public void OnDeselect(BaseEventData eventData)
    {
        var ed = eventData as PointerEventData;
        if (ed.hovered.Contains(this.gameObject))//当前节点是否包含当前界面  //hovered会罗列出当前节点和所属子节点
            return;                              //是触发下面点击事件的必要条件   
                                                 //目的：如果点击的是当前对象的页面就不能关闭
        this.Close(WindowResult.None);
    }

    public void OnEnable()//脚本所挂对象被启用时调用
    {
        this.GetComponent<Selectable>().Select();//保证激活这个窗口的时候顺便保持选中状态
        this.Root.transform.position = Input.mousePosition + new Vector3(80, 0, 0);
    }

    public void OnChat()
    {
        ChatManager.Instance.StartPrivateChat(targetId, targetName);
        this.Close(WindowResult.No);
    }

    public void OnAddFriend()//扩展作业
    {
        FriendService.Instance.SendFriendAddRequest(targetId, targetName);
        this.Close(WindowResult.No);
    }

    public void OnInviteTeam()//扩展作业
    {
        TeamService.Instance.SendTeamInviteRequest(targetId, targetName);
        this.Close(WindowResult.No);
    }
}

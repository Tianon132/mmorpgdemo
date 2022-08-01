using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.Events;
using Network;
using SkillBridge.Message;
using UnityEngine;
using Models;
using Managers;

namespace Services
{
    class FriendService : Singleton<FriendService>,IDisposable
    {
        public UnityAction OnFriendUpdate;//一对一委托，无需声明参数

        public FriendService()
        {
            MessageDistributer.Instance.Subscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer.Instance.Subscribe<FriendAddResponse>(this.OnFriendAddResponse);//好友添加的请求和响应
            MessageDistributer.Instance.Subscribe<FriendListResponse>(this.OnFriendList);
            MessageDistributer.Instance.Subscribe<FriendRemoveResponse>(this.OnFriendRemove);
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer.Instance.Unsubscribe<FriendAddResponse>(this.OnFriendAddResponse);
            MessageDistributer.Instance.Unsubscribe<FriendListResponse>(this.OnFriendList);
            MessageDistributer.Instance.Unsubscribe<FriendRemoveResponse>(this.OnFriendRemove);
        }

        public void Init()
        {

        }

        public void SendFriendAddRequest(int friendId, string friendName)//自己加别人-请求
        {
            Debug.Log("SendFriendAdd");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendAddReq = new FriendAddRequest();
            message.Request.friendAddReq.FromId = User.Instance.CurrentCharacter.Id;
            message.Request.friendAddReq.FromName = User.Instance.CurrentCharacter.Name;//自己的信息
            message.Request.friendAddReq.ToId = friendId;
            message.Request.friendAddReq.ToName = friendName;//要加好友的对方的信息
            NetClient.Instance.SendMessage(message);
        }

        public void SendFriendAddResponse(bool accept, FriendAddRequest request)//别人加自己-自己的回应-发送请求
        {
            Debug.Log("SendFriendAdd");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendAddRes = new FriendAddResponse();
            message.Request.friendAddRes.Result = accept ? Result.Success : Result.Failed;
            message.Request.friendAddRes.Errormsg = accept ? "对方同意" : "对方拒绝了你的请求";
            message.Request.friendAddRes.Request = request;//之前对方发送的加好友请求
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 收到添加好友请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnFriendAddRequest(object sender, FriendAddRequest request)//别人家自己-接受请求
        {
            var confirm = MessageBox.Show(String.Format("{0} 请求加你为好友", request.FromName), "好友请求", MessageBoxType.Confirm, "接受", "拒绝");
            confirm.OnYes = () =>
            {
                this.SendFriendAddResponse(true, request);
            };
            confirm.OnNo = () =>
            {
                this.SendFriendAddResponse(false, request);
            };
        }

        /// <summary>
        /// 收到添加好友响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnFriendAddResponse(object sender, FriendAddResponse message)//自己加别人-收到别人的响应
        {
            if (message.Result == Result.Success)
                MessageBox.Show(message.Request.ToName + "接受了您的请求", "添加好友成功");
            else
                MessageBox.Show(message.Errormsg, "添加好友失败");
        }

        /// <summary>
        /// 好友列表 刷新变化的响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnFriendList(object sender, FriendListResponse message)
        {
            Debug.Log("OnFriendList");
            FriendManager.Instance.allFriends = message.Friends;//manager.Init就不用调用了
            if (this.OnFriendUpdate != null)
                this.OnFriendUpdate();
        }

        /// <summary>
        /// 删除好友的请求
        /// </summary>
        /// <param name="id"></param>
        /// <param name="friendId"></param>
        public void SendFriendRemoveRequest(int id, int friendId)//删除好友
        {
            Debug.Log("SendFriendRemoveRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendRemove = new FriendRemoveRequest();
            message.Request.friendRemove.Id = id;
            message.Request.friendRemove.friendId = friendId;
            NetClient.Instance.SendMessage(message);
        }

        private void OnFriendRemove(object sender, FriendRemoveResponse message)
        {
            if (message.Result == Result.Success)
                MessageBox.Show("刷新成功", "删除好友");
            else
                MessageBox.Show("删除失败", "删除好友", MessageBoxType.Error);
        }
    }
}

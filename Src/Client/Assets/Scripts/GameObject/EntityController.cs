using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Managers;
using Entities;
using SkillBridge.Message;

public class EntityController : MonoBehaviour, IEntityNotify {//完全不考虑输入，有动画，有刚体

	public Animator anim;
	public Rigidbody rb;
	private AnimatorStateInfo currentBaseState;

	public Entity entity;

	public Vector3 position;
	public Vector3 direction;
	Quaternion rotation;

	public Vector3 lastPosition;
	Quaternion lastRotation;

	public float speed;
	public float animSpeed = 1.5f;
	public float jumpPower = 3.0f;

	public bool isPlayer = false;

	public RideController rideController;
	public int currentRide = 0;
	public Transform rideBone;//角色身上的点，一般是屁股上，要跟马对齐

	// Use this for initialization
	void Start () {
		if(entity != null)
        {
			EntityManager.Instance.RegisterEntityChangeNotify(entity.EntityId, this);//如果entity不为空，注册加在这里
			this.UpdateTransform();
        }

		if (!this.isPlayer)
			rb.useGravity = false;//不是玩家，就没重力，防止其他角色掉下去
	}

	void UpdateTransform()
    {
		this.position = GameObjectTool.LogicToWorld(entity.position);//逻辑坐标变成世界坐标
		this.direction = GameObjectTool.LogicToWorld(entity.direction);

		this.rb.MovePosition(this.position);//逻辑对象的方向更新世界对象的方向
		this.transform.forward = this.direction;
		this.lastPosition = this.position;
		this.lastRotation = this.rotation;
    }

	void OnDestroy()
    {
		if (entity != null)
			Debug.LogFormat("{0} onDestroy : ID:{1} POS:{2} DIR:{3} SPD:{4}", this.name, entity.EntityId, entity.position, entity.direction, entity.speed);

		if(UIWorldElementManager.Instance != null)//世界元素的删除
        {
			UIWorldElementManager.Instance.RemoveCharacterNameBar(this.transform);
        }
    }

	void FixedUpdate()
    {
		if (this.entity == null)
			return;

		this.entity.OnUpdate(Time.fixedDeltaTime);

		if(!this.isPlayer)
        {
			this.UpdateTransform();
        }
    }

	public void OnEntityRemoved()
    {
		if (UIWorldElementManager.Instance != null)//先删掉血条
			UIWorldElementManager.Instance.RemoveCharacterNameBar(this.transform);
		Destroy(this.gameObject);//再删自己
    }

	public void OnEntityEvent(EntityEvent entityEvent, int param)
    {
		switch(entityEvent)
        {
			case EntityEvent.Idle:
				anim.SetBool("Move", false);
				anim.SetTrigger("Idle");
				break;
			case EntityEvent.MoveFwd:
				anim.SetBool("Move", true);
				break;
			case EntityEvent.MoveBack:
				anim.SetBool("Move", true);
				break;
			case EntityEvent.Jump:
				anim.SetTrigger("Jump");
				break;
			case EntityEvent.Ride:
                {
					this.Ride(param);
				}
				break;
        }
		if (this.rideController != null) this.rideController.OnEntityEvent(entityEvent, param);
    }

    public void OnEntityChanged(Entity entity)
    {
		Debug.LogFormat("OnEntityChanged:ID:{0} POS:{1} DIR:{2} SPD:{3}", entity.EntityId, entity.position, entity.direction, entity.speed);
    }

	public void Ride(int rideId)
    {
		if (currentRide == rideId) return;
		currentRide = rideId;
		if(rideId > 0)
        {
			this.rideController = GameObjectManager.Instance.LoadRide(rideId, this.transform);
        }
		else
        {
			Destroy(this.rideController.gameObject);
			this.rideController = null;
        }

		if(this.rideController == null)
        {
			this.anim.transform.localPosition = Vector3.zero;
			this.anim.SetLayerWeight(1, 0);
        }
		else
        {
			this.rideController.SetRider(this);//通知我谁在骑
			this.anim.SetLayerWeight(1, 1);//动画状态机方法：需要对动画骨骼进行混合，【后者0变为1，就让步行状态变为骑行状态】
        }
    }

	public void SetRidePosition(Vector3 position)
    {
		this.anim.transform.position = position + (this.anim.transform.position - this.rideBone.position);
    }
}
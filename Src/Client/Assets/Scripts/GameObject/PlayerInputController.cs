using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SkillBridge.Message;
using Entities;
using Services;

public class PlayerInputController : MonoBehaviour {

	public Rigidbody rb;
	CharacterState state;

	public Character character;

	public float rotateSpeed = 2.0f;

	public float turnAngle = 10;

	public float speed;

	public EntityController entityController;

	public bool onAir = false;

	private NavMeshAgent agent;
	private bool autotNav = false;//标记：是不是在寻路

	// Use this for initialization
	void Start () {
		state = CharacterState.Idle;

		if(this.character == null)
        {
			DataManager.Instance.Load();
			NCharacterInfo cinfo = new NCharacterInfo();
			cinfo.Id = 1;
			cinfo.Name = "Test";
			cinfo.ConfigId = 1;
			cinfo.Entity = new NEntity();
			cinfo.Entity.Position = new NVector3();
			cinfo.Entity.Direction = new NVector3();
			cinfo.Entity.Direction.X = 0;
			cinfo.Entity.Direction.Y = 100;
			cinfo.Entity.Direction.Z = 0;
			this.character = new Character(cinfo);

			if (entityController != null)
				entityController.entity = this.character;
        }

		if(agent == null)
        {
			agent = this.gameObject.AddComponent<NavMeshAgent>();
			agent.stoppingDistance = 0.3f;//快到时候，离目标距离0.3米停下
        }
	}
	
	/// <summary>
	/// 寻路
	/// </summary>
	public void StartNav(Vector3 target)
    {
		StartCoroutine(BeginNav(target));
    }

	IEnumerator BeginNav(Vector3 target)//Nav同步开始行走
    {
		agent.SetDestination(target);//设置目的地
		yield return null;
		autotNav = true;
		if (state != CharacterState.Move)
        {
			state = CharacterState.Move;
			this.character.MoveForward();
			this.SendEntityEvent(EntityEvent.MoveFwd);
			agent.speed = this.character.speed / 100f;//保证速度相同
        }
    }

	public void StopNav()//Nav同步角色停止
    {
		autotNav = false;
		agent.ResetPath();//路径清空
		if (state != CharacterState.Idle)
        {
			state = CharacterState.Idle;
			this.rb.velocity = Vector3.zero;
			this.character.Stop();
			this.SendEntityEvent(EntityEvent.Idle);
        }
		NavPathRenderer.Instance.SetPath(null, Vector3.zero);//停止时--路径渲染设为空
    }

	public void NavMove()//在自动行路过程中做的逻辑
    {
		if (agent.pathPending) return;//寻路有没有完成
		if(agent.pathStatus == NavMeshPathStatus.PathInvalid)//寻路失败
        {
			StopNav();
			return;
        }
		if (agent.pathStatus != NavMeshPathStatus.PathComplete) return;//寻路没有完成

		if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.1 || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1)//玩家有输入的情况下，自动寻路停止
        {
			StopNav();
			return;
        }

		NavPathRenderer.Instance.SetPath(agent.path, agent.destination);//路径显示---路径渲染器
		if (agent.isStopped || agent.remainingDistance < 0.3f)//寻路有没有停止或离目标是不是小于0.3米
        {
			StopNav();
			return;
        }

	}

	// Update is called once per frame
	void FixedUpdate () {
		if (character == null)
			return;

		if(autotNav)//如果自动寻路开始，就不能再控制角色了，暂时屏蔽掉输入
        {
			NavMove();
			return;
        }

		if (InputManager.Instance != null && InputManager.Instance.IsInputMode) return;//如果在文字输入就不受控制

		float v = Input.GetAxis("Vertical");
		if(v > 0.01)
        {
			if(state != CharacterState.Move)
            {
				state = CharacterState.Move;//改变为移动状态
				this.character.MoveForward();//获得配置表中前进的速度
				this.SendEntityEvent(EntityEvent.MoveFwd);//播放前进动画
            }
			this.rb.velocity = this.rb.velocity.y * Vector3.up + GameObjectTool.LogicToWorld(character.direction) * (this.character.speed - 9.81f) / 100f;
        }
		else if(v < -0.01)//后移
        {
			if(state != CharacterState.Move)
            {
				state = CharacterState.Move;
				this.character.MoveBack();
				this.SendEntityEvent(EntityEvent.MoveBack);
			}
			this.rb.velocity = this.rb.velocity.y * Vector3.up + GameObjectTool.LogicToWorld(character.direction) * (this.character.speed - 9.81f) / 100f;
		}
        else//回复常态
        {
			if (state != CharacterState.Idle)
			{
				state = CharacterState.Idle;
				this.rb.velocity = Vector3.zero;
				this.character.Stop();
				this.SendEntityEvent(EntityEvent.Idle);
			}
		}

		if(Input.GetButtonDown("Jump"))//跳
        {
			this.SendEntityEvent(EntityEvent.Jump);
		}

		float h = Input.GetAxis("Horizontal");//左右旋转
		if(h < -0.1 || h > 0.1)
        {
			this.transform.Rotate(0, h * rotateSpeed, 0);
			Vector3 dir = GameObjectTool.LogicToWorld(character.direction);
			Quaternion rot = new Quaternion();
			rot.SetFromToRotation(dir, this.transform.forward);

			if (rot.eulerAngles.y > this.turnAngle && rot.eulerAngles.y < (360 - this.turnAngle))
            {
				character.SetDirection(GameObjectTool.WorldToLogic(this.transform.forward));
				rb.transform.forward = this.transform.forward;
				this.SendEntityEvent(EntityEvent.None);
			}
        }
	}

	Vector3 lastPos;
	float lastSync = 0;

	private void LateUpdate()//帧之后Update()之后的更新
	{
		Vector3 offset = this.rb.transform.position - lastPos;//offset偏移量
		this.speed = (int)(offset.magnitude * 100f / Time.deltaTime);
		this.lastPos = this.rb.transform.position;

		Vector3Int goLoginPos = GameObjectTool.WorldToLogic(this.rb.transform.position);
		float logicOffest = (goLoginPos - this.character.position).magnitude;
		if(logicOffest > 100)//逻辑位置和实际位置误差超过100，就看做误差出现，强制更新位置
        {
			this.character.SetPosition(GameObjectTool.WorldToLogic(this.rb.transform.position));
			this.SendEntityEvent(EntityEvent.None);
        }
		this.transform.position = this.rb.transform.position;

		//自动寻路--当我的角色方向产生一定偏差，也要同步
		Vector3 dir = GameObjectTool.LogicToWorld(character.direction);
		Quaternion rot = new Quaternion();
		rot.SetFromToRotation(dir, this.transform.forward);

		if(rot.eulerAngles.y > this.turnAngle && rot.eulerAngles.y < (360 - this.turnAngle))
        {
			character.SetDirection(GameObjectTool.WorldToLogic(this.transform.forward));
			this.SendEntityEvent(EntityEvent.None);
        }
	}

	public void SendEntityEvent(EntityEvent entityEvent, int param = 0)
    {
		if (entityController != null)
			entityController.OnEntityEvent(entityEvent, param);//调用，播放动画
		MapService.Instance.SendMapEntitySync(entityEvent, this.character.EntityData, param);
    }
}

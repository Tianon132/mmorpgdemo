using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillBridge.Message;

public class RideController : MonoBehaviour {

	public Transform mountPoint;
	public EntityController rider;//角色
	public Vector3 offset;//偏移量-因为美术不会百分之百准确
	private Animator anim;

	// Use this for initialization
	void Start () {
		this.anim = this.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if (this.mountPoint == null || this.rider == null) return;

		this.rider.SetRidePosition(this.mountPoint.position + this.mountPoint.TransformDirection(this.offset));//因为是不断变化的，而且转弯的时候也要变化
	}

	public void SetRider(EntityController rider)
    {
		this.rider = rider;
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
        }
    }
}

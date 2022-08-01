using Common.Data;
using Managers;
using Models;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour {

	public int npcID;

	SkinnedMeshRenderer renderer;
	Animator anim;
	Color orignColor;

	private bool inInteractive = false;

	NpcDefine npc;

	NpcQuestStatus questStatus;//任务系统，Npc状态，头顶的图标显示

	// Use this for initialization
	void Start () {
		renderer = this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		anim = this.gameObject.GetComponent<Animator>();
		orignColor = renderer.sharedMaterial.color;//保存最基础的材质的颜色
		npc = NpcManager.Instance.GetNpcDefine(this.npcID);
		NpcManager.Instance.UpdateNpcPosition(this.npcID, this.transform.position);//34.自动寻路--保存Npc位置
		this.StartCoroutine(Actions());

		RefreshMapStatus();//刷新Npc状态
		QuestManager.Instance.OnQuestStatusChanged += OnQuestStatusChanged;//通知任务管理器有消息发给我
	}

	/**任务系统**/
	void OnQuestStatusChanged(Quest quest)
    {
		this.RefreshMapStatus();
    }

	void RefreshMapStatus()
    {
		questStatus = QuestManager.Instance.GetQuestStatusByNpc(this.npcID);
		UIWorldElementManager.Instance.AddNpcQuestStatus(this.transform, questStatus);
    }

	void OnDestroy()
    {
		QuestManager.Instance.OnQuestStatusChanged -= OnQuestStatusChanged;
		if (UIWorldElementManager.Instance != null)
			UIWorldElementManager.Instance.RemoveNpcQuestStatus(this.transform);
    }



	IEnumerator Actions()//过一小段(5-10s)时间，随机做个小动作，即Relax动画
    {
		while(true)
        {
			if (inInteractive)
				yield return new WaitForSeconds(2f);
			else
				yield return new WaitForSeconds(Random.Range(5f, 10f));
			
			this.Relax();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	void Relax()
    {
		anim.SetTrigger("Relax");
    }

	void Interactive()//防止我重复点击
    {
		if(!inInteractive)
        {
			inInteractive = true;
			StartCoroutine(DoInteractive());
        }
    }

	IEnumerator DoInteractive()//携程
    {
		yield return FaceToPlayer();
		if(NpcManager.Instance.Interactive(npc))//交互成功才播放动画
        {
			anim.SetTrigger("Talk");
        }
		yield return new WaitForSeconds(3f);//3秒之内无法再次点击
		inInteractive = false;
    }

	IEnumerator FaceToPlayer()//面向玩家
	{
		Vector3 faceTo = (User.Instance.CurrentCharacterObject.transform.position - this.transform.position).normalized;
		while(Mathf.Abs(Vector3.Angle(this.gameObject.transform.forward, faceTo))>5)//小于5度就不转了
        {
			this.gameObject.transform.forward = Vector3.Lerp(this.gameObject.transform.forward, faceTo, Time.deltaTime * 5f);
			yield return null;
        }
    }

	void OnMouseDown()//点下进行交互，打开界面
    {
		//如果直接点击Npc也会自动寻路（大于2米）
		if (Vector3.Distance(this.transform.position, User.Instance.CurrentCharacterObject.transform.position) > 2f)
        {
			User.Instance.CurrentCharacterObject.StartNav(this.transform.position);
        }
		Interactive();
    }

	private void OnMouseOver()
    {
		Highlight(true);
    }

	private void OnMouseEnter()
    {
		Highlight(true);
    }

	private void OnMouseExit()
    {
		Highlight(false);
    }

	void Highlight(bool highlight)
    {
		if(highlight)//如果鼠标移上去，变白光
        {
			if (renderer.sharedMaterial.color != Color.white)
				renderer.sharedMaterial.color = Color.white;
        }
		else//如果移除，变为原始色
        {
			if (renderer.sharedMaterial.color != orignColor)
				renderer.sharedMaterial.color = orignColor;
        }

    }
}

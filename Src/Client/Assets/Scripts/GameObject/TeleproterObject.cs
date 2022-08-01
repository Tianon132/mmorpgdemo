using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Services;
using Common.Data;

public class TeleproterObject : MonoBehaviour {

	public int ID;
	Mesh mesh = null;

	// Use this for initialization
	void Start () {
		this.mesh = this.GetComponent<MeshFilter>().sharedMesh;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other)
	{
		PlayerInputController playerInputController = other.GetComponent<PlayerInputController>();
		if (playerInputController != null && playerInputController.isActiveAndEnabled)
		{
			TeleporterDefine td = DataManager.Instance.Teleporters[this.ID];

			if (td == null)
			{
				Debug.LogErrorFormat("TeleporterObject: Character:[{0}] Enter Teleporter [{1}], But TeleporterDefine not existed", playerInputController.character.Info.Name, this.ID);
				return;
			}

			Debug.LogFormat("TeleporterObject: Character:[{0}] Enter Teleporter [{1} : {2}]", playerInputController.character.Info.Name, td, ID, td.Name);

			if (td.LinkTo > 0)//有目标传送点（1-8）时，发送传送消息
			{
				if (DataManager.Instance.Teleporters.ContainsKey(td.LinkTo))//判断目标位置是否存在
					MapService.Instance.SendMapTeleporter(this.ID);
				else
					Debug.LogErrorFormat("Teleporter ID:{0} LinkID:{1} error", td.ID, td.LinkTo);
			}
		}
	}

	//**************意思是只在此编译器模式下有效****************
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;//绘制轮廓渲染
		if(this.mesh != null)
		{
			Gizmos.DrawWireMesh(this.mesh, this.transform.position + Vector3.up * this.transform.localScale.y * .5f, this.transform.rotation, this.transform.localScale);
		}
		UnityEditor.Handles.color = Color.red;//绘制方向小箭头
		UnityEditor.Handles.ArrowHandleCap(0, this.transform.position, this.transform.rotation, 1f, EventType.Repaint);
	}
#endif
}

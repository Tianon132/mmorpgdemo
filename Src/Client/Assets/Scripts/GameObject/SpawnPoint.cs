using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Gizmos 线框

[ExecuteInEditMode]
public class SpawnPoint : MonoBehaviour {

	Mesh mesh = null;
	public int ID;

	// Use this for initialization
	void Start () {
		this.mesh = this.GetComponent<MeshFilter>().sharedMesh;//sharedmesh影响的是所有的mesh，而mesh是临时的单个变量
	}
	
	// Update is called once per frame
	void Update () {
		
	}

#if UNITY_EDITOR
	void OnDrawGizmos()//绘制线框
    {
		Vector3 pos = this.transform.position + Vector3.up * this.transform.localScale.y * .5f;//向上移动一个半径，让底部正好在球的底端
		Gizmos.color = Color.red;
		if (this.mesh != null)
			Gizmos.DrawWireMesh(this.mesh, pos, this.transform.rotation, this.transform.localScale);//在原球的位置上方绘制同球的线框

		//加一个方向箭头，这里是指刷怪的方向
		UnityEditor.Handles.color = Color.red;
		UnityEditor.Handles.ArrowHandleCap(0, pos, this.transform.rotation, 1f, EventType.Repaint);
				//Handles.ArrowHandleCap： 绘制一个类似于移动工具所用箭头的箭头
				//EventType.Repaint： 重绘事件。每一帧都发送一个。首先处理其他所有事件，然后再发送此重绘事件。
		UnityEditor.Handles.Label(pos, "SpwanPoint:" + this.ID);
				//Handles.Label: 在三维空间中制作文本标签。标签没有用户交互，不会捕捉鼠标单击，并且始终以正常样式呈现。
	}
#endif
}

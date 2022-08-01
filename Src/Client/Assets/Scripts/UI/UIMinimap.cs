using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Models;
using Managers;

public class UIMinimap : MonoBehaviour {

	public Image minimap;
	public Image arrow;
	public Text mapName;
	public Collider minimapBoundingBox;

	private Transform playerTransform;

	// Use this for initialization
	void Start () {
		MinimapManager.Instance.minimap = this;
		UpdateMap();
	}

	//初始化，获得地图信息
	public void UpdateMap()
    {
		this.mapName.text = User.Instance.CurrentMapData.Name;//当前城镇的名字

		//if(this.minimap.overrideSprite == null)//不为空就加载，但在第17节之后就不再适用了
		this.minimap.overrideSprite = MinimapManager.Instance.LoadCurrentMinimap();

		this.minimap.SetNativeSize();//加载图片后，让它加载为原图像大小
		this.minimap.transform.localPosition = Vector3.zero;

		this.minimapBoundingBox = MinimapManager.Instance.MinimapBoundingBox;//得到包围盒
		this.playerTransform = null;
		//this.playerTransform = User.Instance.CurrentCharacterObject.transform;//加载到缓存，而不是每一帧去访问单例，这样很消耗资源    --使用Update()第一个If来获取信息，因为再次进入可能会容易获取不到
	}
	
	// Update is called once per frame
	void Update () {
		if (playerTransform == null)
			playerTransform = MinimapManager.Instance.PlayerTransform;

		if (minimapBoundingBox == null || playerTransform == null)//检测是否为空绑定，脚本执行顺序导致的
			return;

		float realWidth = minimapBoundingBox.bounds.size.x;
		float realHeight = minimapBoundingBox.bounds.size.z;//真实的宽度和高度

		float relaX = playerTransform.position.x - minimapBoundingBox.bounds.min.x;
		float relaY = playerTransform.position.z - minimapBoundingBox.bounds.min.z;//宽度和高度的相对坐标，unity是x和z

		float pivotX = relaX / realWidth;
		float pivotY = relaY / realHeight;//利用中心点来设置位置

		this.minimap.rectTransform.pivot = new Vector2(pivotX, pivotY);
		this.minimap.rectTransform.localPosition = Vector2.zero;//为了让小地图动起来

		//小箭头 旋转
		this.arrow.transform.eulerAngles = new Vector3(0, 0, -playerTransform.eulerAngles.y);//欧拉角
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Models;

namespace Managers
{
    class MinimapManager : Singleton<MinimapManager>
    {
        public UIMinimap minimap;//在UIMinimap的Start函数中赋值

        private Collider minimapBoundingBox;
        public Collider MinimapBoundingBox
        {
            get { return minimapBoundingBox; }
        }

        public Transform PlayerTransform
        {
            get
            {
                if (User.Instance.CurrentCharacterObject == null)
                    return null;
                return User.Instance.CurrentCharacterObject.transform;
            }
        }

        //作用：加载Minimap给当前的UI进行使用
        public Sprite LoadCurrentMinimap()
        {
            return Resloader.Load<Sprite>("UI/Minimap/" + User.Instance.CurrentMapData.Minimap);
        }

        public void UpdateMinimap(Collider minimapBoundingBox)//管理器告诉小地图，要更新了
        {
            this.minimapBoundingBox = minimapBoundingBox;//更新包围盒
            if (this.minimap != null)
                this.minimap.UpdateMap();
        }
    }
}

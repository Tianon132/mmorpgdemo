using UnityEngine;
using UnityEngine.AI;

public class NavPathRenderer : MonoSingleton<NavPathRenderer>
{
    LineRenderer pathRenderer;
    NavMeshPath path;

    void Start()
    {
        pathRenderer = this.GetComponent<LineRenderer>();
        pathRenderer.enabled = false;
    }

    public void SetPath(NavMeshPath path, Vector3 target)
    {
        this.path = path;
        if (this.path == null)
        {
            pathRenderer.enabled = false;
            pathRenderer.positionCount = 0;//不寻路时把Line隐藏掉。把点清掉
        }
        else
        {
            pathRenderer.enabled = true;
            pathRenderer.positionCount = path.corners.Length + 1;//corners是拐点，+1是因为终点没有算在里面
            pathRenderer.SetPositions(path.corners);//s 批量设置一组拐点
            pathRenderer.SetPosition(pathRenderer.positionCount - 1, target);//设置终点
            for (int i = 0; i < pathRenderer.positionCount; i++)
            {
                pathRenderer.SetPosition(i, pathRenderer.GetPosition(i) + Vector3.up * 0.2f);//所有的点设置一个偏移，向上偏，否则会穿插在地面里
            }
        }
    }
}

using UnityEngine;


public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool global = true;
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance =(T)FindObjectOfType<T>();
            }
            return instance;
        }

    }

//    void Start()
    void Awake() //第20节，背包提到，修改为Awake，因为执行的优先级更高
    {    
        //Debug.LogWarningFormat("{0}[{1}] Awake", typeof(T), this.GetInstanceID());
        if (global)
        {
            if(instance != null && instance != this.gameObject.GetComponent<T>())
            {
                Destroy(this.gameObject);
                return;
            }//如果发现自己是多余的，就销毁

            DontDestroyOnLoad(this.gameObject);//如果是全局对象就不销毁，保护切换的时候保留对象
            instance = this.gameObject.GetComponent<T>();
        }

        this.OnStart();
    }

    protected virtual void OnStart()//虚函数--父类，防止调用时直接把start给覆盖掉
    {

    }
}
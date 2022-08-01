using UnityEngine;

public class InputBox : MonoBehaviour {

	static Object cacheObject = null;

	public static UIInputBox Show(string message, string title = "", string btnOk = "", string btnCancel = "", string  emptyTips = "")
    {
        if(cacheObject == null)
        {
            cacheObject = Resloader.Load<Object>("UI/UIInputBox");
        }

        GameObject go = Instantiate(cacheObject) as GameObject;
        UIInputBox inputBox = go.GetComponent<UIInputBox>();
        inputBox.Init(title, message, btnOk, btnCancel, emptyTips);
        return inputBox;
    }
}

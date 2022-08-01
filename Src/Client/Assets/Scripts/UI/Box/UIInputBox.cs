using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIInputBox : MonoBehaviour {

	public Text title;
	public Text message;
	public Text tips;
	public Button buttonYes;
	public Button buttonNo;
	public InputField input;

	public Text buttonYesTitle;
	public Text buttonNoTitle;

	public delegate bool SubmitHandler(string inputText, out string tips);//一对多委托
	public event SubmitHandler OnSubmit;
	public UnityAction OnCancel;//一对一委托

	public string emptyTips;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Init(string title, string message, string btnOK = "", string btnCancel = "", string emptyTips = "")
    {
		if (!string.IsNullOrEmpty(title)) this.title.text = title;
		this.message.text = message;
		this.tips.text = null;
		this.OnSubmit = null;
		this.emptyTips = emptyTips;

		if (!string.IsNullOrEmpty(btnOK)) this.buttonYesTitle.text = btnOK;
		if (!string.IsNullOrEmpty(btnCancel)) this.buttonNoTitle.text = btnCancel;

		this.buttonYes.onClick.AddListener(OnClickYes);
		this.buttonNo.onClick.AddListener(OnClickNo);
    }

	void OnClickYes()
    {
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Confirm);
		this.tips.text = "";
		if(string.IsNullOrEmpty(input.text))
        {
			this.tips.text = this.emptyTips;
			return;
        }
		if(OnSubmit != null)
        {
			string tips;
			if(!OnSubmit(this.input.text, out tips))
            {
				this.tips.text = tips;
				return;
            }
        }
		Destroy(this.gameObject);
    }

	void OnClickNo()
    {
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);
		Destroy(this.gameObject);
		if (this.OnCancel != null)
			this.OnCancel();
    }
}

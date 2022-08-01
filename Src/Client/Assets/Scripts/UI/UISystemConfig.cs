using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISystemConfig : UIWindow {

	public Image musicOff;
	public Image soundOff;

	public Toggle toggleMusic;
	public Toggle toggleSound;

	public Slider sliderMusic;
	public Slider sliderSound;

	// Use this for initialization
	void Start () {
		this.toggleMusic.isOn = Config.MusicOn;
		this.toggleSound.isOn = Config.SoundOn;//开关
		this.sliderMusic.value = Config.MusicVolume;
		this.sliderSound.value = Config.SoundVolume;//音量大小
	}


    public override void OnYesClick()
    {
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
		PlayerPrefs.Save();//Unity内部的配置  保存
		base.OnYesClick();
    }

	//音乐开关切换
	public void MusicToggle(bool on)
    {
		musicOff.enabled = !on;//关的图片打开或关闭由开关on反向决定
		Config.MusicOn = on;
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

	//音效开关切换
	public void SoundToogle(bool on)
    {
		soundOff.enabled = !on;
		Config.SoundOn = on;
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
	}

	//音乐音量
	public void MusicVolume(float vol)
    {
		Config.MusicVolume = (int)vol;
		PlaySound();
    }

	//音效音量
	public void SoundVolume(float vol)
    {
		Config.SoundVolume = (int)vol;
		PlaySound();
	}

	float lastPlay = 0;
	private void PlaySound()
    {
		if(Time.realtimeSinceStartup - lastPlay > 0.1)//每0.1秒播放一次，实时播放改变的音量效果
        {
			lastPlay = Time.realtimeSinceStartup;
			SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
		}
	}
}

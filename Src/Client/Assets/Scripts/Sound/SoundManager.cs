using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoSingleton<SoundManager> {

	public AudioMixer audioMixer;

	public AudioSource musicAudioSource;
	public AudioSource soundAudioSource;

	const string MusicPath = "Music/";
	const string SoundPath = "Sound/";

	private bool musicOn;
	public bool MusicOn
    {
        get { return musicOn; }
		set
        {
			musicOn = value;
			this.MusicMute(!musicOn);//如果关，就立刻设置静音
        }
    }

	private bool soundOn;
	public bool SoundOn
	{
		get { return soundOn; }
		set
		{
			soundOn = value;
			this.SoundMute(!soundOn);
		}
	}

	private int musicVolume;
	public int MusicVolume
	{
		get { return musicVolume; }
		set
		{
			musicVolume = value;
			if (musicOn) this.SetVolume("MusicVolume", musicVolume);//静音就不操作
		}
	}

	private int soundVolume;
	public int SoundVolume
	{
		get { return soundVolume; }
		set
		{
			soundVolume = value;
			if (soundOn) this.SetVolume("SoundVolume", soundVolume);
		}
	}

	// Use this for initialization
	void Start () {
		this.MusicVolume = Config.MusicVolume;
		this.SoundVolume = Config.SoundVolume;
		this.MusicOn = Config.MusicOn;
		this.SoundOn = Config.SoundOn;
	}
	
	public void MusicMute(bool mute)
    {
		this.SetVolume("MusicVolume", mute ? 0 : musicVolume);//true为静音
    }

	public void SoundMute(bool mute)
    {
		this.SetVolume("SoundVolume", mute ? 0 : soundVolume);
	}

	private void SetVolume(string name, int value)
    {
		float volume = value * 0.5f - 50f;//slider范围是1-100，换算成音贝
		this.audioMixer.SetFloat(name, volume);//调整音量是调整AudioMixer中的相对应的音量
    }
	
	public void PlayMusic(string name)
    {
		AudioClip clip = Resloader.Load<AudioClip>(MusicPath + name);
		if(clip == null)
        {
			Debug.LogWarningFormat("PlayMusic : {0} not existed.", name);
			return;
        }
		if(musicAudioSource.isPlaying)
		{ 
			musicAudioSource.Stop();
		}

		musicAudioSource.clip = clip;
		musicAudioSource.Play();
    }

	public void PlaySound(string name)
    {
		AudioClip clip = Resloader.Load<AudioClip>(SoundPath + name);
		if(clip == null)
        {
			Debug.LogWarningFormat("PlaySound : {0} not existed.", name);
			return;
        }
		soundAudioSource.PlayOneShot(clip);//播放一次
    }

	protected void PlayClipOnAudioSource(AudioSource source, AudioClip clip, bool isLoop)
    {

    }
}

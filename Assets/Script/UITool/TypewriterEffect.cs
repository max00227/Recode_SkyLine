using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 此脚本是能够将文本字符串随着时间打字或褪色显示。
/// </summary>

[RequireComponent(typeof(Text))]
[AddComponentMenu("Typewriter Effect")]
public class TypewriterEffect : MonoBehaviour
{
	public UnityEvent myEvent;
	public float charsPerSecond = 0;
	// public AudioClip mAudioClip;             // 打字的声音，不是没打一个字播放一下，开始的时候播放结束就停止播放
	public bool isActive = false;

	private float timer;
	private string words;
	private Text mText;

	void Start(){
		mText = transform.GetComponent<Text> ();
	}

	public void SetTalkContent(string content){
		//myEvent = new UnityEvent();
		words = content;
		timer = 0;
		isActive = true;
	}

	void OnStartWriter()
	{
		if (isActive)
		{
			try
			{
				mText.text = words.Substring(0, (int)((1/charsPerSecond) * timer));
				timer += Time.deltaTime;
			}
			catch (Exception)
			{
				OnFinish();
			}
		}
	}

	public void OnFinish()
	{
		isActive = false;
		timer = 0;
		mText.text = words;
		/*try
		{
			myEvent.Invoke();
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
		}*/
	}

	void Update()
	{
		OnStartWriter();
	}
}
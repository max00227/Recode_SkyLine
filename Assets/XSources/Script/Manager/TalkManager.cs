using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TalkManager : Singleton<TalkManager> {
	[SerializeField]
	Text talkText;

	[SerializeField]
	GameObject talkButton;

	[SerializeField]
	GameObject TalkUI;

	Queue talkQueue;

	Action onTalkDelegate;

	int StopIdx;

	[HideInInspector]
	public bool isTalking = false;

	bool isActive = false;

	Canvas closedFocus;

	public void SetTalkData(string[] talkDatas, Canvas _closedFocus=null){
		Debug.Log ("SetTalk");
		talkQueue = new Queue ();
		closedFocus = _closedFocus;
		foreach (string talkData in talkDatas) {
			string[] talkDataSpl = talkData.Split (',');

			if (talkDataSpl.Length != 3) {
				Debug.Log ("Set Error");
				return;
			}
			TalkData data = SetData (talkData);


			talkQueue.Enqueue (data);
		}
		Time.timeScale = 0;
	}

	void SetTalkData(int CharId,int ContentId, int Pos, int talkOrder = 0){
		
		Dictionary<string ,object> talkDataDic = new Dictionary<string, object> ();
		//Dictionary<int, >
		talkDataDic.Add ("CharId", CharId);
		//test
		talkDataDic.Add ("TalkContent", ContentId.ToString());
		//test
		if (Pos == 0) {
			talkDataDic.Add ("TalkerPos", TalkPos.Left);
		} else if (Pos == 1) {
			talkDataDic.Add ("TalkerPos", TalkPos.Right);
		} else {
			talkDataDic.Add ("TalkerPos", TalkPos.Center);
		}
		Debug.Log (ContentId);

		talkQueue.Enqueue (talkDataDic);
	}

	public void TalkStart(){
		isTalking = true;
		TalkUI.SetActive (true);
		Next();
	}

	public void Next(){
		Debug.Log ("Next");		
		if (talkQueue.Count == 0 && isActive == false) {
			Debug.Log ("Close Talk");
			TalkUI.SetActive (false);
			isTalking = false;
			if (closedFocus != null) {
				closedFocus.enabled = true;
			}
			return;
		}

		if (isActive) {
			OnFinish ();
		} 
		else {
			if (talkQueue.Count > 0) {
				var data = (TalkData)talkQueue.Dequeue ();
				SetTalkContent (data.talKContent);
				talkText.gameObject.SetActive (true);
			}
		}
	}




	public float charsPerSecond = 0;
	// public AudioClip mAudioClip;             // 打字的声音，不是没打一个字播放一下，开始的时候播放结束就停止播放


	private float timer;
	private float recTime;
	private string words;

	public void SetTalkContent(string content){
		//myEvent = new UnityEvent();
		words = content;
		recTime = Time.realtimeSinceStartup;
		isActive = true;
	}

	void OnStartWriter()
	{
		if (isActive)
		{
			try
			{
				talkText.text = words.Substring(0, (int)((1/charsPerSecond) * timer));
				timer = Time.realtimeSinceStartup - recTime;
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
		//isTalking = false;
		timer = 0;
		talkText.text = words;
	}

	public void CloseTalk(){
	
	}

	void Update()
	{
		OnStartWriter();
	}

	public TalkData SetData(string talkData){
		TalkData data = new TalkData ();
		string[] talkDataSpl = talkData.Split (',');

		if (!System.Int32.TryParse (talkDataSpl [0],out data.charaId)) {
			data.charaId = 0;
		}
		data.talKContent = talkDataSpl [1];


		if (talkDataSpl[2] == "0") {
			data.talkerPos = TalkPos.Left;
		} else if (talkDataSpl[2] == "1") {
			data.talkerPos = TalkPos.Right;
		} else {
			data.talkerPos = TalkPos.Center;
		}
		return data;
	}
}

public enum TalkPos{
	Left,
	Right,
	Center
}

public struct TalkData{
	public int charaId;
	public string talKContent;
	public TalkPos talkerPos;
}
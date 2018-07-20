using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using WndEditor;

public class testLoader : MonoBehaviour {
	string path = "/ClientData/ClientData.txt";
	string teamDataPath = "/ClientData/TeamData.txt";

	static string json = "";
	[SerializeField]
	ParameterReciever reciever; 

	[SerializeField]
	StepControaller process;

	[SerializeField]
	WndEditor.ItemListContainer container;

	int[] ia = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

	// Use this for initialization
	void Awake () {
		//container.SetListItemTest(10);
		//TutorialManager.Instance.SetTutorialStep (process.step);

		test.ClientDataLoader.readClientData();
		StreamReader sr = new StreamReader (Application.dataPath + teamDataPath);
		json = sr.ReadToEnd();

		MyUserLargeData userData = JsonConversionExtensions.ConvertJson<MyUserLargeData>(json);
		MyUserData.UpdataUserdata (userData);
		Debug.Log (MyUserData.GetTeamListData().Count);
		//Debug.Log (largeData.TeamListData.team);

		/*StreamReader sr = new StreamReader (Application.dataPath + path);
		string js = sr.ReadToEnd ();*/
		//reciever.ResolveReq (js);
		//int[] nia = new int[5];
		//Array.Copy (ia, 1, nia, 0, 5);

		//Debug.Log ("Length : "+nia.Length);
		/*foreach (int i in nia) {
			Debug.Log (i);
		}*/
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void WndTest(GameObject go){
		if (TutorialManager.Instance.focusGameObject == go) {
			Debug.Log ("testWnd");
		}
	}

	public void WndTest2(GameObject go){
		if (TutorialManager.Instance.focusGameObject == go) {
			Debug.Log ("testWnd2");
		}
	}
	public void WndTest3(GameObject go){
		if (TutorialManager.Instance.focusGameObject == go) {
			Debug.Log ("testWnd3");
		}
	}

	public void WndTest4(GameObject go){
		if (TutorialManager.Instance.focusGameObject == go) {
			Debug.Log ("testWnd4");
		}
	}

	public void back(){
		WindowManager.Instance.BackWnd ();
	}

	public void testAbility(){
		/*var data = AbilityConvert.GetCharaAbility (MasterDataManager.GetCaraLargeData[1],12);

		Debug.Log (string.Format ("ATK:{0} , DEF:{1} , MATK:{2} , MDEF:{3} , HP:{4}", data["Atk"], data["Def"], data["MAtk"], data["MDef"], data["Hp"]));
		Debug.Log (MasterDataManager.GetCaraLargeData [1].name);
		foreach (var v in MasterDataManager.GetCaraLargeData [1].Act) {
			Debug.Log (v);		
		}*/
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace WndEditor
{
	[RequireComponent(typeof(ParameterReciever))]
	public class WindowController : MonoBehaviour {
		public string wndName;

		[SerializeField]
		bool isPost = true;

		[SerializeField]
		string serviceApi = "";

		public bool isTop = true;

		public enum wndType
		{
			baseWnd=1,

		
		}

		bool isSetTutorial = false;

		[SerializeField]
		ParameterReciever reciever;

		bool isConnect = false;

		public List<object> param;

		
		public void SetWindow (string paramter = "") {
			
			if (string.IsNullOrEmpty (serviceApi)) {
				if (string.IsNullOrEmpty (paramter)) {
					return;
				} else {
					reciever.ResolveReq (paramter);
				}
			} 
			else {
				if (isPost == true) {
					//string json = JsonConvert.SerializeObject(clientDataDic);
				} 
				else {
				}
			}

			if (gameObject.GetComponent<WndTutorial> () && !PlayerPrefsController.HasKey (wndName)) {
				if (isSetTutorial == false) {
					TutorialManager.Instance.SetTutorialStep (gameObject.GetComponent<WndTutorial> ().tutorialSteps);
					isSetTutorial = true;
				}

				TutorialManager.Instance.ProcessStart ();
			}
			gameObject.SetActive (true);
		}
	}
}

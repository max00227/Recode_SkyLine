using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WndEditor;

public class WindowManager : Singleton<WindowManager> {
	string path = "/ClientData/ClientData.txt";

	string js;

	int maxHistoryCount = 10;

	public WindowController currentMainWnd { get; private set;}

	readonly string sourcePath = "Wnd/";

	LinkedList<WindowController> wndHistory;

	List<WindowController> loadedWnd;

	public WindowController currentPopupWnd { get; private set;}

	List<object> prevParamter;

	[SerializeField]
	Transform mainWndPool;

	[SerializeField]
	Transform popupWndPool;

	bool popupWndOpen = false;

	public override void Init(){
		wndHistory = new LinkedList<WindowController> ();
		loadedWnd = new List<WindowController> ();

		StreamReader sr = new StreamReader (Application.dataPath + path);
		js = sr.ReadToEnd ();
	}


	public void TranWnd(string wndName, string paramter = ""){
		AddWnd (wndName);
	}

	public void PopupWnd(string wndName, string parameter = ""){
		AddWnd (wndName, true);
	}

	void AddWnd(string wndName, bool isPopup=false, string parameter = ""){
		if (currentMainWnd!=null && currentMainWnd.wndName == wndName) {
			Debug.Log("Is Open");
			return;
		}
		WindowController window = CheckAddedWnd (wndName, isPopup);
		if (window == null) {
			#if !USE_AB
			GameObject go =Instantiate (Resources.Load (sourcePath + wndName), transform.position, Quaternion.identity) as GameObject;
			window = go.GetComponent<WindowController>();
			#else
			#endif

			window.transform.parent = isPopup ? popupWndPool : mainWndPool;
			window.transform.localPosition = Vector3.zero;
			window.transform.localScale = Vector3.one;
			if (isPopup) {
				popupWndOpen = true;
			}
			loadedWnd.Add (window);
		} 

		if (isPopup) {
			popupWndPool.GetComponent<TweenTool> ().showGameObject = window.gameObject;
			popupWndPool.GetComponent<TweenTool> ().PlayForward ();
		} 
		else {
			if (currentMainWnd != null) {
				currentMainWnd.gameObject.SetActive (false);
				prevParamter = currentMainWnd.param;
			}
				
			wndHistory.AddLast (window);
			currentMainWnd = window;
			currentMainWnd.SetWindow(js);

			if (wndHistory.Count > maxHistoryCount) {
				wndHistory.RemoveFirst ();
			}
		}
	}

	WindowController CheckAddedWnd(string wndName, bool isPopup=false){
		foreach (var wnd in loadedWnd) {
			if (wnd.wndName == wndName) {
				return wnd;
			}
		}
		return null;
	}

	public void BackWnd(){
		if (wndHistory.Count > 1 && currentMainWnd != wndHistory.Last.Previous.Value) {
			wndHistory.Last.Value.gameObject.SetActive (false);

			wndHistory.Last.Previous.Value.gameObject.SetActive (true);

			currentMainWnd = wndHistory.Last.Previous.Value;

			wndHistory.RemoveLast ();
		}
	}

	public void ClosePopupWnd(){
		popupWndPool.GetComponent<TweenTool> ().PlayReverse();
		popupWndOpen = false;
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.B)||Input.GetKeyDown(KeyCode.Escape)) {
			if (popupWndOpen == true) {
				ClosePopupWnd ();
			} 
			else {
				if (currentMainWnd.isTop == false) {
					BackWnd ();
				}
				else {
					
				}
			}
		}	
	}
}

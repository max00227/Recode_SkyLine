using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TutorialManager: Singleton<TutorialManager>
{
	[SerializeField]
	Image mask;

	public static Action GuidCallback;

	LinkedList<TutorialStep> tutorialSteps;

	Canvas canvas;


	TutorialStep tutorialStep;

	int stepOrder = 0;

	public GameObject focusGameObject; 

	Vector4 maskAlpha = new Vector4(0,0,0,0.6f);

	Transform[] orgParant;
	int[] orgSibling;

	GameObject[] recListArray;

	public override void Init ()
	{
		tutorialSteps = new LinkedList<TutorialStep> ();
	}

	private void SetMaskAndBtnHiglight(TutorialProcess lastProcess, TutorialProcess nextProcess, GameObject[] lastListArray = null,GameObject[] nextListArray = null)
	{
		SetLastProcess (lastProcess, lastListArray);
		SetNextProcess (nextProcess, nextListArray);
	}

	private void SetLastProcess(TutorialProcess process, GameObject[] listArray = null){
		GameObject focus = process.focusGameObject;
		GameObject[] array = process.gameObjectAry;
		if (listArray != null) {
			focus = listArray [listArray.Length - 1];
			array = listArray;
		} 
			
		if (process._tutorialType != TutorialType.Talk) {
			if (listArray != null) {
				if (array.Length > 0) {
					for (int i = orgParant.Length - 1; i >= 0; i--) {
						array [i].GetComponent<RectTransform> ().SetParent (orgParant [i].transform);
						array [i].transform.SetSiblingIndex (orgSibling [i]);
					}
					recListArray = null;
				}
			} else {
				if (array == null) {
					return;
				} else {
					if (array.Length > 0) {
						for (int i = array.Length - 1; i >= 0; i--) {
							array [i].GetComponent<RectTransform> ().SetParent (orgParant [i]);
							array [i].transform.SetSiblingIndex (orgSibling [i]);
						}
					}
				}
			}
		}

		if (focus) {
			Destroy (focus.GetComponent<GraphicRaycaster> ());
			Destroy (focus.GetComponent<Canvas> ());
		}
	}

	private void SetNextProcess(TutorialProcess process, GameObject[] listArray = null){
		GameObject focus = process.focusGameObject;
		GameObject[] array = process.gameObjectAry;
		if (listArray != null) {
			focus = listArray [listArray.Length - 1];
			array = listArray;
		} 

		if (focus) {
			Canvas nextCanvas = focus.GetComponent<Canvas> ();
			if (nextCanvas) {
				nextCanvas.overrideSorting = true;
			} else {
				focus.AddComponent<Canvas> ().overrideSorting = true;
			}

			focus.GetComponent<Canvas> ().sortingOrder = 1;
			focusGameObject = null;

			if (process._tutorialType == TutorialType.Button && process.listnerType == EventListnerType.onClick) {
				focusGameObject = focus;
			} 

			focus.AddComponent<GraphicRaycaster> ();
		}

		if (process._tutorialType != TutorialType.Talk) {
			if (listArray != null) {
				if (array.Length > 0) {
					orgParant = new Transform[array.Length - 1];
					orgSibling = new int[array.Length - 1];

					for (int i = 0; i < array.Length - 1; i++) {
						orgParant [i] = array [i].transform.parent;
						orgSibling [i] = array [i].transform.GetSiblingIndex ();
						array [i].GetComponent<RectTransform> ().SetParent (focus.transform);
					}
					recListArray = array;
				}
			} else {
				if (array == null) {
					return;
				} else {
					if (array.Length > 0) {

						orgParant = new Transform[array.Length];
						orgSibling = new int[array.Length];

						for (int i = 0; i < array.Length; i++) {
							orgParant [i] = array [i].transform.parent;
							orgSibling [i] = array [i].transform.GetSiblingIndex ();
							array [i].GetComponent<RectTransform> ().SetParent (focus.transform);
						}
					}
				}
			}
		}
	}

	public void SetTutorialStep(TutorialStep[] steps)
	{
		for (int i =steps.Length-1;i>=0;i--) {
			tutorialSteps.AddFirst (steps [i]);
		}
		SetTutorialProcess (tutorialSteps.First.Value);
	}

	private void SetTutorialProcess(TutorialStep step){
		if (step != null) {
			for (int i = 0; i < step._tutorialProcess.Length; i++) {
				if (i < step._tutorialProcess.Length - 1) {
					SetTutorialProcess (step._tutorialProcess [i], step._tutorialProcess [i + 1]);
				} else {
					SetTutorialProcess (step._tutorialProcess [i], new TutorialProcess ());
				}
			}

			ProcessStart ();
		}
	}

	public void ProcessStart()
	{
		
		stepOrder++;
		Debug.Log (string.Format ("<color=red>Step {0}</color>", stepOrder));


		mask.gameObject.SetActive (true);

		tutorialSteps.First.Value._tutorialProcess[0].focusGameObject.AddComponent<Canvas> ().overrideSorting = true;
		SetMaskAndBtnHiglight (new TutorialProcess (), tutorialSteps.First.Value._tutorialProcess [0], null, GetListArray (tutorialSteps.First.Value._tutorialProcess [0]));
		
		tutorialSteps.RemoveFirst ();
	}

	private void SetTutorialProcess(TutorialProcess process,TutorialProcess nProcess){
		switch (process._tutorialType) {
		case TutorialType.Button:
		case TutorialType.ButtonForWnd:
			SetButtonProcess (process, nProcess);
			break;
		case TutorialType.Talk:
			SetTalkProcess (process, nProcess);
			break;
		case TutorialType.List:
			SetListProcess (process, nProcess);
			break;
		}
	}

	private void SetButtonProcess(TutorialProcess process ,TutorialProcess nProcess){
		


		if (process.listnerType == EventListnerType.onClick) {
			TutorialTriggerListener.GetListener (process.focusGameObject).onClick += go => {
				if (process.focusGameObject == focusGameObject) {
					if (nProcess._tutorialType == TutorialType.End) {
						StepEnd (process);
					} else {
						NextProcess (process, nProcess);
					}
				}
			};
		}
		else{
			if (process.gameObjectAry.Length > 1) {
				GameObject from = process.gameObjectAry [0];
				GameObject to = process.gameObjectAry [process.gameObjectAry.Length - 1];



				TutorialTriggerListener.GetListener (from).onBeginDrag += go => {
					CanvasManager.Instance.SetRaycastCanvas (process.focusGameObject.GetComponent<Canvas> ());
					foreach (RaycastResult result in CanvasManager.Instance.GetRaycastResult()) {
						Debug.Log (result.gameObject.name);
					}
				};
				TutorialTriggerListener.GetListener (from).onEndDrag += go => {
					var results = CanvasManager.Instance.GetRaycastResult ();

					if (results.Count > 0) {
						foreach (RaycastResult result in results) {
							if (result.gameObject == to) {
								if (nProcess._tutorialType == TutorialType.End) {
									StepEnd (process);
									return;
								} else {
									NextProcess (process, nProcess);
									return;
								}
							}
						}
						SetControlError (process.focusGameObject, process.data);
					} else {
						SetControlError (process.focusGameObject, process.data);
					}
				};
			} else {
				if (process.gameObjectAry.Length == 0) {
					Debug.Log ("Have Not Drag Begin And End");
				}
				else {
					Debug.Log ("Have Not Drag End");
				}
			}
		}
	}

	private void SetControlError(GameObject focus, string[] data){
		TalkManager.Instance.SetTalkData(data, focus.GetComponent<Canvas>());
		focus.GetComponent<Canvas>().enabled=false;
		TalkManager.Instance.TalkStart();
	}

	private void SetTalkProcess(TutorialProcess process ,TutorialProcess nProcess){
		GameObject btn = process.focusGameObject;
		TalkManager.Instance.SetTalkData (process.data);

		TutorialTriggerListener.GetListener (btn.gameObject).onClick += go => {
			if(TalkManager.Instance.isTalking==false){
				if(nProcess._tutorialType == TutorialType.End){
					StepEnd(process);
				}
				else{
					NextProcess(process, nProcess);
				}
			}
		};
		TalkManager.Instance.SetTalkData (process.data);
	}


	private void SetListProcess(TutorialProcess process ,TutorialProcess nProcess){
		GameObject focus;

		if (process.focusGameObject.GetComponent<WndEditor.ItemListContainer> ()) {
			int childIdx;
			if (Int32.TryParse (process.data [0], out childIdx)) {
				if (process.focusGameObject.transform.childCount > childIdx) {
					focus = process.focusGameObject.transform.GetChild (childIdx).gameObject;
				} else {
					Debug.Log ("Have Not Child");
					return;
				}
			}
			else {
				Debug.Log ("Have Not Child");
				return;
			}
		}
		else {
			Debug.Log ("Set Error");
			return;
		}

		if (process.listnerType == EventListnerType.onClick) {
			TutorialTriggerListener.GetListener (focus).onClick += go => {
				if (TalkManager.Instance.isTalking == false) {
					if (nProcess._tutorialType == TutorialType.End) {
						StepEnd (process);
					} else {
						NextProcess (process, nProcess, recListArray);
					}
				}
			};
		} 
		else {
			if (process.gameObjectAry.Length > 1) {
				GameObject from = process.gameObjectAry [0];
				GameObject to = process.gameObjectAry [1];

				TutorialTriggerListener.GetListener (focus).onBeginDrag += go => {
					CanvasManager.Instance.SetRaycastCanvas (process.gameObjectAry[0].GetComponent<Canvas> ());
					foreach (RaycastResult result in CanvasManager.Instance.GetRaycastResult()) {
					}
				};
				TutorialTriggerListener.GetListener (focus).onEndDrag += go => {
					var results = CanvasManager.Instance.GetRaycastResult ();

					if (results.Count > 0) {
						foreach (RaycastResult result in results) {
							if (result.gameObject == to) {
								if (nProcess._tutorialType == TutorialType.End) {
									StepEnd (process);
									return;
								} else {
									NextProcess (process, nProcess, recListArray);
									return;
								}
							}
						}
						string[] startTwo = new string[process.data.Length-1];
						Array.Copy(process.data,1,startTwo,0,startTwo.Length);
						SetControlError(process.gameObjectAry[0],startTwo);
					} else {
						string[] startTwo = new string[process.data.Length-1];
						Array.Copy(process.data,1,startTwo,0,startTwo.Length);
						SetControlError(process.gameObjectAry[0],startTwo);
						Debug.Log ("End False");
					}
				};
			} else {
				if (process.gameObjectAry.Length == 0) {
					Debug.Log ("Have Not Drag Begin And End");
				}
				else {
					Debug.Log ("Have Not Drag End");
				}
			}
		}
	}


	private void NextProcess(TutorialProcess process ,TutorialProcess nProcess, GameObject[] listArray=null){
		GameObject btn = process.focusGameObject;
		GameObject[] gos = process.gameObjectAry;
		GameObject nBtn;
		GameObject[] ngos;

		btn.gameObject.SetActive (true);

		nBtn = nProcess.focusGameObject;
		ngos = nProcess.gameObjectAry;	
		if (nProcess._tutorialType != TutorialType.Wait) {
			nBtn.gameObject.SetActive (true);
		}

		if (process._tutorialType == TutorialType.Talk && nProcess._tutorialType != TutorialType.Talk) {
			Time.timeScale = 1;
		}

		if (nProcess._tutorialType == TutorialType.Talk) {
			SetMaskAndBtnHiglight (process, nProcess);
			TalkManager.Instance.TalkStart ();
		} else if (nProcess._tutorialType == TutorialType.Wait) {
			SetWaitProcess (process, nProcess);
		} else if (nProcess._tutorialType == TutorialType.List) {
			SetMaskAndBtnHiglight (process, nProcess, recListArray, GetListArray (nProcess));
		} else {
			SetMaskAndBtnHiglight (process, nProcess);
		}
	}

	public GameObject[] GetListArray(TutorialProcess process){
		if (process._tutorialType == TutorialType.List) {
			GameObject[] listArray = process.gameObjectAry;
			Array.Resize (ref listArray, process.gameObjectAry.Length + 1);

			GameObject focus;
			if (process.focusGameObject.GetComponent<WndEditor.ItemListContainer> ()) {
				int childIdx;
				if (Int32.TryParse (process.data [0], out childIdx)) {
					if (process.focusGameObject.transform.childCount > childIdx) {
						focus = process.focusGameObject.transform.GetChild (childIdx).gameObject;
					} else {
						Debug.Log ("Have Not Child");
						return null;
					}
				} else {
					Debug.Log ("Have Not Child");
					return null;
				}
			} else {
				Debug.Log ("Set Error");
				return null;
			}
			listArray [listArray.Length - 1] = focus;
			Array.Reverse (listArray);

			return listArray;
		} 
		else {
			return null;
		}
	}

	private void SetWaitProcess(TutorialProcess process ,TutorialProcess nProcess)
	{
		Debug.Log ("Set Wait");
		SetMaskAndBtnHiglight (process, nProcess);
		//SetMaskAndBtnHiglight (btn, nBtn, gos, ngos);
		Debug.Log ((nProcess.focusGameObject == null).ToString () + " , " + (nProcess.gameObjectAry.Length == 0).ToString ());
		if (nProcess.focusGameObject == null && nProcess.gameObjectAry.Length == 0) {
			mask.color = (Color)Vector4.zero;
		}
		Wait (nProcess);
	}

	private void Wait(TutorialProcess process){
		float waitTime;
		if (float.TryParse (process.data [0], out waitTime)) {
			StartCoroutine (TutorialWait (waitTime, process));
		} else {
			SetTutorialProcess (tutorialSteps.First.Value);
			return;
		}
	}

	IEnumerator TutorialWait(float waitTime, TutorialProcess process){
		Debug.Log ("Wait");
		yield return new WaitForSeconds (waitTime);

		StepEnd (process);
	}

	private void StepEnd(TutorialProcess process){
		mask.color = (Color)maskAlpha;
		SetMaskAndBtnHiglight (process, new TutorialProcess (), recListArray);
		if(tutorialSteps.Count>0){
			if (process._tutorialType == TutorialType.ButtonForWnd) {
				SetTutorialProcess (null);
			} else {
				SetTutorialProcess (tutorialSteps.First.Value);
			}
			return;
		}
		TutorialEnd (process.saveKey);
	}

	private void TutorialEnd(string key){
		if (!string.IsNullOrEmpty (key)) {
			PlayerPrefsController.SetInt (key, 1);
		}

		Debug.Log ("TutorialEnd");
		mask.gameObject.SetActive(false);
	}
}



public enum EventListnerType{
	onClick,
	onDrag
}

public enum TutorialType{
	End,
	Button,
	ButtonForWnd,
	Talk,
	Wait,
	List,
}

[Serializable]
public struct TutorialProcess{
	public string saveKey;

	public TutorialType _tutorialType;

	public GameObject focusGameObject;

	public EventListnerType listnerType;

	public GameObject[] gameObjectAry;

	public string[] data; 
}


/*
使用方法
TutorialType.Button
	FocusGameObject不能為Null
	ListenerType.onClick
		FocusGameObject為指定之按鈕
	ListenerType.onDrag
		FocusGameObject為空物件
		GameObjectAry第一個物件為起點最後一個物件為終點

TutorialType.Talk
	FocusGameObject為TalkManager之按鈕
	GameObjectAry為TalkManager之UI

	Data
	CharaId,TalkContent,CharaPosition

TutorialType.Wait
		若GameObjectAry有物件時FocusGameObject須設置空物件
		Data
		等待時間

TutorialType.List
	FocusGameObject為設置清單物件
	
	GameObjectAry第一個設置空物件，第二個物件為終點	

	Data
	清單Index	

PS. 每個Step只能有一個TutorialType.Talk及TutorialType.Wait
	同個Stop中有onDrag事件就不能有TutorialType.Talk

SaveKey
教學Key，用於教學記錄，設置在Step的最後一個Process
*/
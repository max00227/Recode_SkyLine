using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EventManager : MonoBehaviour {
	Button btn;
	// Use this for initialization
	void Awake () {
		btn = GetComponent<Button>();
		btn.onClick.AddListener(OnButtonClick);
	}


	public void OnButtonClick()
	{
		Debug.Log("click");
	}

}
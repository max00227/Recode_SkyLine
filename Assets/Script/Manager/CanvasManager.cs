using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class CanvasManager : Singleton<CanvasManager> {
	[SerializeField]
	Canvas mainCanvas;

	[SerializeField]
	Canvas defaultCanvas;


	// Use this for initialization
	public override void Init ()
	{
		mainCanvas = defaultCanvas;
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public List<RaycastResult> GetTutorialRaycastResult(Canvas _canvas,bool isTouch = false){
		List<RaycastResult> results = new List<RaycastResult> ();
		Canvas mainCanvas = _canvas;


		GraphicRaycaster m_Raycaster = mainCanvas.GetComponent<GraphicRaycaster> ();

		PointerEventData m_PointerEventData = new PointerEventData (EventSystem.current);

		if (!isTouch) {
			m_PointerEventData.position = Input.mousePosition;
		} else {
			m_PointerEventData.position = Input.GetTouch (0).position;
		}

		EventSystem.current.RaycastAll (m_PointerEventData, results);

		return results;
	}

	public List<RaycastResult> GetRaycastResult(bool isTouch = false){
		List<RaycastResult> results = new List<RaycastResult> ();

        GraphicRaycaster m_Raycaster = mainCanvas.GetComponent<GraphicRaycaster> ();

		PointerEventData m_PointerEventData = new PointerEventData(EventSystem.current);

		if (!isTouch) {
			m_PointerEventData.position = Input.mousePosition;
		} 
		else {
			m_PointerEventData.position = Input.GetTouch(0).position;
		}

        EventSystem.current.RaycastAll(m_PointerEventData, results);

        return results;
	}

	public void ResetCanvas(){
		mainCanvas = defaultCanvas;
	}

	public void SetRaycastCanvas(Canvas canvas){
		mainCanvas = canvas;
	}
}

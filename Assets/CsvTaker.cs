using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsvTaker : MonoBehaviour {
	string m_info;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartWWW(){
		//StartCoroutine (GetCsvData ("http://211.20.178.198/master-data/csv/event.csv"));
		Debug.Log ("StartWWW");
	}

	IEnumerator GetCsvData(string url){
		WWW www = new WWW (url);

		yield return www;

		if (www.error != null) {
			m_info = www.error;
			yield return null;
		}

		m_info = www.text;
		//Debug.Log (www.);
	}
}

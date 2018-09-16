using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class testAngle : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [MenuItem("GameObject/GetEularAngle", false, -1000)]
    static void GetLocalUelarAngle()
    {
        GameObject sel = Selection.activeGameObject;

        Debug.Log(sel.transform.localEulerAngles.x + " , " + sel.transform.localEulerAngles.y + " , " + sel.transform.localEulerAngles.z);
        Debug.Log(sel.transform.localPosition.x + " , " + sel.transform.localPosition.y + " , " + sel.transform.localPosition.z);

    }

    [MenuItem("GameObject/CalcEularAngle", false, -1000)]
    static void CalcEularAngle()
    {
        GameObject sel = Selection.activeGameObject;

        Vector3 f = sel.transform.GetChild(0).localPosition;
        Vector3 t = sel.transform.GetChild(1).localPosition;


        Vector3 relativePos = f - t;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        Debug.Log(rotation.x + " , " + rotation.y + " , " + rotation.z);
        sel.transform.GetChild(2).rotation = rotation;
        sel.transform.GetChild(2).GetChild(1).localPosition = new Vector3(0, 0, Vector3.Distance(f, t));
    }
}

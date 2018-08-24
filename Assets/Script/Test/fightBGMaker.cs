﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class fightBGMaker : MonoBehaviour {
    int groundRadio = 4;
    int maxHoriCount;
    int spacing = 3;

    [SerializeField]
    GameObject openWnd;

    List<GroundController> restrictArea = new List<GroundController>();
    List<GroundController> normalArea = new List<GroundController>();
    

    Vector4 colorRed = new Vector4(1, 0, 0, 1);

    Vector4 colorGreen = new Vector4(0, 1, 0, 1);

    Vector4 colorBlack = new Vector4(0, 0, 0, 1);

    Vector4 colorGold = new Vector4(0.7f, 0.7f, 0, 1);

    Vector4 colorSilver = new Vector4(0.6f, 0.6f, 0.6f, 1);

    Vector4 colorCopper = new Vector4(0.7f, 0.4f, 0.3f, 1);

    Vector4 colorWhite = new Vector4(1, 1, 1, 1);

    [SerializeField]
    Color[] playColor;

	Vector3 hitPoint;

	Vector3 cameraCenter;

	Vector3 centerPos;

	GameObject _player = null;

	bool isSet = false;

    Color _selectionChar;

	GameObject bgPool = null;

    Vector3 rayOrg;

    Collider hitOrg;

    Collider hitDir;

	float objDis = 0f;

    bool isAct = false;

    Color orgInitialColor;
    Color dirInitialColor;

    [SerializeField]
	GameObject bg = null;


	[MenuItem("MyProject/CreateBG")]
	public static void CreateBg(){
		Debug.Log (File.Exists(Application.dataPath + "/_Texture/fightBg.prefab"));
		Object srcObj = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath ("Assets/Sources/Prefab/fightBg.prefab", typeof(UnityEngine.Object));

		//GameObject go = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
		//go.transform.parent = Selection.activeGameObject.transform;
		float myDis = 0.97f;
		int myRadio = 4;
		int myMaxHC = 7;

		int num = 0;
		for (int i = 0; i < myMaxHC; i++) {
			if (i < myRadio) {
				for (int j = 0; j < myRadio + i; j++) {
					GameObject backGround = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
					backGround.name = num.ToString ();
					num++;
					backGround.transform.parent = Selection.activeGameObject.transform;
					backGround.transform.localPosition = new Vector3 ((i * myDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(myDis*j)
						, - i*(myDis * Mathf.Sin (60 * Mathf.Deg2Rad))
						, 0);
					if (i == 0 || i== myMaxHC - 1) {
                        backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
                    }
                    else
					{
						if (j == 0 || j == myRadio + i - 1)
						{
                            backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
						}
						else {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.None;
                        }
                    }
				}
			} 
			else {
				for (int j = 0; j < myRadio + ((myMaxHC - 1) - i); j++) {
					GameObject backGround = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
					backGround.name = num.ToString ();
					num++;
					backGround.transform.parent = Selection.activeGameObject.transform;
					backGround.transform.localPosition = new Vector3 ((((myMaxHC - 1) - i) * myDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(myDis*j)
						, - i*(myDis * Mathf.Sin (60 * Mathf.Deg2Rad))
						, 0);
					if (i == 0 || i == myMaxHC - 1) {
                        backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
                    }
                    else {
						if (j == 0 || j == myRadio + ((myMaxHC - 1) - i) - 1) {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
                        }
                        else
						{
                            backGround.GetComponent<GroundController>()._groundType = GroundType.None;
                        }
                    }   
				}
			}
		}
	}

    [MenuItem("MyProject/CreateBGUI")]
    public static void CreateBgForUI()
    {
        Debug.Log(File.Exists(Application.dataPath + "/bg2.prefab"));
        Object srcObj = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/Sources/Prefab/fightBgUI.prefab", typeof(UnityEngine.Object));

        //GameObject go = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
        //go.transform.parent = Selection.activeGameObject.transform;
        float myDis = 97f;
        int myRadio = 4;
        int myMaxHC = 7;

        int num = 0;
        for (int i = 0; i < myMaxHC; i++)
        {
            if (i < myRadio)
            {
                for (int j = 0; j < myRadio + i; j++)
                {
                    GameObject backGround = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
                    backGround.name = num.ToString();
                    num++;
                    backGround.transform.parent = Selection.activeGameObject.transform;
                    backGround.transform.localPosition = new Vector3((i * myDis) * Mathf.Sin(210 * Mathf.Deg2Rad) + (myDis * j)
                        , -i * (myDis * Mathf.Sin(60 * Mathf.Deg2Rad))
                        , 0);
                    if (i == 0 || i == myMaxHC - 1)
                    {
                        backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
                    }
                    else
                    {
                        if (j == 0 || j == myRadio + i - 1)
                        {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
                        }
                        else
                        {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.None;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < myRadio + ((myMaxHC - 1) - i); j++)
                {
                    GameObject backGround = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
                    backGround.name = num.ToString();
                    num++;
                    backGround.transform.parent = Selection.activeGameObject.transform;
                    backGround.transform.localPosition = new Vector3((((myMaxHC - 1) - i) * myDis) * Mathf.Sin(210 * Mathf.Deg2Rad) + (myDis * j)
                        , -i * (myDis * Mathf.Sin(60 * Mathf.Deg2Rad))
                        , 0);
                    if (i == 0 || i == myMaxHC - 1)
                    {
                        backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
                    }
                    else
                    {
                        if (j == 0 || j == myRadio + ((myMaxHC - 1) - i) - 1)
                        {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
                        }
                        else
                        {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.None;
                        }
                    }
                }
            }
        }
    }

    [MenuItem("MyProject/SetType")]
	public static void SetType(){
		GameObject selGO = Selection.activeGameObject;
		GroundController[] uiGC = selGO.transform.GetChild (0).GetComponentsInChildren<GroundController> ();
		GroundController[] gc = selGO.transform.GetChild (1).GetComponentsInChildren<GroundController> ();

		if (uiGC.Length == gc.Length) {
			for (int i = 0; i < gc.Length; i++) {
				if ((int)gc [i]._groundType != 0 && (int)gc [i]._groundType != 99) {
					gc [i]._groundType =  GroundType.None;
				}
				if ((int)uiGC [i]._groundType != 0 && (int)uiGC [i]._groundType != 99) {
					uiGC [i]._groundType = GroundType.None;
				}
			}
		}
	}

	[MenuItem("MyProject/SetSprite")]
	public static void SetSprite(){
		GameObject selGO = Selection.activeGameObject;
		GroundController[] uiGC = selGO.transform.GetChild (1).GetComponentsInChildren<GroundController> ();

		foreach (GroundController gc in uiGC) {
            Debug.Log(gc.matchController.transform.parent);
            //gc.matchController.GetComponent<UIPolygon>().sprite = ;
		}
	}

	

	[MenuItem("MyProject/Match")]
	public static void MatchGC(){
		GameObject selGO = Selection.activeGameObject;
		GroundController[] uiGC = selGO.transform.GetChild (0).GetComponentsInChildren<GroundController> ();
		GroundController[] gc = selGO.transform.GetChild (1).GetComponentsInChildren<GroundController> ();

		if (uiGC.Length == gc.Length) {
			for (int i = 0; i < gc.Length; i++) {
				gc [i].matchController = uiGC [i];
				uiGC [i].matchController = gc [i];
			}
		}
	}
}
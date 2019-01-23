using System.Collections;
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


	[MenuItem("MyProject/CreateBgLandscape")]
	public static void CreateBgLandscape(){
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
	public static void CreateBgForUILandscape()
    {
        Debug.Log(File.Exists(Application.dataPath + "/bg2.prefab"));
        Object srcObj = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/Sources/Prefab/fightBgUI.prefab", typeof(UnityEngine.Object));

        //GameObject go = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
        //go.transform.parent = Selection.activeGameObject.transform;
        float myDis = 116f;
        int myRadio = 5;
        int myMaxHC = 9;

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
						backGround.GetComponent<GroundController> ().ChangeSprite (GroundType.Caution);
                    }
                    else
                    {
                        if (j == 0 || j == myRadio + i - 1)
                        {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
							backGround.GetComponent<GroundController> ().ChangeSprite (GroundType.Caution);
                        }
                        else
                        {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.None;
							backGround.GetComponent<GroundController> ().ChangeSprite (GroundType.None);
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
						backGround.GetComponent<GroundController> ().ChangeSprite (GroundType.Caution);
                    }
                    else
                    {
                        if (j == 0 || j == myRadio + ((myMaxHC - 1) - i) - 1)
                        {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
							backGround.GetComponent<GroundController> ().ChangeSprite (GroundType.Caution);
                        }
                        else
                        {
                            backGround.GetComponent<GroundController>()._groundType = GroundType.None;
							backGround.GetComponent<GroundController> ().ChangeSprite (GroundType.None);
                        }
                    }
                }
            }
        }


		Vector3 centerPos = new Vector3();
		int bgCount = Selection.activeGameObject.transform.childCount;
		centerPos = Selection.activeGameObject.transform.GetChild ((bgCount - 1) / 2).transform.localPosition;

		for (int i = 0; i < bgCount; i++) {
			Selection.activeGameObject.transform.GetChild (i).localPosition = Selection.activeGameObject.transform.GetChild (i).localPosition - centerPos;
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
            Debug.Log(gc.transform.parent);
            //gc.matchController.GetComponent<UIPolygon>().sprite = ;
		}
	}

	

	[MenuItem("MyProject/Match")]
	public static void MatchGC(){
		GameObject selGO = Selection.activeGameObject;
		GroundController[] uiGC = selGO.transform.GetChild (0).GetComponentsInChildren<GroundController> ();
		GroundController[] gc = selGO.transform.GetChild (1).GetComponentsInChildren<GroundController> ();


		/*if (uiGC.Length == gc.Length) {
			for (int i = 0; i < gc.Length; i++) {
				gc [i].matchController = uiGC [i];
				uiGC [i].matchController = gc [i];
			}
		}*/
	}

	[MenuItem("MyProject/CreateBgPortrait")]
	public static void CreateBgPortrait(){
		Object srcObj = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath ("Assets/Sources/Prefab/fightBgPortrait.prefab", typeof(UnityEngine.Object));

		//GameObject go = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
		//go.transform.parent = Selection.activeGameObject.transform;
		float myDis = 0.97f;
		int myRadio = 5;
		int myMaxHC = 9;

		int num = 0;
		for (int i = 0; i < myMaxHC; i++) {
			if (i < myRadio) {
				for (int j = 0; j < myRadio + i; j++) {
					GameObject backGround = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
					backGround.name = num.ToString ();
					num++;
					backGround.transform.parent = Selection.activeGameObject.transform;
					backGround.transform.localPosition = new Vector3 (
						- i*(myDis * Mathf.Sin (60 * Mathf.Deg2Rad)),
						(i * myDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(myDis*j)
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
					backGround.transform.localPosition = new Vector3 (
						- i*(myDis * Mathf.Sin (60 * Mathf.Deg2Rad)),
						(((myMaxHC - 1) - i) * myDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(myDis*j)
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

	[MenuItem("MyProject/CreateBGUIPortrait")]
	public static void CreateBgForUIPortrait()
	{
		Object srcObj = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/XSources/Prefab/fightBgUIProtrait.prefab", typeof(UnityEngine.Object));

		//GameObject go = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
		//go.transform.parent = Selection.activeGameObject.transform;
		float myDis = 116f;
		int myRadio = 5;
		int myMaxHC = 9;

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
					backGround.transform.localPosition = new Vector3(
						-i * (myDis * Mathf.Sin(60 * Mathf.Deg2Rad)),
						(i * myDis) * Mathf.Sin(210 * Mathf.Deg2Rad) + (myDis * j)
						, 0);
					if (i == 0 || i == myMaxHC - 1)
					{
						backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
						backGround.GetComponent<GroundController>().background.sprite = backGround.GetComponent<GroundController> ().GetSprites [4];
					}
					else
					{
						if (j == 0 || j == myRadio + i - 1)
						{
							backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
							backGround.GetComponent<GroundController>().background.sprite = backGround.GetComponent<GroundController> ().GetSprites [4];
						}
						else
						{
							backGround.GetComponent<GroundController>()._groundType = GroundType.None;
							backGround.GetComponent<GroundController>().background.sprite = backGround.GetComponent<GroundController> ().GetSprites [0];
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
					backGround.transform.localPosition = new Vector3(
						-i * (myDis * Mathf.Sin(60 * Mathf.Deg2Rad)),
						(((myMaxHC - 1) - i) * myDis) * Mathf.Sin(210 * Mathf.Deg2Rad) + (myDis * j)
						, 0);
					if (i == 0 || i == myMaxHC - 1)
					{
						backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
						backGround.GetComponent<GroundController>().background.sprite = backGround.GetComponent<GroundController> ().GetSprites [4];
					}
					else
					{
						if (j == 0 || j == myRadio + ((myMaxHC - 1) - i) - 1)
						{
							backGround.GetComponent<GroundController>()._groundType = GroundType.Caution;
							backGround.GetComponent<GroundController>().background.sprite = backGround.GetComponent<GroundController> ().GetSprites [4];
                        }
						else
						{
							backGround.GetComponent<GroundController>()._groundType = GroundType.None;
							backGround.GetComponent<GroundController>().background.sprite = backGround.GetComponent<GroundController> ().GetSprites [0];
                        }
					}
				}
			}
		}


		Vector3 centerPos = new Vector3();
		int bgCount = Selection.activeGameObject.transform.childCount;
		centerPos = Selection.activeGameObject.transform.GetChild ((bgCount - 1) / 2).transform.localPosition;

		for (int i = 0; i < bgCount; i++) {
			Selection.activeGameObject.transform.GetChild (i).localPosition = Selection.activeGameObject.transform.GetChild (i).localPosition - centerPos;
            Selection.activeGameObject.transform.GetChild(i).tag = "fightG";
        }
    }

    [MenuItem("MyProject/CreateSEUIPortrait")]
    public static void CreateSEForUIPortrait()
    {
        Object srcObj = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/XSources/Prefab/fightSEUIProtrait.prefab", typeof(UnityEngine.Object));

        //GameObject go = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
        //go.transform.parent = Selection.activeGameObject.transform;
        float myDis = 116f;
        int myRadio = 5;
        int myMaxHC = 9;

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
                    backGround.transform.localPosition = new Vector3(
                        -i * (myDis * Mathf.Sin(60 * Mathf.Deg2Rad)),
                        (i * myDis) * Mathf.Sin(210 * Mathf.Deg2Rad) + (myDis * j)
                        , 0);
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
                    backGround.transform.localPosition = new Vector3(
                        -i * (myDis * Mathf.Sin(60 * Mathf.Deg2Rad)),
                        (((myMaxHC - 1) - i) * myDis) * Mathf.Sin(210 * Mathf.Deg2Rad) + (myDis * j)
                        , 0);
                }
            }
        }


        Vector3 centerPos = new Vector3();
        int bgCount = Selection.activeGameObject.transform.childCount;
        centerPos = Selection.activeGameObject.transform.GetChild((bgCount - 1) / 2).transform.localPosition;

        for (int i = 0; i < bgCount; i++)
        {
            Selection.activeGameObject.transform.GetChild(i).localPosition = Selection.activeGameObject.transform.GetChild(i).localPosition - centerPos;
            Selection.activeGameObject.transform.GetChild(i).GetComponent<GroundController>().raycastController = Selection.activeGameObject.GetComponent<GroundRaycastController>();
        }
    }


    [MenuItem("MyProject/Match2")]
    public static void MatchGCNew()
    {
        GameObject selGO = Selection.activeGameObject;
        GroundController[] uiGC = selGO.transform.GetChild(0).GetComponentsInChildren<GroundController>();
        GroundController[] gc = selGO.transform.GetChild(2).GetComponentsInChildren<GroundController>();
        Transform[] se = selGO.transform.GetChild(1).GetComponentsInChildren<Transform>();



        if (uiGC.Length == gc.Length)
        {
            for (int i = 0; i < gc.Length; i++)
            {
                
                uiGC[i].light = selGO.transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<TweenColor>();
                uiGC[i].colorLight = selGO.transform.GetChild(1).GetChild(i).GetChild(0).GetChild(0).GetComponent<TweenColor>();
                //gc[i].matchController = uiGC[i];
                //uiGC[i].matchController = gc[i];
            }
        }
    }

    [MenuItem("MyProject/SetRow")]
    public static void SetRow()
    {
        float myDis = 58f;
        GameObject selGO = Selection.activeGameObject;
        GroundController[] gc = selGO.transform.GetChild(0).GetComponentsInChildren<GroundController>();

        for (int i = 0; i < gc.Length; i++)
        {
            for (int j = 0; j < 17; j++)
            {
                if (gc[i].transform.localPosition.y > (myDis * (8 - j) - (myDis * 0.05f)) &&
                gc[i].transform.localPosition.y < (myDis * (8 - j) + (myDis * 0.05f)))
                {
                    gc[i].groundRow = j;
                }
            }
        }
    }

    [MenuItem("MyProject/SetRaycastController")]
    public static void SetRaycastController()
    {
        GameObject selGO = Selection.activeGameObject;
        GroundController[] gc = selGO.transform.GetChild(0).GetComponentsInChildren<GroundController>();

        for (int i = 0; i < gc.Length; i++)
        {
            gc[i].raycastController = selGO.transform.GetChild(0).GetComponent<GroundRaycastController>();
        }
    }

    [MenuItem("MyProject/test16bit")]
    public static void Test16Bit()
    {
        bool a = true;
        Debug.Log(System.Convert.ToInt32(!a));
    }
}

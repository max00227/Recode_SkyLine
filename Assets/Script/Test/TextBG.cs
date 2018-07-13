using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class TextBG : MonoBehaviour {
    int groundRadio = 5;
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


	[MenuItem("MyProject/CreateBGForUI")]
	public static void CreateBgForUI(){
		Debug.Log (File.Exists(Application.dataPath + "/_Texture/fightBg.prefab"));
		Object srcObj = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath ("Assets/_Texture/fightBg.prefab", typeof(UnityEngine.Object));

		//GameObject go = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
		//go.transform.parent = Selection.activeGameObject.transform;
		float myDis = 97f;
		int myRadio = 5;
		int myMaxHC = 9;

		int num = 0;
		for (int i = 0; i < myMaxHC; i++) {
			if (i < myRadio) {
				for (int j = 0; j < myRadio + i; j++) {
					GameObject backGround = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
					backGround.name = num.ToString ();
					backGround.AddComponent<GroundController> ();
					num++;
					backGround.transform.parent = Selection.activeGameObject.transform;
					backGround.transform.localPosition = new Vector3 ((i * myDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(myDis*j)
						, - i*(myDis * Mathf.Sin (60 * Mathf.Deg2Rad))
						, 0);
					/*if (i == 0 || i==maxHoriCount-1) {
						restrictArea.Add(backGround.GetComponent<GroundController>());
					}
					else
					{
						if (j == 0 || j == groundRadio + i - 1)
						{
							restrictArea.Add(backGround.GetComponent<GroundController>());
						}
						else {
							normalArea.Add(backGround.GetComponent<GroundController>());
						}
					}*/
					//backGround.GetComponent<MeshRenderer> ().material.color = (Color)colorWhite;
				}
			} 
			else {
				for (int j = 0; j < myRadio + ((myMaxHC - 1) - i); j++) {
					GameObject backGround = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
					backGround.name = num.ToString ();
					backGround.AddComponent<GroundController> ();
					num++;
					backGround.transform.parent = Selection.activeGameObject.transform;
					backGround.transform.localPosition = new Vector3 ((((myMaxHC - 1) - i) * myDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(myDis*j)
						, - i*(myDis * Mathf.Sin (60 * Mathf.Deg2Rad))
						, 0);
					/*if (i == 0 || i == maxHoriCount - 1) {
						restrictArea.Add (backGround.GetComponent<GroundController>());
					} 
					else {
						if (j == 0 || j == groundRadio + ((maxHoriCount - 1) - i) - 1) {
							restrictArea.Add(backGround.GetComponent<GroundController>());
						}
						else
						{
							normalArea.Add(backGround.GetComponent<GroundController>());
						}
					}*/
					//backGround.GetComponent<MeshRenderer> ().material.color = (Color)colorWhite;
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

	[MenuItem("MyProject/CloseCollider")]
	public static void CloseCollider(){
		GameObject selGO = Selection.activeGameObject;
		GroundController[] gc = selGO.transform.GetChild (1).GetComponentsInChildren<GroundController> ();

		for (int i = 0; i < gc.Length; i++) {
			if ((int)gc [i]._groundType == 99) {
				gc[i].gameObject.AddComponent<PolygonCollider2D>();
			}
		}
	}

	[MenuItem("MyProject/SetSprite")]
	public static void SetSprite(){
		GameObject selGO = Selection.activeGameObject;
		GroundController[] uiGC = selGO.transform.GetChild (0).GetComponentsInChildren<GroundController> ();

		foreach (GroundController gc in uiGC) {
			gc.image = gc.GetComponent<UIPolygon> ();
			//gc.ChangeSprite ();
			gc.image.color = Color.white;
		}
	}

	[MenuItem("MyProject/CreateBG")]
	public static void CreateBg(){
		Debug.Log (File.Exists(Application.dataPath + "/bg2.prefab"));
		Object srcObj = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath ("Assets/bg2.prefab", typeof(UnityEngine.Object));

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
					backGround.AddComponent<GroundController> ();
					num++;
					backGround.transform.parent = Selection.activeGameObject.transform;
					backGround.transform.localPosition = new Vector3 ((i * myDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(myDis*j)
						, - i*(myDis * Mathf.Sin (60 * Mathf.Deg2Rad))
						, 0);
					if (i == 0 || i==myMaxHC-1) {
						backGround.GetComponent<GroundController> ()._groundType = GroundType.Caution;
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
					//backGround.GetComponent<MeshRenderer> ().material.color = (Color)colorWhite;
				}
			} 
			else {
				for (int j = 0; j < myRadio + ((myMaxHC - 1) - i); j++) {
					GameObject backGround = PrefabUtility.InstantiatePrefab(srcObj) as GameObject;
					backGround.name = num.ToString ();
					backGround.AddComponent<GroundController> ();
					num++;
					backGround.transform.parent = Selection.activeGameObject.transform;
					backGround.transform.localPosition = new Vector3 ((((myMaxHC - 1) - i) * myDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(myDis*j)
						, - i*(myDis * Mathf.Sin (60 * Mathf.Deg2Rad))
						, 0);
					if (i == 0 || i == myMaxHC - 1) {
						backGround.GetComponent<GroundController> ()._groundType = GroundType.Caution;
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
	
	void Start () {
		
		maxHoriCount = groundRadio * 2 - 1;

		objDis = CalutionDis(bg.transform.GetComponent<Renderer> ().bounds.size.y);

        GameObject wnd = GameObject.Instantiate(openWnd);
        wnd.name = openWnd.name;

        SetBackGround();

    }
		

    void Update()
    {
        RaycastHit hit;
        Ray ray;
        
        if (Input.GetMouseButtonDown(0))
        {

           
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    hit.collider.gameObject.GetComponent<MeshRenderer>().material.color = _selectionChar;
                    rayOrg = hit.transform.localPosition;
                    hitOrg  = hitDir = hit.collider;
                }
            }

           
        }

        if (Input.GetMouseButton(0)) {
            RaycastHit mHit;

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    
                    if (hit.collider != hitOrg)
                    {
                        isAct = true;
                        hit.collider.gameObject.GetComponent<MeshRenderer>().material.color = _selectionChar;
                        if (hitDir != hitOrg && hitDir != hit.collider)
                        {
                            hitDir.gameObject.GetComponent<MeshRenderer>().material.color = InitialColor(hitDir.gameObject);
                        }
                        hitDir = hit.collider;

                       
                        if (Physics.Raycast(rayOrg, hit.transform.localPosition, out mHit)) {
                           
                           // if((Vector2)hit.transform.localPosition-(Vector2)rOrg)
                        }
                    }
                    if (hit.collider == hitOrg)
                    {
                        
                        if (isAct == true)
                        {
                            hitDir.gameObject.GetComponent<MeshRenderer>().material.color = InitialColor(hitDir.gameObject);
                            isAct = false;
                        }
                        hitDir = hit.collider;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {


            ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (hit.collider == hitOrg)
                    {
                        Debug.Log("Cancel");
                        hitDir.gameObject.GetComponent<MeshRenderer>().material.color = InitialColor(hitDir.gameObject);
                        hitOrg.gameObject.GetComponent<MeshRenderer>().material.color = InitialColor(hitOrg.gameObject);
                    }
                    else {
                        if (IsCalculation((Vector2)rayOrg, (Vector2)hit.transform.localPosition))
                        {
                            Debug.Log("IsCal");
                            hitDir.gameObject.GetComponent<MeshRenderer>().material.color = InitialColor(hitDir.gameObject);
                            hitOrg.gameObject.GetComponent<MeshRenderer>().material.color = InitialColor(hitOrg.gameObject);
                        }
                        else
                        {
                            Debug.Log("Not IsCal");
                            hitDir.gameObject.GetComponent<MeshRenderer>().material.color = InitialColor(hitDir.gameObject);
                            hitOrg.gameObject.GetComponent<MeshRenderer>().material.color = InitialColor(hitOrg.gameObject);
                        }
                    }
                    isAct = false;
                }
            }
        }

        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            }
        }
    }

    public Color InitialColor(GameObject go)
    {
        Vector4 _color = new Color();
        foreach (GroundController gc in restrictArea)
        {
            if (go == gc.gameObject)
            {
                _color = colorRed;
                break;
            }
            else {
                _color = colorWhite;
            }
        }

        return (Color)_color;
    }

    public bool IsCalculation(Vector2 org, Vector2 dir) {
        Vector2 dirNormalized = (dir - org).normalized;
        if (Mathf.Round(Mathf.Abs(dirNormalized.x * 10)) == 5 && Mathf.Round(Mathf.Abs(dirNormalized.y * 10)) == 9)
        {
            return true;
        }
        else if (Mathf.Round(Mathf.Abs(dirNormalized.x * 10)) == 10 && Mathf.Round(Mathf.Abs(dirNormalized.y * 10)) == 0)
        {
            return true;
        }
        else {
            return false;
        }
    }

	public void SetBackGround(){
		if (bgPool == null) {
			bgPool = new GameObject ();
			bgPool.name = "BackGroundPool";
			bgPool.transform.position = Vector3.back;
           
		}

		int num = 0;
		for (int i = 0; i < maxHoriCount; i++) {
			if (i < groundRadio) {
				for (int j = 0; j < groundRadio + i; j++) {
					GameObject backGround = GameObject.Instantiate(bg);
					backGround.name = num.ToString ();
					backGround.AddComponent<GroundController> ();
					num++;
					backGround.transform.parent = bgPool.transform;
					backGround.transform.localPosition = new Vector3 ((i * objDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(objDis*j)
					, - i*(objDis * Mathf.Sin (60 * Mathf.Deg2Rad))
					, 0);
					if (i == 0 || i==maxHoriCount-1) {
                        restrictArea.Add(backGround.GetComponent<GroundController>());
                    }
                    else
                    {
                        if (j == 0 || j == groundRadio + i - 1)
                        {
                            restrictArea.Add(backGround.GetComponent<GroundController>());
                        }
                        else {
                            normalArea.Add(backGround.GetComponent<GroundController>());
                        }
                    }
					backGround.GetComponent<MeshRenderer> ().material.color = (Color)colorWhite;
				}
			} 
			else {
				for (int j = 0; j < groundRadio + ((maxHoriCount - 1) - i); j++) {
					GameObject backGround = GameObject.Instantiate(bg);
					backGround.name = num.ToString ();
					backGround.AddComponent<GroundController> ();
					num++;
					backGround.transform.parent = bgPool.transform;
					backGround.transform.localPosition = new Vector3 ((((maxHoriCount - 1) - i) * objDis) * Mathf.Sin (210 * Mathf.Deg2Rad)+(objDis*j)
						, - i*(objDis * Mathf.Sin (60 * Mathf.Deg2Rad))
					, 0);
					if (i == 0 || i == maxHoriCount - 1) {
						restrictArea.Add (backGround.GetComponent<GroundController>());
					} 
					else {
						if (j == 0 || j == groundRadio + ((maxHoriCount - 1) - i) - 1) {
                            restrictArea.Add(backGround.GetComponent<GroundController>());
                        }
                        else
                        {
                            normalArea.Add(backGround.GetComponent<GroundController>());
                        }
                    }
					backGround.GetComponent<MeshRenderer> ().material.color = (Color)colorWhite;
				}
			}
		}

		centerPos = bgPool.transform.GetChild ((int)(groundRadio * (groundRadio - 1) / 4 * 6)).localPosition;

        Camera.main.transform.position = new Vector3(centerPos.x, centerPos.y, -10);
        Camera.main.fieldOfView = 45f;

		//SetPlayer ();

		foreach (GroundController go in restrictArea) {
			go.GetComponent<MeshRenderer> ().material.color = (Color)colorRed;
		}
        Debug.Log(restrictArea.Count + " , " + normalArea.Count);
		isSet = true;
       
    }

	float CalutionDis(float height){
		return height*Mathf.Sin (60 * Mathf.Deg2Rad) * (1 + (float)spacing / 30);
	}


	public void SetPlayer(){
		_player = Instantiate (bg);
		hitPoint = new Vector3 (centerPos.x, centerPos.y, -2);
		_player.transform.position = hitPoint;
		_player.transform.localScale = Vector3.one * 0.8f;
		_player.GetComponent<MeshRenderer> ().material.color = Color.blue;
		_player.GetComponent<SphereCollider> ().enabled = false;
		_player.name = "Player";
	}

    public void SelectionChar(int idx) {
        _selectionChar = playColor[idx];
    }
}

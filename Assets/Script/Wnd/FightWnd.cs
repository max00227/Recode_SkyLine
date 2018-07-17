using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using model.data;
using System;
using System.IO;
using System.Linq;
using UnityEngine.Profiling;

public class FightWnd : MonoBehaviour {
	[SerializeField]
	GameObject groundPool;

	[SerializeField]
	DirectGroundController center;

	[SerializeField]
	DirectGroundController[] angleGc = new DirectGroundController[6];

	[SerializeField]
	Transform CharaGroup;

	[SerializeField]
	Transform rayGroup;

	[SerializeField]
	Transform imagePool;

	GroundController[] allGcs;

	GroundController startGc;

	GroundController endGc;

	bool isResetGround=false;

    //Dictionary<int, List<GroundSpace>> groundSpaces;

    private CharaLargeData[] characters;

	List<GroundController> norGcs;

	int CreateGround;

	int ResetCount;

	Stack<Image> _charaGroup = new Stack<Image> ();

	Stack<Image> _imagePool = new Stack<Image> ();

	Stack<Image> _rayGroup = new Stack<Image> ();


	[SerializeField]
	Image spriteImage;

	int? charaIdx;

	Image startCharaImage;
	Image endCharaImage;

    LinkedList<GroundController> charaGc;

	int resetGroundCount;

    public Sprite[] CharaSprite;

	public Sprite lineSprite;

    int[] charaDamage;

    [SerializeField]
    public Text[] damageTxt;

    private bool spaceCorrect;

    Dictionary<int, List<RaycastData>> recCharaDamages;

    Dictionary<int, List<RaycastData>> charaDamages;

    List<GroundController[]> raycasted;

    List<PlusDamageData> plusDamages;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < 32; i++) {
			Image _image = Instantiate (spriteImage) as Image;
			_image.GetComponent<RectTransform> ().SetParent (imagePool);
			_image.transform.localPosition = Vector3.zero;
			_image.gameObject.SetActive (false);
			_imagePool.Push (_image);
		}
			

		allGcs = groundPool.GetComponentsInChildren<GroundController> ();
		norGcs = new List<GroundController> ();

		resetGroundCount = 0;

		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType == 0) {
                gc.matchController.plusDamage += OnPlusDamage;
				norGcs.Add (gc.matchController);
			}
		}

        if (norGcs.Count == 37)
        {
            CreateGround = 3;
        }
        else
        {
            CreateGround = 2;
        }

        ResetGround(true);
    }


    private void OnPlusDamage(PlusDamageData plusDamage) {
		Debug.Log (plusDamage.gc.name + " : " + plusDamage.charaIdx);
        plusDamages.Add(plusDamage);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
			RoundStart(new DirectGroundController());
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            ResetGround();
        }

        if (Input.GetKeyDown(KeyCode.H)) {
			CheckGround ();
			ChangeLayer ();
            NextRound(false);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            foreach (KeyValuePair<int, List<RaycastData>> kv in recCharaDamages) {
                foreach (var v in kv.Value) {
                    Debug.Log(v.start.name + " :" + v.end.name);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            foreach (var v in charaGc)
            {
                Debug.Log(v.name + " : " + v.isActived + " , " + v._groundType+" , "+(int)v.charaIdx);
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            foreach (KeyValuePair<int, List<RaycastData>> kv in charaDamages)
            {
                foreach (var v in kv.Value)
                {
                    Debug.Log(v.start.name + " :" + v.end.name);
                }
            }
        }

        if (Input.GetKeyDown (KeyCode.Mouse0)) {
			TouchDown ();
		}

		if (Input.GetKeyDown (KeyCode.Y)) {
			testCheckGround ();
		}

        if (Input.GetKey(KeyCode.Mouse0))
        {
            TouchDrap ();
        }

		if (Input.GetKeyUp (KeyCode.Mouse0)) {
			TouchUp ();
		}
	}

    void SetChara() {
        //foreach()
    }


	void RoundStart(DirectGroundController dirCenter){
		if (dirCenter.gc == null) {
			dirCenter = center;
		}

		RaycastHit2D[] hits;

        List<int> randomList = new List<int>();

        if (norGcs.Count >= 37)
        {
            randomList = RandomList((CreateGround - 1) + (int)Mathf.Ceil(resetGroundCount / 2), dirCenter.randomList);
        }
        else
        {
            randomList = RandomList(CreateGround + (int)Mathf.Ceil(resetGroundCount / 2), dirCenter.randomList);
        }


        dirCenter.gc.matchController.ChangeType ();

        if (randomList.Count>0){
			foreach (int randomI in randomList) {
				hits = GetRaycastHits(dirCenter.gc.matchController.transform.localPosition, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + dirCenter.randomList[randomI] * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + dirCenter.randomList[randomI] * 60))), 0.97f);
				if (hits.Length > 0) {
					foreach (var hit in hits) {
						hit.collider.GetComponent<GroundController> ().ChangeType ();
                    }
                }
			}
		}
	}

    /// <summary>
    /// 進行下一回合擺放前抽選產生Ground
    /// </summary>
    /// <param name="isSpace">是否擺放角色</param>
	private void NextRound(bool isSpace = true){
		List<GroundController> nextRoundGc = new List<GroundController> ();
		List<GroundController> layerList = new List<GroundController> ();
		GroundController maxLayerGc = null;

        int noneCount = 1;
		for (int i = 7; i > 0; i--) {
			foreach (GroundController gc in norGcs) {
				if (maxLayerGc == null) {
                    if (gc._layer == i) {
						layerList.Add (gc);
					}
				} else {
                    if (gc._layer >= i && gc != maxLayerGc) {
						if (i == 1) {
							noneCount++;
						}
						layerList.Add (gc);
					}
				}
			}
				
			if (maxLayerGc == null) {
                if (layerList.Count == 1) {
					maxLayerGc = layerList [0];
				} else if (layerList.Count > 1) {
					maxLayerGc = RandomList (1, layerList.ToArray ()) [0];
				}
				layerList = new List<GroundController> ();
			}
		}

		if (layerList.Count > 0) {
			foreach (var gc in RandomList ((CreateGround*(Convert.ToInt32(!isSpace)+1))-1+(int)Mathf.Ceil(resetGroundCount/2), layerList.ToArray(),noneCount)) {
				nextRoundGc.Add (gc);
			}
		}
		if (maxLayerGc != null) {
			nextRoundGc.Add (maxLayerGc);
		}

		if (nextRoundGc != null && nextRoundGc.Count > 0) {
			foreach (GroundController gc in nextRoundGc) {
				gc.ChangeType ();
			}
		} else {
			ResetGround ();
		}
	}

	private void TouchDown(){
		if (charaIdx != null) {
			var result = CanvasManager.Instance.GetRaycastResult ();
			if (result.Count > 0) {
				foreach (var r in result) {
					if (r.gameObject.tag == "fightG") {
						if ((int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 0
						   || (int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 99) {
							startGc = r.gameObject.GetComponent<GroundController> ().matchController;
                            charaGc.AddLast(startGc);
							startGc.ChangeChara(charaIdx);

							startCharaImage = PopImage (CharaGroup, false, r.gameObject.transform.localPosition);
							endCharaImage = PopImage (CharaGroup, false, r.gameObject.transform.localPosition);

							if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
								isResetGround = true;
							}
						} else {
							startGc.PrevType ();
							Debug.Log ("Start Error");
						}
					}
				}
			}
		}
	}

	private void TouchDrap(){
		if (endCharaImage != null) {
			var result = CanvasManager.Instance.GetRaycastResult ();
			if (result.Count > 0) {
				foreach (var r in result) {
                    if (r.gameObject.tag == "fightG")
                    {
						endCharaImage.transform.localPosition = r.gameObject.transform.localPosition;


						if (endGc != r.gameObject.GetComponent<GroundController> ().matchController) {
                            foreach (GroundController gc in charaGc) {
								gc.OnPrevType ();
							}

							if (endGc != null) {
								Debug.Log (charaGc.Count);
								if (charaGc.Last.Value == endGc) {
									endGc.ResetType ();
									charaGc.RemoveLast ();
								}
							}

                            if ((int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 0 || (int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 99)
                            {
                                endGc = r.gameObject.GetComponent<GroundController>().matchController;

                                Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, endGc.transform.localPosition);

                                if (IsCorrectEnd(dir))
                                {
                                    endGc.ChangeChara(charaIdx);
                                    charaGc.AddLast(endGc);

                                    CheckGround();
                                    spaceCorrect = true;
                                }
                                else
                                {
									endGc = null;
                                    ResetDamage();
                                    spaceCorrect = false;
                                }
                            }
                            else
                            {
                                endGc = null;
                                spaceCorrect = false;
                            }
						} 
						else {
							if (endGc == startGc) {
								endGc = null;
								spaceCorrect = false;
							}
						}
                    }
				}
			}
		}
	}

   

	private void TouchUp(){
		if (endCharaImage != null) {
			var result = CanvasManager.Instance.GetRaycastResult ();
			if (result.Count > 0) {
				foreach (var r in result) {
                    if (spaceCorrect == true && r.gameObject.tag == "fightG")
                    {
                        if ((int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 99) {
                            isResetGround = true;
                        }
						RoundEnd();
						if (isResetGround) {
							ResetGround ();
							return;
						}
						ChangeLayer ();
						NextRound ();
                    }
                    else {
						PopImage (CharaGroup);
						PopImage (CharaGroup);
						startCharaImage = endCharaImage = null;
						startGc.ResetType ();
                        charaGc.RemoveLast();
                        ResetDamage();

                        Debug.Log("End Error");
                    }
				}
			}
		}
    }

	private void testCheckGround() {
		foreach (GroundController gc in charaGc) {
			gc.testRaycasted = false;
		}
		foreach (GroundController gc in charaGc) {
			gc.OnTestChangeType();
		}
	}

    private void CheckGround() {
        charaDamages = new Dictionary<int, List<RaycastData>> ();
        plusDamages = new List<PlusDamageData>();

		foreach (GroundController gc in charaGc) {
			gc.raycasted = false;
		}
		foreach (GroundController gc in charaGc) {
			ResponseData(gc.OnChangeType());
		}

        ChangeCharaDamage ();
    }
		
	private void ResponseData(Dictionary<int, List<RaycastData>> raycastData){
        if (charaDamages.Count == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                charaDamages.Add(i, new List<RaycastData>());
            }
        }

        foreach (KeyValuePair<int,List<RaycastData>> kv in raycastData) {
            foreach (var data in raycastData[kv.Key])
            {
                charaDamages[kv.Key].Add(data);
            }
		}
	}

	private void ChangeCharaDamage()
	{
		foreach (KeyValuePair<int, List<RaycastData>> kv in charaDamages)
		{
			int sumDamage = 0;
			foreach (var data in charaDamages[kv.Key])
			{
				sumDamage = sumDamage + data.damage;
			}

			if (sumDamage != charaDamage [kv.Key]) {
				damageTxt [kv.Key].text = sumDamage.ToString();
				damageTxt [kv.Key].color = Color.red;
			} 
			else {
				damageTxt [kv.Key].color = Color.black;
			}
		}
	}

	private void ChangeCharaDamage(bool add_Plus)
    {
		List<int> damages = new List<int> ();
		for (int i = 0; i < 5; i++) {
			damages.Add (0);
		}
        foreach (KeyValuePair<int, List<RaycastData>> kv in charaDamages)
        {
            int sumDamage = 0;
            foreach (var data in charaDamages[kv.Key])
            {
                sumDamage = sumDamage + data.damage;
            }
				
			damages [kv.Key] = sumDamage;
        }

		foreach (var plus in plusDamages) {
			damages [plus.charaIdx] += 25;
		}

		for (int i = 0; i < 5; i++) {
			if (damages[i] != charaDamage [i]) {
				damageTxt [i].text = damages [i].ToString();
				damageTxt [i].color = Color.red;
			} 
			else {
				damageTxt [i].color = Color.black;
			}
		}
    }

    private void RoundEnd()
    {
        bool damageChange = false;
		if (charaDamages.Count > 0) {
			foreach (KeyValuePair<int, List<RaycastData>> kv in charaDamages) {
                int sumDamage = 0;
                foreach (var data in charaDamages[kv.Key])
                {
                    sumDamage = sumDamage + data.damage;
                }
					
                charaDamage[kv.Key] = sumDamage;
			}
		}

		foreach (GroundController gc in norGcs) {
			gc.SetType();
		}

        
		StartCoroutine(CheckDamage());
		//OldCheckDamage();


        charaIdx = null;
        startCharaImage = endCharaImage = null;
    }

	private void OldCheckDamage() {
		foreach (KeyValuePair<int, List<RaycastData>> kv in charaDamages) {
			if (kv.Value.Count > recCharaDamages [kv.Key].Count) {
				foreach (var data in kv.Value) {
					if (recCharaDamages [kv.Key].Count > 0) {
						foreach (var recData in recCharaDamages[kv.Key]) {
						
							if (data.start != recData.start || data.end != recData.end) {
								Debug.Log (data.damage);
							}
						}
					} else {
						Debug.Log (data.damage);
					}
				}
			}
		}
	}

	private IEnumerator CheckDamage() {
		foreach (KeyValuePair<int, List<RaycastData>> kv in charaDamages) {
			if (kv.Value.Count > recCharaDamages [kv.Key].Count) {
				foreach (var data in kv.Value) {
					if (recCharaDamages [kv.Key].Count > 0) {
						bool hasNew = true;
						foreach (var recData in recCharaDamages[kv.Key]) {
							if (data.start == recData.start && data.end == recData.end) {
								hasNew = false;
								break;
							}
 						}
						if (hasNew == true) {
							//Image line = PopImage (rayGroup, false, data.start.transform.localPosition);
							//line.GetComponent<LineConnecter> ().SetConnect (data.start.transform.localPosition, data.end.transform.localPosition);		
							foreach (var v in data.hits) {
								v.ChangeSprite ();
								yield return new WaitForSeconds (0.5f);
							}
							//Debug.Log ("Plus : "+data.damage);
							yield return new WaitForSeconds (0.05f);
						}

					} 
					else {
						//Image line = PopImage (rayGroup, false, data.start.transform.localPosition);
						foreach (var v in data.hits) {
							v.ChangeSprite ();
							yield return new WaitForSeconds (0.5f);
						}
						//line.GetComponent<LineConnecter> ().SetConnect (data.start.transform.localPosition, data.end.transform.localPosition);
						//Debug.Log ("New : " + data.damage);
						yield return new WaitForSeconds (0.05f);
					}
				}
			}
		}
		recCharaDamages = charaDamages;
		/*while (_rayGroup.Count > 0) {
			PopImage (rayGroup);
		}*/
		Debug.Log ("CheckEnd");
    }

    private void ResetDamage() {
        for (int i = 0; i < charaDamage.Length; i++) {
            damageTxt[i].text = charaDamage[i].ToString();
			damageTxt [i].color = Color.black;
        }
    }

	public bool IsCorrectEnd(Vector2 dirNormalized) {
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

	public Vector2 ConvertDirNormalized(Vector2 org, Vector2 dir){

		return (dir - org).normalized;
	}

	private void ResetGround(bool isInit=false){
		foreach (GroundController gc in allGcs) {
			gc.ResetType ();
			gc.matchController.ResetType ();
		}

		charaDamage = new int[5] { 0, 0, 0, 0, 0 };
		for (int i = 0; i < charaDamage.Length; i++) {
			damageTxt [i].text = charaDamage [i].ToString ();
			damageTxt [i].color = Color.black;
		}

		charaDamages = new Dictionary<int, List<RaycastData>> ();
		recCharaDamages = new Dictionary<int, List<RaycastData>> ();

		for (int i = 0; i < 6; i++) {
			recCharaDamages.Add (i, new List<RaycastData> ());
		}

		spaceCorrect = false;
		charaGc = new LinkedList<GroundController> ();

        charaIdx = null;
		startCharaImage = endCharaImage = null;

		isResetGround = false;
		while (_charaGroup.Count > 0) {
			PopImage (CharaGroup);
		}

		if (isInit == false) {
			resetGroundCount++;
		}
		RoundStart (new DirectGroundController ());
	}

    private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis) {
        LayerMask mask = 1 << 8;
        RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

        return hits;
    }

	private void ChangeLayer(){
		foreach (GroundController gc in norGcs) {
			if ((int)gc._groundType != 0 && (int)gc._groundType != 99) {
				ChangeLayer (gc.transform.localPosition);
			}
		}
	}

	private void ChangeLayer(Vector2 center){
		RaycastHit2D[] hits;
		for (int i = 0; i < 6; i++) {
			hits = GetRaycastHits(center, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + i * 60))), 0.97f);
			foreach (var hit in hits) {
				hit.collider.GetComponent<GroundController>().UpLayer();
			}
		}
	}

	List<T> RandomList<T>(int randomCount, T[] array, int? lastCount= null){
		List<T> ListT = new List<T> ();
		int count = lastCount==null?array.Length:(int)lastCount;
		if (count > randomCount) {
			for (int i = 0; i < randomCount; i++) {
				int idx = UnityEngine.Random.Range (0, array.Length);
				while (ListT.Contains (array [idx])) {
					idx = UnityEngine.Random.Range (0, array.Length);
				}

				ListT.Add (array [idx]);
			}
		} else if (count <= randomCount) {
			for (int i = 0; i < array.Length; i++) {
				if (!ListT.Contains (array [i])) {
					ListT.Add (array [i]);
				}
			}
		}
		return ListT;
	}

	private Image PopImage(Transform target = null,bool isPush = true,Vector3? position = null){
		Image image;
		if (!isPush) {

			image = _imagePool.Pop ();

			if (target == CharaGroup) {
				image.GetComponent<RectTransform> ().SetParent (CharaGroup);
				_charaGroup.Push (image);
				image.sprite = CharaSprite [(int)charaIdx];
			} 
			else {
				image.GetComponent<RectTransform> ().SetParent (rayGroup);
				_rayGroup.Push (image);
				image.GetComponent<RectTransform> ().pivot = new Vector2 (0.5f, 0);
				image.gameObject.AddComponent<LineConnecter> ();
				image.type = Image.Type.Sliced;

				image.sprite = lineSprite;
			}
			if (position != null) {
				image.transform.localPosition = (Vector3)position;
                
			}
			image.transform.localScale = Vector3.one;
			image.SetNativeSize ();

			image.gameObject.SetActive (true);

			return image;
		} else {
			if (target == CharaGroup) {
				image = _charaGroup.Pop ();
			} else {
				image = _rayGroup.Pop ();
				Destroy (image.GetComponent<LineConnecter> ());
			}
			image.type = Image.Type.Simple;
			image.GetComponent<RectTransform> ().pivot = new Vector2 (0.5f, 0.5f);
			image.GetComponent<RectTransform> ().SetParent (imagePool);
			image.transform.localPosition = Vector3.zero;
			image.gameObject.SetActive (false);
			image.sprite = null;
			_imagePool.Push (image);
			return null;
		}
	}

	public void SelectChara(int idx){
		charaIdx = idx;
	}


    [Serializable]
	public struct DirectGroundController{
		public GroundController gc;
		public int[] randomList;
	}
}

public enum GroundType{
	None = 0,
	Copper = 1,
	Silver = 2,
	gold = 3,
	Chara = 10,
	Caution = 99,
}

public struct RaycastData {
    public GroundController start;
    public GroundController end;
	public List<GroundController> hits;
    public int damage;
}

public struct PlusDamageData {
    public int charaIdx;
    public GroundController gc;
}

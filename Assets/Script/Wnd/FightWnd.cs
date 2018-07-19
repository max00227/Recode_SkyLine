using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using model.data;
using System;
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

	private CharaLargeData[] characters;

	private MonsterLargeData[] monsters;

	private int[] monsterCdTimes;

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

	int[] recCharaRatios;

	int[] charaRatios;

    [SerializeField]
    public Text[] damageTxt;

    private bool spaceCorrect;

	bool hasDamage;

    List<RaycastData> recAllRatios;

	List<RaycastData> allRatios;

    List<GroundController[]> raycasted;

	List<PlusRatioData> plusRatios;

	Dictionary<int, int> charaMaxRatio;

    [SerializeField]
    ReversalGrounds reversalGrounds;

    [SerializeField]
    Transform reversalGroup;

    Queue<ReversalGrounds> reversalPool = new Queue<ReversalGrounds>();
	Queue<ReversalGrounds> reversingPool = new Queue<ReversalGrounds>();

	void SetData() {
		//foreach()
	}

    // Use this for initialization
    void Start () {
		for (int i = 0; i < 16; i++) {
			Image _image = Instantiate (spriteImage) as Image;
			_image.GetComponent<RectTransform> ().SetParent (imagePool);
			_image.transform.localPosition = Vector3.zero;
			_image.gameObject.SetActive (false);
			_imagePool.Push (_image);
		}

		int c = 0;
        for (int i = 0; i < 8; i++)
        {
            ReversalGrounds reversal = Instantiate(reversalGrounds) as ReversalGrounds;
            reversal.GetComponent<RectTransform>().SetParent(reversalGroup);
			reversal.transform.localPosition = Vector3.zero;
			reversal.name = c.ToString ();
			c++;
            reversalPool.Enqueue(reversal);
        }


        allGcs = groundPool.GetComponentsInChildren<GroundController> ();
		norGcs = new List<GroundController> ();

		resetGroundCount = 0;

		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType == 0) {
				gc.plusRatio = OnPlusRatio;
				norGcs.Add (gc);
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

    private void RecycleRevesal(ReversalGrounds rg) {
		reversalPool.Enqueue (rg);
		rg.onRecycle = null;
		if (reversalPool.Count == 8 && reversingPool.Count == 0) {
			recAllRatios = allRatios;
			if (isResetGround) {
				ResetGround ();
			} else {
				NextRound ();
			}
		}
    }


	private void OnPlusRatio(PlusRatioData plusDamage) {
		Debug.Log (plusDamage.gc.name + " : " + plusDamage.charaIdx);
		plusRatios.Add(plusDamage);
    }

	private void OnProtection(int guardian, int target){
		//Debug.Log ("Guardian : " + guardian + " , Target" + target);
	}

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
			RoundStart();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            ResetGround();
        }

        if (Input.GetKeyDown(KeyCode.H)) {
			CheckGround ();
			ChangeLayer ();
            NextRound(false);
        }	

        if (Input.GetKeyDown (KeyCode.Mouse0)) {
			TouchDown ();
		}

        if (Input.GetKey(KeyCode.Mouse0))
        {
            TouchDrap ();
        }

		if (Input.GetKeyUp (KeyCode.Mouse0)) {
			TouchUp ();
		}
	}


	void RoundStart(bool isCenter = true){
		DirectGroundController dirCenter;
		if (isCenter) {
			dirCenter = center;
		} else {
			dirCenter = angleGc [UnityEngine.Random.Range (0, 7)];
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
		List<GroundController> nextRoundGcs = new List<GroundController>();
		List<GroundController> layerList = new List<GroundController> ();
		List<GroundController> maxLayerGcs = new List<GroundController>();

		foreach (var v in damageTxt) {
			v.color = Color.black;
		}

        int noneCount = 1;
		for (int i = 7; i > 0; i--) {
			foreach (GroundController gc in norGcs) {
				if (maxLayerGcs.Count < 2) {
					if (gc._layer == i) {
						layerList.Add (gc);
					}
				}
			}
				
			foreach(var gc in RandomList (2 - maxLayerGcs.Count, layerList.ToArray ())){
				maxLayerGcs.Add (gc); 
			}
			layerList = new List<GroundController> ();
			if (maxLayerGcs.Count == 2) {
				break;
			}
		}


		for (int i = 7; i > 0; i--) {
			foreach (GroundController gc in norGcs) {
				if (gc._layer >= i && !maxLayerGcs.Contains (gc)) {
					if (i == 1) {
						noneCount++;
					}
					layerList.Add (gc);
				}
			}
		}

		if (maxLayerGcs.Count > 0) {
			nextRoundGcs = maxLayerGcs;
		}

		if (layerList.Count > 0) {
			foreach (var gc in RandomList ((CreateGround*(Convert.ToInt32(!isSpace)+1))-2+(int)Mathf.Ceil(resetGroundCount/2), layerList.ToArray(),noneCount)) {
				nextRoundGcs.Add (gc);
			}
		}

		hasDamage = false;

		if (nextRoundGcs.Count > 0) {
			foreach (GroundController gc in nextRoundGcs) {
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

							/*if (characters [(int)charaIdx].job == 3) {
								endGc.onProtection = OnProtection;
							}*/

                            charaGc.AddLast(startGc);
							startGc.ChangeChara ((int)charaIdx, (int)charaIdx + 1);

							startCharaImage = PopImage (CharaGroup, false, r.gameObject.transform.localPosition);
							endCharaImage = PopImage (CharaGroup, false, r.gameObject.transform.localPosition);

							if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
								isResetGround = true;
							}
						} else {
							startGc.PrevType ();
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
									endGc.ChangeChara ((int)charaIdx, (int)charaIdx + 1);
									/*if (characters [(int)charaIdx].job == 3) {
										endGc.onProtection = OnProtection;
									}*/
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
								ResetDamage();
                                spaceCorrect = false;
                            }
						} 
						else {
							if (endGc == startGc) {
								endGc = null;
								ResetDamage();
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
                        if ((int)r.gameObject.GetComponent<GroundController>()._groundType == 99) {
                            isResetGround = true;
                        }
						RoundEnd();

						ChangeLayer ();

						if (!hasDamage) {
							if (isResetGround) {
								ResetGround ();
								return;
							}
							NextRound ();
							recAllRatios = allRatios;
						}
                    }
                    else {
						PopImage (CharaGroup);
						PopImage (CharaGroup);
						startCharaImage = endCharaImage = null;
						startGc.ResetType ();
                        charaGc.RemoveLast();
                        ResetDamage();
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
		allRatios = new List<RaycastData> ();
		plusRatios = new List<PlusRatioData>();
		hasDamage = false;

		foreach (GroundController gc in charaGc) {
			gc.raycasted = false;
		}
		foreach (GroundController gc in charaGc) {
			ResponseData(gc.OnChangeType());
		}

		if (recAllRatios.Count != allRatios.Count) {
			hasDamage = true;
		}
		ChangeCharaRatio ();
    }
		
	private void ResponseData(List<RaycastData> raycastData){
        foreach (var data in raycastData) {
			allRatios.Add (data);
		}
	}

	private void ChangeCharaRatio()
	{
		charaRatios = new int[5]{ 0, 0, 0, 0, 0 };
		foreach (var data in allRatios)
		{
			if (charaMaxRatio [data.charaIdx] < data.ratio) {
				charaMaxRatio [data.charaIdx] = data.ratio;
			}
			charaRatios [data.charaIdx] = charaRatios [data.charaIdx] + data.ratio;
		}

		for (int i =0;i<charaRatios.Length;i++) {
			damageTxt [i].text = charaRatios[i].ToString ();
			if (recCharaRatios[i] != charaRatios [i]) {
				damageTxt [i].color = Color.red;
			} else {
				damageTxt [i].color = Color.black;
			}
		}
	}
		
    private void RoundEnd()
    {
		recCharaRatios = charaRatios;

		foreach (GroundController gc in norGcs) {
			gc.SetType();
		}

		if (hasDamage) {
			StartCoroutine (CheckRatio ());
		}

        charaIdx = null;
        startCharaImage = endCharaImage = null;
    }

	private IEnumerator CheckRatio() {
		if (allRatios.Count > recAllRatios.Count) {
			foreach (var data in allRatios) {
				if (recAllRatios.Count > 0) {
					bool hasNew = true;
					foreach (var recData in recAllRatios) {
						if (data.start == recData.start && data.end == recData.end) {
							hasNew = false;
							break;
						}
					}
					if (hasNew == true) {
						//Image line = PopImage (rayGroup, false, data.start.transform.localPosition);
						//line.GetComponent<LineConnecter> ().SetConnect (data.start.transform.localPosition, data.end.transform.localPosition);		
						//yield return new WaitForSeconds (0.01f * (data.hits.Count - 1));
						ReversalGrounds rg = reversalPool.Dequeue ();
						rg.SetReversal (data.hits);
						rg.onRecycle = RecycleRevesal;
						reversingPool.Enqueue(rg);
					}

				} else {
					//Image line = PopImage (rayGroup, false, data.start.transform.localPosition);
					//line.GetComponent<LineConnecter> ().SetConnect (data.start.transform.localPosition, data.end.transform.localPosition);
					//yield return new WaitForSeconds (0.01f * (data.hits.Count-1));
					ReversalGrounds rg = reversalPool.Dequeue ();
					rg.SetReversal (data.hits);
					rg.onRecycle = RecycleRevesal;
					reversingPool.Enqueue(rg);
				}
			}
		}

		while (reversingPool.Count > 0) {
			ReversalGrounds rg = reversingPool.Dequeue();
			rg.Run ();
			yield return new WaitForSeconds (0.1f*(rg.reversalGrounds.Count-1));
		}
	}

    private void ResetDamage() {
		for (int i = 0; i < recCharaRatios.Length; i++) {
			damageTxt[i].text = recCharaRatios[i].ToString();
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
		}

		recCharaRatios = new int[5] { 0, 0, 0, 0, 0 };
		for (int i = 0; i < recCharaRatios.Length; i++) {
			damageTxt [i].text = recCharaRatios [i].ToString ();
			damageTxt [i].color = Color.black;
		}

		allRatios = new List<RaycastData> ();
		recAllRatios = new List<RaycastData> ();
		charaMaxRatio = new Dictionary<int, int> ();

		for (int i = 0; i < 5; i++) {
			charaMaxRatio.Add (i, 0);
		}

		spaceCorrect = false;
		charaGc = new LinkedList<GroundController> ();

        charaIdx = null;
		startCharaImage = endCharaImage = null;

		hasDamage = false;

		isResetGround = false;
		while (_charaGroup.Count > 0) {
			PopImage (CharaGroup);
		}

		if (isInit == false) {
			resetGroundCount++;
		}
		RoundStart ();
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
			Debug.Log ("Push");

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
	public int charaIdx;
    public int ratio;
}

public struct PlusRatioData {
    public int charaIdx;
    public GroundController gc;
}
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

	List<CharaImageData> _charaGroup = new List<CharaImageData> ();

	Stack<Image> _imagePool = new Stack<Image> ();

	[SerializeField]
	Image spriteImage;

	int? charaIdx;

	Image startCharaImage;
	Image endCharaImage;

    LinkedList<GroundController> charaGc;

	int resetGroundCount;

    public Sprite[] CharaSprite;

	public Sprite lineSprite;

	int[] recJobRatios;

	int[] jobRatios;

    [SerializeField]
    public Text[] damageTxt;

    private bool spaceCorrect;

	bool hasDamage;

    List<RaycastData> recAllRatios;

	List<RaycastData> allRatios;

    List<GroundController[]> raycasted;

	List<PlusRatioData> plusRatios;

	Dictionary<int, int> jobMaxRatio;

    [SerializeField]
    ReversalGrounds reversalGrounds;

    [SerializeField]
    Transform reversalGroup;

    Queue<ReversalGrounds> reversalPool = new Queue<ReversalGrounds>();
	Queue<ReversalGrounds> reversingPool = new Queue<ReversalGrounds>();

	bool isRuined;

	bool ruining;

	GroundController ruinGc;

	GroundController errorEnd;

	void SetData() {
		for (int i = 0;i<MyUserData.GetTeamData(0).Team.Count;i++) {
			characters[i] = MasterDataManager.GetCharaData (MyUserData.GetTeamData(0).Team[i].id);
			characters [i].Merge (ParameterConvert.GetCharaAbility (characters [i], MyUserData.GetTeamData (0).Team [i].lv));
		}
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

		characters = new CharaLargeData[5];
		monsters = new MonsterLargeData[5];

		SetData ();
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

		if (Input.GetKeyDown(KeyCode.Y)) {
			foreach (var v in charaGc) {
				Debug.Log (v.name);
			}
		}

		if (Input.GetKeyDown(KeyCode.U)) {
			foreach (var v in _charaGroup) {
				Debug.Log (v.linkGc.name);
			}
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

		int focusCount = 1;

		foreach (var v in damageTxt) {
			v.color = Color.black;
		}

        int noneCount = 1;
		for (int i = 7; i > 0; i--) {
			foreach (GroundController gc in norGcs) {
				if (maxLayerGcs.Count < focusCount) {
					if (gc._layer == i) {
						layerList.Add (gc);
					}
				}
			}
				
			foreach(var gc in RandomList (focusCount - maxLayerGcs.Count, layerList.ToArray ())){
				maxLayerGcs.Add (gc); 
			}
			layerList = new List<GroundController> ();
			if (maxLayerGcs.Count == focusCount) {
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
			foreach (var gc in RandomList ((CreateGround*(Convert.ToInt32(!isSpace)+1))-focusCount+(int)Mathf.Ceil(resetGroundCount/2), layerList.ToArray(),noneCount)) {
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
			var result = CanvasManager.Instance.GetRaycastResult ();
		if (result.Count > 0) {
			foreach (var r in result) {
				if (r.gameObject.tag == "fightG") {
					
					if ((int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 0
					    || (int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 99) {
						startGc = r.gameObject.GetComponent<GroundController> ().matchController;
						if (charaIdx != null) {
							if (characters [(int)charaIdx].job == 3) {
								endGc.onProtection = OnProtection;
							}

							charaGc.AddLast (startGc);
							startGc.ChangeChara ((int)charaIdx, characters [(int)charaIdx].job, null);

							startCharaImage = PopImage (_imagePool, startGc, r.gameObject.transform.localPosition);
							endCharaImage = PopImage (_imagePool, null, r.gameObject.transform.localPosition);

							if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
								isResetGround = true;
							}
						} 
					} else if ((int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 10 && CanRuin && !isRuined) {
						if (r.gameObject.GetComponent<GroundController> ().matchController.isActived
						    && r.gameObject.GetComponent<GroundController> ().matchController.pairGc.isActived
							&& !r.gameObject.GetComponent<GroundController> ().matchController.isRuined
							&& !r.gameObject.GetComponent<GroundController> ().matchController.pairGc.isRuined) {
							ruining = true;
							ruinGc = r.gameObject.GetComponent<GroundController> ().matchController;
							foreach (var data in _charaGroup) {
								if (data.linkGc == ruinGc) {
									endCharaImage = data.image;	
								}
							}
							endGc = ruinGc;
							charaIdx = endGc.charaIdx;
							startGc = endGc.pairGc;
						}
					}
				}
			}
		}
	}

	private bool CanRuin = true;

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
								if (!ruining) {
									if (charaGc.Last.Value == endGc) {
										endGc.ResetType ();
										charaGc.RemoveLast ();
									}
								} 
								else {
									endGc.ResetType ();
								}
							}

                            if ((int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 0 || (int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 99)
                            {
								GroundController checkGc = r.gameObject.GetComponent<GroundController>().matchController;

								Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, checkGc.transform.localPosition);

                                if (IsCorrectEnd(dir))
                                {
									endGc = r.gameObject.GetComponent<GroundController>().matchController;
									if (ruining) {
										endGc.ChangeChara ((int)charaIdx, characters [(int)charaIdx].job, startGc, ruining);
										if (charaGc.Contains (ruinGc)) {
											charaGc.Find (ruinGc).Value = endGc;
										}
									} 
									else {
										endGc.ChangeChara ((int)charaIdx, characters [(int)charaIdx].job, startGc);
									}

									if (characters [(int)charaIdx].job == 3) {
										endGc.onProtection = OnProtection;
									}

									if (!ruining) {
										charaGc.AddLast (endGc);
									}

                                    CheckGround();
                                    spaceCorrect = true;
                                }
                                else
                                {
									if (endGc != null) {
										errorEnd = endGc;
									}
									endGc = null;
                                    ResetDamage();
                                    spaceCorrect = false;
                                }
                            }
                            else
                            {
								if (endGc != null) {
									errorEnd = endGc;
								}
								endGc = null;
								ResetDamage();
                                spaceCorrect = false;
                            }
						} 
						else {
							if (endGc == startGc) {
								if (endGc != null) {
									errorEnd = endGc;
								}
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

						startGc.pairGc = endGc;

						for (int i = 0; i < _charaGroup.Count; i++) {
							if (_charaGroup [i].image == endCharaImage) {
								CharaImageData imageData = new CharaImageData ();
								imageData.image = endCharaImage;
								imageData.linkGc = endGc;
								_charaGroup [i] = imageData;
							}
						}


						if (!ruining) {
							if (!hasDamage) {
								if (isResetGround) {
									ResetGround ();
									return;
								}
								ChangeLayer ();

								NextRound ();
								recAllRatios = allRatios;
							}
						} 
						else {
							if (endGc != ruinGc) {
								startGc.OnRuined ();
								endGc.OnRuined ();
								isRuined = true;
							}
						}

						RoundEnd();
                    }
                    else {
						if (!ruining) {
							PopImage ();
							PopImage ();
							startCharaImage = endCharaImage = null;
							startGc.ResetType ();
							charaGc.RemoveLast ();
							ResetDamage ();
						} 
						else {
							if (charaGc.Contains (errorEnd)) {
								charaGc.Find (errorEnd).Value = ruinGc;
							}
							endCharaImage.transform.localPosition = ruinGc.matchController.transform.localPosition;
							ruinGc.ChangeChara ((int)charaIdx, characters [(int)charaIdx].job, startGc, ruining);
							ruining = false;
						}
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

		if (CanRuin) {
			if (allRatios.Count != 0) {
				foreach (var ratio in allRatios) {
					if (recAllRatios.Count == 0) {
						hasDamage = true;
					} else {
						foreach (var recRatio in recAllRatios) {
							if (ratio.end != recRatio.end || ratio.ratio != recRatio.ratio) {
								hasDamage = true;
								break;
							} else if (ratio.start != recRatio.start || ratio.ratio != recRatio.ratio) {
								hasDamage = true;
								break;
							} else if (ratio.end != recRatio.end || ratio.start != recRatio.start) {
								hasDamage = true;
								break;
							}
						}
					}
				}
			}
		} else {
			if (allRatios.Count != recAllRatios.Count) {
				hasDamage = true;
			}
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
		jobRatios = new int[5]{ 0, 0, 0, 0, 0 };
		foreach (var data in allRatios) {
			if (jobMaxRatio [data.CharaJob-1] < data.ratio) {
				jobMaxRatio [data.CharaJob-1] = data.ratio;
			}
			jobRatios [data.CharaJob-1] = jobRatios [data.CharaJob-1] + data.ratio;
		}

		for (int i = 0; i < characters.Length; i++) {
			damageTxt [i].text = jobRatios [characters [i].job-1].ToString ();
			if (recJobRatios [characters [i].job-1] != jobRatios [characters [i].job-1]) {
				damageTxt [i].color = Color.red;
			} else {
				damageTxt [i].color = Color.black;
			}
		}
	}
		
    private void RoundEnd()
    {
		recJobRatios = jobRatios;

		foreach (GroundController gc in norGcs) {
			gc.SetType(!ruining);
		}

		if (!ruining) {
			if (hasDamage) {
				StartCoroutine (CheckRatio ());
			}
		}
		ruining = false;

        charaIdx = null;
        startCharaImage = endCharaImage = null;
		startGc = endGc = ruinGc = null;
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
						ReversalGrounds rg = reversalPool.Dequeue ();
						rg.SetReversal (data.hits);
						rg.onRecycle = RecycleRevesal;
						reversingPool.Enqueue(rg);
					}

				} else {
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
		for (int i = 0; i < characters.Length; i++) {
			damageTxt[i].text = recJobRatios[characters[i].job-1].ToString();
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

		recJobRatios = new int[5] { 0, 0, 0, 0, 0 };
		for (int i = 0; i < damageTxt.Length; i++) {
			damageTxt [i].text = "0";
			damageTxt [i].color = Color.black;
		}

		allRatios = new List<RaycastData> ();
		recAllRatios = new List<RaycastData> ();
		jobMaxRatio = new Dictionary<int, int> ();


		for (int i = 0; i < 5; i++) {
			jobMaxRatio.Add (i, 0);
		}

		spaceCorrect = false;
		charaGc = new LinkedList<GroundController> ();

        charaIdx = null;
		startCharaImage = endCharaImage = null;

		hasDamage = false;

		isResetGround = false;
		while (_charaGroup.Count > 0) {
			PopImage ();
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

	private Image PopImage(Stack<Image> pool = null, GroundController linkGc = null, Vector3? position = null){
		Image image;
		if (pool != null) {
			image = pool.Pop ();
			image.GetComponent<RectTransform> ().SetParent (CharaGroup);
			image.sprite = CharaSprite [(int)charaIdx];

			if (position != null) {
				image.transform.localPosition = (Vector3)position;
                
			}
			CharaImageData imageData = new CharaImageData ();
			image.transform.localScale = Vector3.one;
			image.SetNativeSize ();

			image.gameObject.SetActive (true);
			imageData.image = image;
			imageData.linkGc = linkGc;
			_charaGroup.Add (imageData);
			return image;
		} else {
			image = _charaGroup [_charaGroup.Count - 1].image;
			_charaGroup.RemoveRange (_charaGroup.Count - 1, 1);
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
	public int CharaJob;
    public int ratio;
}

public struct PlusRatioData {
    public int charaIdx;
    public GroundController gc;
}

public struct CharaImageData{
	public Image image;
	public GroundController linkGc;
}
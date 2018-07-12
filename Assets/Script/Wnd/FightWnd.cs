using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using model.data;
using System;
using System.Linq;

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
	Transform CharaPool;

	GroundController[] allGcs;

	GroundController startGc;

	GroundController endGc;

	bool isResetGround=false;

    //Dictionary<int, List<GroundSpace>> groundSpaces;

    private CharaLargeData[] characters;

	List<GroundController> norGcs;

	int CreateGround;

	int ResetCount;

	Stack<Image> Group = new Stack<Image> ();

	Stack<Image> Pool = new Stack<Image> ();

	[SerializeField]
	Image charaImage;

	int charaIdx;

	Image startCharaImage;
	Image endCharaImage;

    Stack<GroundController> charaGc;


    public Sprite[] CharaSprite;

    int[] charaDamage;

    [SerializeField]
    public Text[] damageTxt;

    private bool spaceCorrect;

    Dictionary<int, List<int>> charaDamages;

    List<GroundController[]> raycasted;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < 32; i++) {
			Image _charaImage = Instantiate (charaImage) as Image;
			_charaImage.GetComponent<RectTransform> ().SetParent (CharaPool);
			_charaImage.transform.localPosition = Vector3.zero;
			_charaImage.gameObject.SetActive (false);
			Pool.Push (_charaImage);
		}

		allGcs = groundPool.GetComponentsInChildren<GroundController> ();
		norGcs = new List<GroundController> ();

        ResetGround();

		CreateGround = 3;
		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType == 0) {
				norGcs.Add (gc.matchController);
			}
		}
	}

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            FightStart(new DirectGroundController());
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            ResetGround();
        }

        if (Input.GetKeyDown(KeyCode.H)) {
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

    void SetChara() {
        //foreach()
    }


	void FightStart(DirectGroundController dirCenter){
		if (dirCenter.gc == null) {
			dirCenter = center;
		}

		RaycastHit2D[] hits;

		List<int> randomList = RandomList (2, dirCenter.randomList);

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

        /*int noneCount = 1;
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
			foreach (var gc in RandomList ((CreateGround*(Convert.ToInt32(!isSpace)+1))-1, layerList.ToArray(),noneCount)) {
				nextRoundGc.Add (gc.matchController);
			}
		}
		if (maxLayerGc != null) {
			nextRoundGc.Add (maxLayerGc.matchController);
		}

		if (nextRoundGc != null && nextRoundGc.Count > 0) {
			foreach (GroundController gc in nextRoundGc) {
				gc.ChangeType ();
                ChangeLayer(gc.matchController.transform.localPosition);
            }
        }*/
	}

	private void TouchDown(){
		if (charaIdx != null) {
			var result = CanvasManager.Instance.GetRaycastResult ();
			if (result.Count > 0) {
				foreach (var r in result) {
					if (r.gameObject.tag == "fightG") {
						if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 0
						   || (int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
							startGc = r.gameObject.GetComponent<GroundController> ().matchController;
                            charaGc.Push(startGc);
							startGc.ChangeType(charaIdx);

							startCharaImage = PopImage (Pool, r.gameObject.transform.localPosition);
							endCharaImage = PopImage (Pool);
					
							if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
								isResetGround = true;
							}
						} else {
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
                       
                        if (endGc != r.gameObject.GetComponent<GroundController>().matchController
                            && startGc != r.gameObject.GetComponent<GroundController>().matchController) {

							foreach (GroundController gc in charaGc) {
								gc.OnPrevType ();
							}

							if (endGc!=null && charaGc.Peek() == endGc)
							{
								endGc.ResetType ();
                                charaGc.Pop();
                            }
								
                            endCharaImage.transform.localPosition = r.gameObject.transform.localPosition;
							if ((int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 0 || (int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 99){
								endGc = r.gameObject.GetComponent<GroundController>().matchController;

                                Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, endGc.transform.localPosition);


                                if (IsCorrectEnd(dir))
                                {
									endGc.ChangeType (charaIdx);
                                    charaGc.Push(endGc);
                                    CheckGround();
									spaceCorrect = true;
                                }
                                else {
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
                    if (spaceCorrect == true)
                    {
						ChangeLayer ();
						NextRound ();

						RoundEnd();
                    }
                    else {
                        Debug.Log("End Error");
                    }
				}
			}
		}
    }

    private void CheckGround() {
		charaDamages = new Dictionary<int, List<int>> ();
        foreach (GroundController gc in charaGc) {
			ResponseData (gc.OnChangeType ());
        }
		ChangeCharaDamage ();
    }
		
	private void ResponseData(List<RaycastData> raycastData){
		foreach (RaycastData data in raycastData) {
			if (charaDamages.Count == 0 || !charaDamages.ContainsKey (data.charaIdx)) {
				charaDamages.Add (data.charaIdx, new List<int> ());
			}
			charaDamages [data.charaIdx].Add (data.damage);
		}
	}

    private void ChangeCharaDamage()
    {
        foreach (KeyValuePair<int, List<int>> kv in charaDamages)
        {
            damageTxt[kv.Key].text = (kv.Value.Take(kv.Value.Count).Sum()).ToString();
        }
    }

    private void RoundEnd()
    {
		if (charaDamages.Count > 0) {
			foreach (KeyValuePair<int, List<int>> kv in charaDamages) {
				charaDamage [kv.Key] = kv.Value.Take (kv.Value.Count).Sum ();
			}
		}

		foreach (GroundController gc in norGcs) {
			gc.SetType();
		}
    }

    private void ResetDamage() {
        for (int i = 0; i < charaDamage.Length; i++) {
            damageTxt[i].text = charaDamage[i].ToString();
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

	private void ResetGround(){
		foreach (GroundController gc in allGcs) {
			gc.ResetType ();
			gc.matchController.ResetType ();
		}

		charaDamage = new int[5] { 0, 0, 0, 0, 0 };

		charaDamages = new Dictionary<int, List<int>> ();

		spaceCorrect = false;
		charaGc = new Stack<GroundController> ();

		isResetGround = false;
		while (Group.Count > 0) {
			PopImage (Group);
		}
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

	private Image PopImage(Stack<Image> stack,Vector3? position = null){
		Image image = stack.Pop ();
		if (stack == Pool) {
			image.GetComponent<RectTransform> ().SetParent (CharaGroup);
			if (position != null) {
				image.transform.localPosition = (Vector3)position;
			}
			image.sprite = CharaSprite [(int)charaIdx];
			image.gameObject.SetActive (true);
				Group.Push (image);
			return image;
		} else {
			image.GetComponent<RectTransform> ().SetParent (CharaPool);
			image.transform.localPosition = Vector3.zero;
			image.gameObject.SetActive (false);
			image.sprite = null;
			Pool.Push (image);
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
    public int damage;
	public int charaIdx;
}

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

	int? charaIdx;

	Image startCharaImage;
	Image endCharaImage;

    Stack<GroundController> charaGc;


    public Sprite[] CharaSprite;

	bool CheckSpace = false;

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
				norGcs.Add (gc);
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

        ChangeType(dirCenter.gc.matchController);
        ChangeLayer(dirCenter.gc.matchController.transform.localPosition);

        if (randomList.Count>0){
			foreach (int randomI in randomList) {
				hits = GetRaycastHits(dirCenter.gc.matchController.transform.localPosition, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + dirCenter.randomList[randomI] * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + dirCenter.randomList[randomI] * 60))), 0.97f);
				if (hits.Length > 0) {
                    int i = 0;
					foreach (var hit in hits) {
                        i++;
                        ChangeType(hit.collider.GetComponent<GroundController>());
                        ChangeLayer(hit.transform.localPosition);
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
        CheckSpace = false;

        int noneCount = 1;
		for (int i = 6; i > 0; i--) {
			foreach (GroundController gc in norGcs) {
				if (maxLayerGc == null) {
                    if (gc.matchController._layer == i) {
						layerList.Add (gc);
					}
				} else {
                    if (gc.matchController._layer >= i && gc != maxLayerGc) {
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
				nextRoundGc.Add (gc);
			}
		}
		if (maxLayerGc != null) {
            nextRoundGc.Add (maxLayerGc);
		}

		if (nextRoundGc != null && nextRoundGc.Count > 0) {
			foreach (GroundController gc in nextRoundGc) {
				ChangeType (gc.matchController);
                ChangeLayer(gc.matchController.transform.localPosition);
            }
        }
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
                            ChangeType(startGc, GroundType.Chara, charaIdx);

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

                            if (endGc!=null && charaGc.Peek() == endGc)
                            {
                                ResetType(endGc);
                                charaGc.Pop();
                            }
                            endCharaImage.transform.localPosition = r.gameObject.transform.localPosition;
                            if ((int)r.gameObject.GetComponent<GroundController>()._groundType == 0 || (int)r.gameObject.GetComponent<GroundController>()._groundType == 99){
                                endGc = r.gameObject.GetComponent<GroundController>().matchController;

                                Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, endGc.transform.localPosition);

                                if (IsCorrectEnd(dir))
                                {
                                    ChangeType(endGc, GroundType.Chara, charaIdx);
                                    charaGc.Push(endGc);
                                    CheckGround();
                                }
                                else {
                                    Debug.Log("1, " + charaGc.Count);
                                    PrevGround();
                                    ResetDamage();
                                    spaceCorrect = false;
                                }
                            }
                            else
                            {
                                PrevGround();

                                ResetDamage();
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
                        ChangeLayer(startGc.transform.localPosition);
                        ChangeLayer(endGc.transform.localPosition);
                        if (charaDamages.Count > 0)
                        {
                            SetCharaDamage();
                        }
                        NextRound();
                    }
                    else {
                        Debug.Log("End Error");
                    }
				}
			}
		}
    }

    private void CheckGround() {
        charaDamages = new Dictionary<int, List<int>>();
        raycasted = new List<GroundController[]>();
        foreach (GroundController gc in charaGc) {
            RaycastRound(gc);
        }
        spaceCorrect = true;
    }

    private void RaycastRound(GroundController _center) {
        RaycastHit2D[] hits;
        for (int i = 0; i < 6; i++)
        {
            hits = GetRaycastHits(_center.transform.localPosition, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos(Mathf.Deg2Rad * (30 + i * 60))), 0.97f * 8);

            List<RaycastHit2D> hitGcs = new List<RaycastHit2D>();
            for (int j = 0; j < hits.Length; j++)
            {
                hitGcs.Add(hits[j]);
                if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 10)
                {
                    if (hits[j].transform.GetComponent<GroundController>().charaIdx == _center.charaIdx)
                    {
                        if (charaDamages.Count == 0 || !charaDamages.ContainsKey((int)_center.charaIdx))
                        {
                            charaDamages.Add((int)_center.charaIdx, new List<int>());
                        }
                        if (hitGcs.Count > 1)
                        {
                            if (raycasted.Count > 0)
                            {
                                foreach (var gcs in raycasted)
                                {
                                    if (!gcs.Contains(_center) || !gcs.Contains(hits[j].transform.GetComponent<GroundController>()))
                                    {
                                        charaDamages[(int)_center.charaIdx].Add(CalculateDamage(hitGcs.ToArray(), _center.isActived));
                                        raycasted.Add(new GroundController[2] { _center, hits[j].transform.GetComponent<GroundController>()});
                                    }
                                }
                            }
                            else {
                                charaDamages[(int)_center.charaIdx].Add(CalculateDamage(hitGcs.ToArray(), _center.isActived));
                                raycasted.Add(new GroundController[2] { _center, hits[j].transform.GetComponent<GroundController>()});
                            }
                        }
                        break;
                    }
                }
            }
        }

        ChangeCharaDamage();
    }


    private int CalculateDamage(RaycastHit2D[] hits, bool isActived)
    {
        int extraDamage = 0;
       
        if (Array.TrueForAll(hits, HasDamage))
        {
            foreach (var hit in hits)
            {
                if ((int)hit.collider.GetComponent<GroundController>()._groundType != 10)
                {
                    if (!hits[hits.Length - 1].transform.GetComponent<GroundController>().isActived)
                    {
                        ChangeType(hit.collider.GetComponent<GroundController>());
                    }

                    switch ((int)hit.collider.GetComponent<GroundController>()._groundType)
                    {
                        case 2:
                            extraDamage = extraDamage + 50;
                            break;
                        case 3:
                            extraDamage = extraDamage + 75;
                            break;
                    }
                }
            }
        }
        else
        {
            PrevGround();
        }

        return extraDamage;
    }

    private void ChangeCharaDamage()
    {
        foreach (KeyValuePair<int, List<int>> kv in charaDamages)
        {
            damageTxt[kv.Key].text = (kv.Value.Take(kv.Value.Count).Sum()).ToString();
        }
    }

    private void SetCharaDamage()
    {
        if (charaDamage[(int)charaIdx] != charaDamages[(int)charaIdx].Take(charaDamages[(int)charaIdx].Count).Sum())
        {
            startGc.isActived = true;
            endGc.isActived = true;
        }

        foreach (GroundController gc in norGcs) {
            gc.SetType();
        }

        foreach (KeyValuePair<int, List<int>> kv in charaDamages)
        {
            charaDamage[kv.Key] = kv.Value.Take(kv.Value.Count).Sum();
        }
    }

    private bool HasDamage(RaycastHit2D hit){
        //Debug.Log((int)hit.collider.GetComponent<GroundController>()._groundType);
		if ((int)hit.collider.GetComponent<GroundController> ()._groundType == 0) {
			return false;
		} else {
			return true;
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
			gc.ResetType (true);
			gc.matchController.ResetType (true);
		}

        charaDamage = new int[5] { 0, 0, 0, 0, 0 };

        charaDamages = new Dictionary<int, List<int>>();

        spaceCorrect = false;
        charaGc = new Stack<GroundController>();

        isResetGround = false;
		while (Group.Count > 0) {
			PopImage (Group);
		}
	}

    private void PrevGround() {
        foreach (GroundController gc in norGcs)
        {
            if (!charaGc.Contains(gc.matchController)) {
               
                gc.ResetType();
                gc.matchController.ResetType();
            }
        }
    }

    private void ResetType(GroundController gc) {
        gc.ResetType();
        gc.matchController.ResetType();
    }

    private void ChangeType(GroundController gc, GroundType type = GroundType.None, int? charaIdx = null)
    {
        gc.ChangeType(type, charaIdx);
        gc.matchController.ChangeType(type, charaIdx);
    }

    private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis) {
        LayerMask mask = 1 << 8;
        RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

        return hits;
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using model.data;
using System;

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

	List<GroundSpace> groundSpaces;

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

	public Sprite[] CharaSprite;

	bool CheckSpace = false;

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
		CreateGround = 3;
		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType == 0) {
				norGcs.Add (gc);
			}
		}
        groundSpaces = new List<GroundSpace> ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.G)) {
			FightStart (new DirectGroundController ());
		}

		if (Input.GetKeyDown (KeyCode.R)) {
			ResetGround ();
		}

		if (Input.GetKeyDown (KeyCode.H)) {
			NextRound (false);
		}

		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			TouchDown ();
		}

		if (Input.GetKey (KeyCode.Mouse0)) {
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
		
		if(randomList.Count>0){
			foreach (int randomI in randomList) {
				hits = GetRaycastHits(dirCenter.gc.matchController.transform.localPosition, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + dirCenter.randomList[randomI] * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + dirCenter.randomList[randomI] * 60))), 0.97f);
				if (hits.Length > 0) {
					foreach (var hit in hits) {
                        ChangeType(hit.collider.GetComponent<GroundController>());
					}
				}
			}
		}
	}

	private void NextRound(bool isSpace = true){
		List<GroundController> nextRoundGc = new List<GroundController> ();
		List<GroundController> layerList = new List<GroundController> ();
		GroundController maxLayerGc = null;
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
		Debug.Log (noneCount);

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
					if (r.gameObject.tag == "fightG") {
						endGc = r.gameObject.GetComponent<GroundController> ().matchController;
						endCharaImage.transform.localPosition = r.gameObject.transform.localPosition;
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
					if (r.gameObject.tag == "fightG") {
						if (((int)endGc._groundType == 0
						   || (int)endGc._groundType == 99)
						   && startGc != null
						   && startGc.gameObject != endGc.matchController.gameObject) {
                        
							CalculatePlace ();

							if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
								isResetGround = true;
							}
						} else {
							isResetGround = false;
							startGc = null;
							endGc = null;
							PopImage (Group);
							PopImage (Group);
							Debug.Log ("End Error");
						}
					}
				}
			}
		}
	}

	private void CalculatePlace(int? charaIdx = null){
		Vector2 dir = ConvertDirNormalized (startGc.transform.localPosition, endGc.transform.localPosition);
		if (IsCorrectEnd (dir)) {
			float dis = Vector2.Distance (startGc.transform.localPosition, endGc.transform.localPosition);
			RaycastHit2D[] hits;
			hits = GetRaycastHits(startGc.transform.localPosition, dir, dis);
			if (hits.Length > 0) {
                ChangeType(endGc, GroundType.Chara, charaIdx);
                ChangeType(startGc, GroundType.Chara, charaIdx);

                CalculateDamage(hits);
			}
		}			
	}

   

	private void CalculateDamage(RaycastHit2D[] hits){
		bool isNone = false;
		int extraDamage = 0;

		GroundSpace groundSpace = new GroundSpace ();
		groundSpace.start = startGc;
        groundSpace.end = endGc;
        groundSpace.hits = new List<GroundController>();
		foreach (var hit in hits) {
            if ((int)hit.collider.GetComponent<GroundController>()._groundType < 10) {
                groundSpace.hits.Add(hit.collider.GetComponent<GroundController>());
            }
		}

		if (Array.TrueForAll (hits, HasDamage)) {
			foreach (var hit in hits) {
                ChangeType(hit.collider.GetComponent<GroundController>());

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

			groundSpace.isActive = true;
			Debug.Log ("Damage " + extraDamage);
		} 
		else {
			groundSpace.isActive = false;
            Debug.Log("Damage " + extraDamage);
		}

		if (CheckSpace == false && groundSpaces.Count > 0) {
			CheckActiveFalse ();
			CheckSpace = true;
		}

		if (isResetGround) {
			ResetGround ();
		} else {
            groundSpaces.Add (groundSpace);
            NextRound();
		}
	}

	private void CheckActiveFalse(){
		Debug.Log (groundSpaces.Count);
		foreach (GroundSpace space in groundSpaces) {
			if (space.isActive == false) {
				Debug.Log ("Start : " + space.start.name + " , End : " + space.end.name);
				/*startGc = space.start;
				endGc = space.end;
				CalculatePlace ();*/
			}
		}
	}

	private bool HasDamage(RaycastHit2D hit){
		if ((int)hit.collider.GetComponent<GroundController> ()._groundType == 0) {
			return false;
		} else {
			return true;
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
        Debug.Log("ResetGround");
		foreach (GroundController gc in allGcs) {
			gc.ResetType ();
			gc.matchController.ResetType ();
		}
        isResetGround = false;
        groundSpaces = new List<GroundSpace> ();
		while (Group.Count > 0) {
			PopImage (Group);
		}
	}

    private void ChangeType(GroundController gc, GroundType type = GroundType.None, int? charaIdx = null)
    {
        gc.ChangeType(type, charaIdx);
        gc.matchController.ChangeType(type, charaIdx);

		ChangeLayer (gc.transform.localPosition);
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

    private struct GroundSpace {
        public GroundController start;
        public GroundController end;
        public List<GroundController> hits;
        public bool isActive;
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

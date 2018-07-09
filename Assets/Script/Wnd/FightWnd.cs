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

	GroundController[] allGcs;

	GroundController orgGc;

	bool isResetGround=false;

	List<GroundSpace> groundSpaces;

    private CharaLargeData[] characters;

	List<GroundController> norGcs;

	int CreateGround;

	int ResetCount;

	// Use this for initialization
	void Start () {
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
        Debug.Log("NextRound");
		List<GroundController> nextRoundGc = new List<GroundController> ();
		List<GroundController> layerList = new List<GroundController> ();
		GroundController maxLayerGc = null;
		for (int i = 6; i > 0; i--) {
			foreach (GroundController gc in norGcs) {
				if (maxLayerGc == null) {
                    Debug.Log("NextRound2");
                    if (gc.matchController._layer == i) {
						layerList.Add (gc);
					}
				} else {
                    Debug.Log("NextRound3");
                    if (gc.matchController._layer >= i && gc != maxLayerGc) {
						layerList.Add (gc);
					}
				}
			}
				
			if (maxLayerGc == null) {
                Debug.Log("NextRound6");
                if (layerList.Count == 1) {
					maxLayerGc = layerList [0];
				} else if (layerList.Count > 1) {
					maxLayerGc = RandomList (1, layerList.ToArray ()) [0];
				}
				layerList = new List<GroundController> ();
			}
		}

		if (layerList.Count > 0) {
            Debug.Log("NextRound4");
            foreach (var gc in RandomList ((CreateGround*(Convert.ToInt32(!isSpace)+1))-1, layerList.ToArray())) {
				nextRoundGc.Add (gc);
			}
		}
		if (maxLayerGc != null) {
            Debug.Log("NextRound5");
            nextRoundGc.Add (maxLayerGc);
		}

		if (nextRoundGc != null && nextRoundGc.Count > 0) {
			foreach (GroundController gc in nextRoundGc) {
				ChangeType (gc.matchController);
			}
		}
	}

	private void TouchDown(){
		var result = CanvasManager.Instance.GetRaycastResult ();
		if (result.Count > 0) {
			foreach (var r in result) {
				if (r.gameObject.tag == "fightG") {
					if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 0
					    || (int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
						orgGc = r.gameObject.GetComponent<GroundController> ().matchController;

						if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
							isResetGround = true;
						}
					} 
					else {
						Debug.Log ("Start Error");
					}
				}
			}
		}
	}

	private void TouchUp(){
		var result = CanvasManager.Instance.GetRaycastResult ();
		if (result.Count > 0) {
			foreach (var r in result) {
				if (r.gameObject.tag == "fightG") {
                    if (((int)r.gameObject.GetComponent<GroundController>()._groundType == 0
                        || (int)r.gameObject.GetComponent<GroundController>()._groundType == 99)
                        && orgGc != null
                        && orgGc.gameObject != r.gameObject.GetComponent<GroundController>().matchController.gameObject)
                    {
                        
                        CalculatePlace(orgGc, r.gameObject.GetComponent<GroundController>().matchController);

                        if ((int)r.gameObject.GetComponent<GroundController>()._groundType == 99)
                        {
                            isResetGround = true;
                        }
                    }
                    else
                    {
                        isResetGround = false;
                        orgGc = null;
                        Debug.Log("End Error");
                    }
				}
			}
		}
	}

	private void CalculatePlace(GroundController startGO, GroundController endGo, int? charaIdx = null){
		Vector2 dir = ConvertDirNormalized (startGO.transform.localPosition, endGo.transform.localPosition);
		if (IsCorrectEnd (dir)) {
			float dis = Vector2.Distance (startGO.transform.localPosition, endGo.transform.localPosition);
			RaycastHit2D[] hits;
			hits = GetRaycastHits(startGO.transform.localPosition, dir, dis);
			if (hits.Length > 0) {
                ChangeType(endGo, GroundType.Chara, charaIdx);
                ChangeType(startGO, GroundType.Chara, charaIdx);

                CalculateDamage(hits, endGo);
			}
		}			
	}

   

	private void CalculateDamage(RaycastHit2D[] hits, GroundController endGc){
		bool isNone = false;
		int extraDamage = 0;

		GroundSpace groundSpace = new GroundSpace ();
        groundSpace.start = orgGc;
        groundSpace.end = endGc;
        groundSpace.hits = new List<GroundController>();
		foreach (var hit in hits) {
            if ((int)hit.collider.GetComponent<GroundController>()._groundType < 10) {
                groundSpace.hits.Add(hit.collider.GetComponent<GroundController>());
            }
		}

        Debug.Log(groundSpace.hits.Count);

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
			Debug.Log ("Damage " + extraDamage);
		} 
		else {
            Debug.Log("Damage " + extraDamage);
		}

		if (isResetGround) {
			ResetGround ();
		} else {
            groundSpaces.Add (groundSpace);
            NextRound();
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

	List<int> RandomInt(int iCount, int length){
		List<int> intL = new List<int> ();
		if (length > iCount) {
			for (int i = 0; i < iCount; i++) {
				int idx = UnityEngine.Random.Range (0, length);
				while (intL.Contains (idx)) {
					idx = UnityEngine.Random.Range (0, length);
				}

				intL.Add (idx);
			}
		} else if (length <= iCount) {
			for (int i = 0; i < length; i++) {
				intL.Add (i);
			}
		}
		return intL;
	}

	List<T> RandomList<T>(int iCount, T[] array){
		List<T> ListT = new List<T> ();
		if (array.Length > iCount) {
			for (int i = 0; i < iCount; i++) {
				int idx = UnityEngine.Random.Range (0, array.Length);
				while (ListT.Contains (array[idx])) {
					idx = UnityEngine.Random.Range (0, array.Length);
				}

				ListT.Add (array[idx]);
			}
		} else if (array.Length <= iCount) {
			Debug.Log ("Over");
			for (int i = 0; i < array.Length; i++) {
				ListT.Add (array[i]);
			}
		}
		return ListT;
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

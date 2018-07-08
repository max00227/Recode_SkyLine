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

	GroundController[] allGc;

	GroundController orgGc;

	bool isResetGround=false;

	List<GroundSpace> groundSpaces;

    private CharaLargeData[] characters;

    

	// Use this for initialization
	void Start () {
		allGc = groundPool.GetComponentsInChildren<GroundController> ();
        groundSpaces = new List<GroundSpace> ();
	}

    void SetChara() {
        //foreach()
    }


	void FightStart(DirectGroundController dirCenter){
		if (dirCenter.gc == null) {
			dirCenter = center;
		}

		RaycastHit2D[] hits;

		List<int> randomList = RandomInt (2, dirCenter.randomList);
        ChangeType(dirCenter.gc);
		
		if(randomList.Count>0){
			foreach (int randomI in randomList) {
				hits = GetRaycastHits(dirCenter.gc.matchController.transform.localPosition, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + randomI * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + randomI * 60))), 0.97f);
				if (hits.Length > 0) {
					foreach (var hit in hits) {
						Debug.Log (hit.collider+","+hit.collider.transform.parent.name);
                        ChangeType(hit.collider.GetComponent<GroundController>());
					}
				}
			}
		}
	}

	List<int> RandomInt(int iCount, int[] randomList){
		List<int> intL = new List<int> ();
		if (randomList.Length > iCount) {
			for (int i = 0; i < iCount; i++) {
				int idx = UnityEngine.Random.Range (0, randomList.Length);
				while (intL.Contains (idx)) {
					idx = UnityEngine.Random.Range (0, randomList.Length);
				}
				intL.Add (randomList[idx]);
			}
		} else if (randomList.Length == iCount) {
			for (int i = 0; i < iCount; i++) {
				intL.Add (randomList[i]);
			}
		}
		return intL;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.G)) {
			FightStart (new DirectGroundController ());
		}

		if (Input.GetKeyDown (KeyCode.R)) {
			ResetGround ();
		}


		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			TouchDown ();
		}

		if (Input.GetKeyUp (KeyCode.Mouse0)) {
			TouchUp ();
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
		foreach (GroundController gc in allGc) {
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
    }

    private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis) {
        LayerMask mask = 1 << 8;
        RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

        return hits;
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

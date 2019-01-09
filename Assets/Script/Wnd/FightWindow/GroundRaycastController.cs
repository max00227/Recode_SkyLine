using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GroundRaycastController : MonoBehaviour {
	GroundController[] allGcs;

	[SerializeField]
	bool isPortrait = false;

	[SerializeField]
	DirectGroundController center;

	int CreateGround;

	int constAngle;

	// Use this for initialization
	public void SetController () {
		allGcs = GetComponentsInChildren<GroundController> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetCreateGround (int count){
		CreateGround = count;
	}

	public void RoundStart(bool isCenter = true){
		RaycastHit2D[] hits;

		constAngle = isPortrait == true ? 0 : 30;

		List<int> randomList = new List<int>();


		if (allGcs.Length >= 61)
		{
			randomList = DataUtil.RandomList(CreateGround - 1, center.randomList);
		}
		else
		{
			randomList = DataUtil.RandomList(CreateGround, center.randomList);
		}

		center.gc.ChangeType (false, true);

		if (randomList.Count>0){
			foreach (int randomI in randomList) {
				hits = GetRaycastHits(center.gc.transform.position, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (constAngle + center.randomList[randomI] * 60)), Mathf.Cos (Mathf.Deg2Rad * (constAngle + center.randomList[randomI] * 60))), 0.97f);
                if (hits.Length > 0) {
					foreach (var hit in hits) {
						hit.collider.GetComponent<GroundController> ().ChangeType (false, true);
					}
				}
			}
		}
	}

	public void RoundEnd()
	{
		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType != 99) {
				gc.SetType ();
			}
		}
	}

	public void ChangeLayer(){
		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType != 0 && (int)gc._groundType != 99) {
				ChangeLayer (gc.transform.position);
			}
		}
	}

	private void ChangeLayer(Vector2 center){
		RaycastHit2D[] hits;
		for (int i = 0; i < 6; i++) {
			hits = GetRaycastHits(center, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (constAngle + i * 60)), Mathf.Cos (Mathf.Deg2Rad * (constAngle + i * 60))), 0.97f);
			foreach (var hit in hits) {
				hit.collider.GetComponent<GroundController>().UpLayer();
			}
		}
	}

	private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis) {
		LayerMask mask = 1 << 8;
		RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

		return hits;
	}

	public bool NextRound (){
		List<GroundController> nextRoundGcs = new List<GroundController> ();
		List<GroundController> layerList = new List<GroundController> ();
		List<GroundController> hasLayerGcs = new List<GroundController> ();
		List<GroundController> layerGcs = new List<GroundController> ();
		GroundController maxGc = null;

		int focusCount = 1;

		foreach (GroundController gc in allGcs) {
			if (gc._layer != 0 && (int)gc._groundType != 99) {
				hasLayerGcs.Add (gc);
			}
		}

		if (hasLayerGcs.Count == 0) {
			return false;
		} 
		else if (hasLayerGcs.Count <= CreateGround) {
			nextRoundGcs = hasLayerGcs;
		} 
		else {
			while (nextRoundGcs.Count < CreateGround - 1) {
				if (maxGc == null) {
					for (int i = 7; i > 0; i--) {
						foreach (GroundController gc in hasLayerGcs) {
							if (gc._layer == i) {
								layerList.Add (gc);
							}
						}
							
						if (layerList.Count > 0) {
							maxGc = DataUtil.RandomList (focusCount, layerList.ToArray ()) [0];
							break;
						}
					}
				} 
				else {
					for (int i = 1; i <= 7; i++) {
						layerList = new List<GroundController> ();
						foreach (GroundController gc in hasLayerGcs) {
							if (gc._layer == i) {
								if (gc != maxGc && !nextRoundGcs.Contains (gc)) {
									layerList.Add (gc);
								}
							}
						}
						if (layerList.Count > 0) {
							layerGcs.Add (DataUtil.RandomList (1, layerList.ToArray ()) [0]);
						}
					}

					List<GroundController> gcs = new List<GroundController> ();

					for (int i = 0; i < layerGcs.Count; i++) {
						for (int j = 0; j < Mathf.FloorToInt(i/2) + 1; j++) {
							gcs.Add (layerGcs [i]);
						}
					}

					//避免因數量不足進入無窮迴圈
					if (nextRoundGcs.Count == 0) {
						nextRoundGcs = DataUtil.RandomList (CreateGround - 1 - nextRoundGcs.Count, gcs.ToArray (), layerGcs.Count);
					} 
					else {
						List<GroundController> randomGcs = DataUtil.RandomList (CreateGround - 1 - nextRoundGcs.Count, gcs.ToArray (), layerGcs.Count);
						if (randomGcs.Count > 0) {
							Debug.LogWarning (randomGcs.Count);
							foreach (GroundController gc in randomGcs) {
								nextRoundGcs.Add (gc);
							}
						}
					}
				}
			}
			nextRoundGcs.Add (maxGc);
		}

		foreach (GroundController gc in nextRoundGcs) {
			gc.ChangeType (false, true);
		}
		return true;
	}

	[Serializable]
	public struct DirectGroundController{
		public GroundController gc;
		public int[] randomList;
	}
}

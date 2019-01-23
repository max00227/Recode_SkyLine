using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GroundRaycastController : MonoBehaviour {
	GroundController[] allGcs;

	[SerializeField]
	bool isPortrait = false;

	[SerializeField]
	GroundController center;

    public int[] randomDir;

    int CreateGround;

	int constAngle;

	// Use this for initialization
	public void SetController () {
		allGcs = GetComponentsInChildren<GroundController> ();
	}

    float canvasScale;
	
	// Update is called once per frame
	void Start () {
        canvasScale = CanvasManager.Instance.GetCanvasScale().x;

    }

	public void SetCreateGround (int count){
		CreateGround = count;
	}

    public void SetCenter(GroundController gc) {
        center = gc;
    }

    public void RoundStart(bool isCenter = true){
		RaycastHit2D[] hits;

		constAngle = isPortrait == true ? 0 : 30;

		List<int> randomList = new List<int>();


		
		center.ChangeType (false, true);

        List<int> currentHit = new List<int>();
        int currentCount = 0;

        while (currentCount < CreateGround -1) {
            if (allGcs.Length >= 61)
            {
                randomList = DataUtil.RandomList(CreateGround - 1 -currentCount, randomDir);
            }
            else
            {
                randomList = DataUtil.RandomList(CreateGround - currentCount, randomDir);
            }


            if (randomList.Count > 0)
            {
                foreach (int randomI in randomList)
                {
                    if (!currentHit.Contains(randomI))
                    {
                        hits = GetRaycastHits(center.transform.position, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (constAngle + randomDir[randomI] * 60)), Mathf.Cos(Mathf.Deg2Rad * (constAngle + randomDir[randomI] * 60))), 116f * 8);
                        if (hits.Length > 2)
                        {
                            hits[0].collider.GetComponent<GroundController>().ChangeType(false, true);
                            currentCount++;
                            currentHit.Add(randomI);
                        }
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
			hits = GetRaycastHits(center, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (constAngle + i * 60)), Mathf.Cos (Mathf.Deg2Rad * (constAngle + i * 60))), 116f);
			foreach (var hit in hits) {
				hit.collider.GetComponent<GroundController>().UpLayer();
			}
		}
	}

	public RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis) {
        LayerMask mask = 1 << 9;

		RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis * canvasScale, mask);

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
		
	}
}

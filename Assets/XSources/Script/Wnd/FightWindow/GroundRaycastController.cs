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



        center.ChangeType(true, false);

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
                            hits[0].collider.GetComponent<GroundController>().ChangeType(true, false);
                            currentCount++;
                            currentHit.Add(randomI);
                        }
                    }
                }
            }
        }

        OnBonus();
    }

    public void RoundEnd()
	{
		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType != 99) {
				gc.SetType ();
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
		List<GroundController> noneGcs = new List<GroundController> ();

		foreach (GroundController gc in allGcs) {
			if (gc._groundType == GroundType.None) {
				noneGcs.Add (gc);
			}
		}
        if (noneGcs.Count == 0) {
            return false;
        }

        nextRoundGcs = DataUtil.RandomList(CreateGround, noneGcs.ToArray(), noneGcs.Count);

		foreach (GroundController gc in nextRoundGcs) {
            gc.ChangeType(true, false);
		}

        OnBonus();
		return true;
	}

    public void OnBonus()
    {
        int randomIdx = UnityEngine.Random.Range(0, allGcs.Length);

        while (!(allGcs[randomIdx]._groundType != GroundType.None || allGcs[randomIdx]._groundType != GroundType.Caution))
        {
            randomIdx = UnityEngine.Random.Range(0, allGcs.Length);
        }

        for (int i = 0; i < allGcs.Length; i++)
        {
            allGcs[i].OpenBonus(i == randomIdx);
        }
    }

    [Serializable]
	public struct DirectGroundController{
		public GroundController gc;
		
	}
}

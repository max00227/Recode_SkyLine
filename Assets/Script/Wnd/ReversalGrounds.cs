using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversalGrounds : MonoBehaviour {
   // ReversalGrounds rg;
    List<GroundController> reversalGrounds;
    bool setComplete;
    float reversalTime;
    int reversedCount;

    public delegate void OnRecycle(ReversalGrounds rg);

    public OnRecycle onRecycle;

    // Use this for initialization
    void Start() {
        
    }

    public ReversalGrounds New() {
        return new ReversalGrounds();
    }

	// Update is called once per frame
	void Update () {
        if (setComplete) {
            Debug.Log(reversalGrounds.Count);
            if (reversedCount < reversalGrounds.Count)
            {
                Debug.Log(reversalTime);
                reversalTime -= Time.deltaTime;
                if (reversalTime <= 0) {
                    reversalGrounds[reversedCount].ChangeSprite();
                    reversedCount++;
                    reversalTime = 0.5f;
                }
            }
            else {
                setComplete = false;
                reversedCount = 0;
                onRecycle.Invoke(this);
            }
        }
	}

    public void SetReversal(List<GroundController> grounds)
    {
        reversalGrounds = grounds;
        reversedCount = 0;
        reversalTime = 0;
        setComplete = true;
    }
}

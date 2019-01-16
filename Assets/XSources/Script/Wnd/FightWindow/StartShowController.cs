using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartShowController : MonoBehaviour
{
    [SerializeField]
    Transform raycastGroup;

    [SerializeField]
    ShowStart start;

    [SerializeField]
    ShowStart end;

    [SerializeField]
    ShowStart startCorner;

    [SerializeField]
    ShowStart endCorner;

    [SerializeField]
    int maxCount;

    int collisionCount;

    bool setComplete = false;
    bool isRun = false;
    Vector3 upScale = new Vector3(1, 1, 1);
    [SerializeField]
    float endScale;

    [SerializeField]
    float upSpeed;
    // Start is called before the first frame update
    void Start()
    {
        start.onCollisionObject = CollisionGc;
        end.onCollisionObject = CollisionGc;
        startCorner.onCollisionObject = CollisionGc;
        endCorner.onCollisionObject = CollisionGc;

    }

    // Update is called once per frame
    void Update()
    {
        if (isRun && setComplete) {
            start.transform.localScale += upScale * upSpeed * Time.deltaTime;
            end.transform.localScale += upScale * upSpeed * Time.deltaTime;
            startCorner.transform.localScale += upScale * upSpeed * Time.deltaTime;
            endCorner.transform.localScale += upScale * upSpeed * Time.deltaTime;

            if (collisionCount >= maxCount) {
                isRun = false;
                transform.gameObject.SetActive(false);
                start.transform.localScale = Vector3.one * 0.8f;
                end.transform.localScale = Vector3.one * endScale;
                startCorner.transform.localScale = Vector3.one * 1.09f;
                endCorner.transform.localScale = Vector3.one * (endScale + 0.29f);

            }
        }
    }

    public void SetCenter(int startPoint = 30) {
        transform.position = raycastGroup.GetChild(startPoint).position;
        foreach (GroundController gc in raycastGroup.GetComponentsInChildren<GroundController>()) {
            gc.transform.tag = "raycastG";
        }
        raycastGroup.GetChild(startPoint).tag = "Center";
        raycastGroup.GetChild(startPoint).GetComponent<GroundController>().SetTag();

        setComplete = true;
    }


    public void ShowStart() {
        end.transform.localScale = Vector3.one * endScale;
        endCorner.transform.localScale = Vector3.one * (endScale + 0.29f);
        collisionCount = 0;
        isRun = true;
    }

    public void CollisionGc(GroundController gc, GameObject go)
    {
        if (go.CompareTag("CollisionStart"))
        {
            gc.matchController.OpenColor(0);
        }
        else if (go.CompareTag("CollisionEnd"))
        {
            gc.matchController.CloseColor();
            collisionCount++;
        }
    }
}

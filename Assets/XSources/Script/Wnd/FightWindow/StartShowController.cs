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
    float startScale;

    [SerializeField]
    float upSpeed;

    public delegate void Callback();

    public Callback callback;

    // Start is called before the first frame update
    void Awake()
    {
        start.onCollisionObject = TriggerGc;
        end.onCollisionObject = TriggerGc;
        startCorner.onCollisionObject = TriggerGc;
        endCorner.onCollisionObject = TriggerGc;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRun)
        {
            start.transform.localScale += upScale * upSpeed * Time.deltaTime;
            end.transform.localScale += upScale * upSpeed * Time.deltaTime;
            startCorner.transform.localScale += upScale * upSpeed * Time.deltaTime;
            endCorner.transform.localScale += upScale * upSpeed * Time.deltaTime;

            if (collisionCount >= maxCount)
            {
                isRun = false;
                transform.gameObject.SetActive(false);
                callback.Invoke();
            }
        }
    }

    public void ShowCount()
    {
        Debug.Log(collisionCount);
    }

    public void ShowStart(Vector3 centerPos)
    {
        transform.position = centerPos;
        end.transform.localScale = Vector3.one * endScale;
        endCorner.transform.localScale = Vector3.one * (endScale + 0.29f);
        start.transform.localScale = Vector3.one * startScale;
        startCorner.transform.localScale = Vector3.one * (startScale + 0.29f);

        collisionCount = 0;
        transform.gameObject.SetActive(true);

        isRun = true;
    }

    public void TriggerGc(GroundController gc, GameObject go)
    {
        if (go.CompareTag("CollisionStart"))
        {
            gc.matchController.OpenLight(GroundType.Copper, false);
        }
        else if (go.CompareTag("CollisionEnd"))
        {
            gc.matchController.CloseLight();
            collisionCount++;
        }
    }
}

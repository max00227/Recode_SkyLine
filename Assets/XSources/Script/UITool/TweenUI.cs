using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenUI : MonoBehaviour
{
    public bool isPopup;
    public GameObject popupGo;

    public bool isLoop;

    public float delay;

    public enum DelayType
    {
        Before,
        After
    }

    public DelayType delayType;

    public AnimationCurve[] animationCurves;

    [HideInInspector]
    public AnimationCurve mainAniCurve;

    [HideInInspector]
    public float oriTime = 0;
    [HideInInspector]
    public float recTime;

    [HideInInspector]
    public bool isRun = false;

    [HideInInspector]
    public bool runForward = true;

    [SerializeField]
    public float TweenTime = 1;

    public delegate void RunFinish(TweenUI tu);
    public RunFinish runFinish;


    [HideInInspector]
    public bool delayFinish;

    // Use this for initialization
    public void PlayForward(int idx = 0)
    {
        mainAniCurve = animationCurves[idx];
        Play(true);
    }

    public void PlayReverse(int idx = 0)
    {
        mainAniCurve = animationCurves[idx];
        Play(false);
    }



    void Play(bool isForward)
    {
        runForward = isForward;
        ResetRecTime(true);
        isRun = true;
    }

    public virtual void Reset() { }

    public void TweenEnd() {
        isRun = false;

        if (runFinish !=null)
        {
            if (!DelayCallback())
            {
                runFinish.Invoke(this);
                runFinish = null;
            }
        }
    }

    public void ResetRecTime(bool init = false) {
        delayFinish = true;

        if (delay > 0)
        {
            if (delayType == DelayType.Before && runForward)
            {
                delayFinish = false;
            }
            else if (delayType == DelayType.After && !runForward)
            {
                delayFinish = false;
            }
            else
            {
                if (!init) {
                    delayFinish = false;
                }
            }
        }
        recTime = Time.realtimeSinceStartup;
    }

    public void CalOriTime() {
        if (runForward)
        {
            oriTime = Time.realtimeSinceStartup - recTime;
        }
        else
        {
            oriTime = TweenTime - (Time.realtimeSinceStartup - recTime);
        }
    }

    public bool DelayCallback()
    {
        if (delay > 0)
        {
            if (delayType == DelayType.After && runForward)
            {
                StartCoroutine(DelayTime(delay));
                return true;
            }
            else if (delayType == DelayType.Before && !runForward)
            {
                StartCoroutine(DelayTime(delay));
                return true;
            }
        }

        return false;
    }

    public void SetPopupGameObject(GameObject go)
    {
        popupGo = go;
    }

    IEnumerator DelayTime(float time) {
        yield return new WaitForSeconds(time);
        runFinish.Invoke(this);
        runFinish = null;
    }
}

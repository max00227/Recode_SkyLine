using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenColor : MonoBehaviour
{
    [SerializeField]
    Image image;

    public Color from;

    public Color to;

    [SerializeField]
    AnimationCurve[] animationCurves;

    [SerializeField]
    float TweenTime = 1;

    [SerializeField]
    bool isRoop;

    [SerializeField]
    bool isPopupWnd = false;

    [HideInInspector]
    public GameObject showGameObject;

    bool isRun = false;

    bool runForward = true;

    AnimationCurve mainAniCurve;

    Color orginClr;

    float oriTime = 0;
    float recTime;   

    public delegate void RunFinish(TweenColor tc);

    public RunFinish runFinish;

    // Use this for initialization
    public void PlayForward()
    {
        Play(true);
    }

    public void PlayReverse()
    {
        Play(false);
    }



    void Play(bool isForward)
    {
        runForward = isForward;
        recTime = Time.realtimeSinceStartup;
        isRun = true;
    }

    void Stop()
    {
        isRun = false;
        image.color = orginClr;
    }


    void Start()
    {
        orginClr = image.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRun == true)
        {
            if (isPopupWnd)
            {
                showGameObject.SetActive(true);
            }
            if (runForward)
            {
                oriTime = Time.realtimeSinceStartup - recTime;
            }
            else
            {
                oriTime = TweenTime - (Time.realtimeSinceStartup - recTime);
            }

            Color color = from + (to - from) * mainAniCurve.Evaluate(oriTime / TweenTime);

            image.color = color;

            if (runForward)
            {
                if (oriTime >= TweenTime)
                {
                    isRun = false;

                    if (runFinish != null)
                    {
                        runFinish.Invoke(this);
                    }
                }
            }
            else
            {
                if (oriTime <= 0)
                {
                    if (isPopupWnd)
                    {
                        showGameObject.SetActive(false);
                    }
                    isRun = false;
                    if (runFinish != null)
                    {
                        runFinish.Invoke(this);
                    }
                }
            }
        }
    }

    public void SetFromAndTo(Color f, Color t, int idx = 0)
    {
        from = f;
        to = t;

        mainAniCurve = animationCurves[idx];
    }

    public void SetFromAndTo(string f, string t, int idx = 0)
    {
        from = DataUtil.ColorConvert(f);
        to = DataUtil.ColorConvert(t);
        mainAniCurve = animationCurves[idx];
    }


    public void resetPosition()
    {
        image.color = from;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenColor : TweenUI
{
    [SerializeField]
    Image image;

    public Color from;

    public Color to;

    
    Color orginClr;

    public delegate void RunFinish(TweenColor tc);

    public RunFinish runFinish;


    public void Stop(Color? stopColor = null)
    {
        isRun = false;
        image.color = stopColor == null ? orginClr : (Color)stopColor;
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
                    if (!isLoop)
                    {
                        isRun = false;

                        if (runFinish != null)
                        {
                            runFinish.Invoke(this);
                        }
                    }
                    else
                    {
                        recTime = Time.realtimeSinceStartup;
                    }
                }
            }
            else
            {
                if (oriTime <= 0)
                {
                    if (!isLoop)
                    {
                        isRun = false;
                        if (runFinish != null)
                        {
                            runFinish.Invoke(this);
                        }
                    }
                    else {
                        recTime = Time.realtimeSinceStartup;
                    }
                }
            }
        }
    }

    public void SetFromAndTo(Color f, Color t)
    {
        from = f;
        to = t;
    }

    public void SetFromAndTo(string f, string t)
    {
        from = DataUtil.ColorConvert(f);
        to = DataUtil.ColorConvert(t);
    }


    public void resetPosition()
    {
        image.color = from;
    }
}

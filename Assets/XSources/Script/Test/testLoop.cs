using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testLoop : MonoBehaviour
{

    public TweenColor tc;

    Color a = new Color(1, 1, 1, 1);
    Color b = new Color(1, 1, 1, 0);
    // Start is called before the first frame update
    void Start()
    {
        tc.SetFromAndTo(a, b, 1);
        tc.PlayForward();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

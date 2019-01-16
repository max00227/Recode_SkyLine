using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowStart : MonoBehaviour
{
    [SerializeField]
    bool isCorner;
    private void Update()
    {

    }

    public delegate void OnCollisionObject(GroundController gc, GameObject go);

    public OnCollisionObject onCollisionObject;


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (transform.localScale.x > 0)
        {
            if (!isCorner)
            {
                if (col.gameObject.CompareTag("raycastG"))
                {
                    onCollisionObject.Invoke(col.gameObject.GetComponent<GroundController>(), transform.gameObject);
                }
            }
            else
            {
                if (col.gameObject.CompareTag("raycastGCorner"))
                {
                    onCollisionObject.Invoke(col.gameObject.GetComponent<GroundController>(), transform.gameObject);
                }
            }
        }
    }
}

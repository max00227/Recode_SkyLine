using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(PolygonCollider2D))]
public class UIPolygon : Image 
{
	PolygonCollider2D collider;
	void Awake()
	{
		collider = GetComponent<PolygonCollider2D>();
	}

	public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		bool inside = collider.OverlapPoint(screenPoint);
		return inside;
	}
}
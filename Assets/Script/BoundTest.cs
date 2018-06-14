using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Shape;

public class BoundTest : MonoBehaviour {

	ConvexRect rect;

	public float x = -2.5f;
	public float y = 2.5f;
	public float width = 5.0f;
	public float height = 5.0f;
	public Vector2 center;

	// Use this for initialization
	void Start () {

		rect = new ConvexRect (center, new Vector2 (width, height));
		//rect = new ConvexRect(x,y,width,height);
	}
	
	// Update is called once per frame
	void Update () {

		center = rect.Center;
		//rect.x = x;
		//rect.y = y;
		//rect.width = width;
		//rect.height = height;

		//rect.center = center;
		rect.Width = width;
		rect.Height = height;
	}

	void OnDrawGizmos(){

		if (rect == null)
			return;

		float scale = 0.04f;

		//draw bound
		Gizmos.color = Color.white;
		Vector2[] corners = rect.AllCorners;

		for (int i = 0; i < corners.Length-1; i++) {

			Gizmos.DrawLine (new Vector3 (corners [i].x, corners [i].y),
				new Vector3 (corners [i + 1].x, corners [i + 1].y));
		}

		Gizmos.DrawLine (new Vector3 (corners [3].x, corners [3].y),
			new Vector3 (corners [0].x, corners [0].y));

		//Draw corner
		Gizmos.color = Color.red;
		for (int i = 0; i < corners.Length; i++) {

			Gizmos.DrawCube (new Vector3(corners[i].x, corners[i].y),
				new Vector3(rect.Width * scale, rect.Height * scale));
		}

		//draw center
		Gizmos.color = Color.red;
		Gizmos.DrawSphere (rect.Center, rect.Width*scale);
	}
}

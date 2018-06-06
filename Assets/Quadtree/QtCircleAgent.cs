using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;

public class QtCircleAgent : QtAgent, IQuadtreeCircleAgent {

	public float radius = 2.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override CollisionResult IntersectWithBoundary (ConvexRect nodeBoundary){

		if (nodeBoundary.ContainPoint2D (new Vector2 (transform.position.x, transform.position.y))) {

			return CollisionResult.Fit;
		}

		return CollisionResult.None;

		/*TODO check if circle in boundary
		Vector2 pos = new Vector2 (transform.position.x, transform.position.y);

		if (nodeBoundary.ContainPoint2D (pos)) {

			//if circle fit
			bool tl = nodeBoundary.TLCorner - pos
		}
		*/
	}

	public override bool InQueryRange (IQuadtreeQuery query)
	{
		return query.CircleAgentInRange (this);
	}

	public virtual float Radius(){

		return radius;
	}
}

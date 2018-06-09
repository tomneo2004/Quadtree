using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;

public class QtCircleAgent : QtAgent, IQuadtreeCircleAgent {

	public float radius = 0.2f;

	ConvexCircle circle;

	void Awake(){

		circle = new ConvexCircle (new Vector2 (transform.position.x, transform.position.y), radius);
	}

	// Use this for initialization
	public override void Start () {

		base.AgentStart ();
	}
	
	// Update is called once per frame
	public override void Update () {

		UpdateCircle ();
		base.Update ();


	}

	void UpdateCircle(){
		//update circle properties
		circle.Radius = radius;
		circle.Center = new Vector2 (transform.position.x, transform.position.y);
		Debug.Log ("Circle center " + circle.Center);
	}

	protected override CollisionResult ContactWIthCircle (ConvexCircle circle)
	{
		return circle.CollideWithCircle (circle);
	}

	protected override CollisionResult ContactWithRectangle (ConvexRect rect)
	{
		return circle.CollideWithRect (rect);
	}

	public override CollisionResult IntersectWithBoundary (ConvexRect nodeBoundary){

		return circle.CollideWithRect (nodeBoundary);
	}

	public override void BeforeAddToQuadtreeNode (QuadtreeNode node){

		base.BeforeAddToQuadtreeNode (node);

		UpdateCircle ();
	}

	public override void AfterAddToQuadtreeNode (QuadtreeNode node){

		base.AfterAddToQuadtreeNode (node);
	}

	public override Vector2 GetCenter (){

		return circle.Center;
	}


	public override GameObject GetGameObject (){

		return gameObject;
	}

	public virtual float Radius(){

		return radius;
	}



	void OnDrawGizmos(){

		if (circle == null)
			return;

		Gizmos.color = Color.white;
		float cx = circle.Radius*Mathf.Cos(0);
		float cy = circle.Radius*Mathf.Sin(0);
		Vector2 cpos = circle.Center + new Vector2 (cx, cy);
		Vector2 cnewPos = cpos;
		Vector2 clastPos = cpos;
		for(float theta = 0.1f; theta<Mathf.PI*2.0f; theta+=0.1f){
			cx = circle.Radius*Mathf.Cos(theta);
			cy = circle.Radius*Mathf.Sin(theta);
			cnewPos = circle.Center+ new Vector2(cx,cy);
			Gizmos.DrawLine(cpos,cnewPos);
			cpos = cnewPos;
		}
		Gizmos.DrawLine(cpos,clastPos);
	}
}

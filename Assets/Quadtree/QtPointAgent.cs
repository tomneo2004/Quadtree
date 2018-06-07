using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	public class QtPointAgent : QtAgent, IQuadtreePointAgent {

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
		}

		public override bool InQueryRange (IQuadtreeQuery query)
		{
			return query.PointAgentInRange (this);
		}

		public virtual Vector2 Point2D (){

			return new Vector2 (transform.position.x, transform.position.y);

		}

		public override void BeforeAddToQuadtreeNode (QuadtreeNode node){

			base.BeforeAddToQuadtreeNode (node);
		}

		public override void AfterAddToQuadtreeNode (QuadtreeNode node){

			base.AfterAddToQuadtreeNode (node);
		}
	}
}


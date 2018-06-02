using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;

namespace NP.NPQuadtree{

	public class QtPointAgent : QtAgent, IQuadtreePointAgent {

		// Use this for initialization
		void Start () {

		}

		// Update is called once per frame
		void Update () {

		}

		public override IntersectionResult IntersectWithBoundary (NodeBound nodeBoundary){

			if (nodeBoundary.ContainPoint2D (new Vector2 (transform.position.x, transform.position.y))) {

				return IntersectionResult.Fit;
			}

			return IntersectionResult.None;
		}

		public override bool InQueryRange (IQuadtreeQuery query)
		{
			return query.PointAgentInRange (this);
		}

		public virtual Vector2 Point2D (){

			return new Vector2 (transform.position.x, transform.position.y);

		}
	}
}


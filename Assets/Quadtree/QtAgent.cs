using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;

namespace NP.NPQuadtree{

	public class QtAgent : MonoBehaviour, IQuadtreeAgent {

		// Use this for initialization
		void Start () {

		}

		// Update is called once per frame
		void Update () {

		}

		public virtual Vector2 Position2D(){

			return new Vector2 (transform.position.x, transform.position.y);
		}

		public virtual IntersectionResult IntersectWithBoundary (NodeBound nodeBoundary){

			return IntersectionResult.None;
		}

		public virtual bool InQueryRange (IQuadtreeQuery query){

			return false;
		}
	}
}


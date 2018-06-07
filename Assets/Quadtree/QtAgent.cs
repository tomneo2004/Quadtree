using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;

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

		public virtual CollisionResult IntersectWithBoundary (ConvexRect nodeBoundary){

			return CollisionResult.None;
		}

		public virtual bool InQueryRange (IQuadtreeQuery query){

			return false;
		}

		public virtual void BeforeAddToQuadtreeNode (QuadtreeNode node){
		}

		public virtual void AfterAddToQuadtreeNode (QuadtreeNode node){
		}
			
		public virtual Vector2 GetCenter (){

			return new Vector2(transform.position.x, transform.position.y);
		}


		public virtual GameObject GetGameObject (){

			return gameObject;
		}
	}
}


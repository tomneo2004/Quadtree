using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	public class QtAgent : MonoBehaviour, IQuadtreeAgent {

		public Vector2  lastPosition;
		Vector2 newPosition;

		QuadtreeNode currentNode;

		// Use this for initialization
		public virtual void Start () {

			AgentStart ();
		}

		// Update is called once per frame
		public virtual void Update () {

			AgentUpdate ();
			UpdateAgentNode ();
		}

		void UpdateAgentNode(){

			if (lastPosition != newPosition) {

				if (currentNode != null) {
					
					CollisionResult result = IntersectWithBoundary (currentNode.Boundary);

					switch (result) {

					case CollisionResult.Fit:
						if (!currentNode.IsLeaf) {//has child node

							//for all child nodes
							foreach (QuadtreeNode n in currentNode.AllNodes) {

								//see if agent in this child node
								if (n.Boundary.ContainPoint2D (this.GetCenter ())) {
								
									//if agent fit in this child node
									if (IntersectWithBoundary (n.Boundary) == CollisionResult.Fit) {
									
										currentNode.Remove (this);
										currentNode.Add (this);
										break;
									}
								}
							}

						}
						break;
					case CollisionResult.Overlap:
						/*
						if (currentNode.Parent != null) {

							currentNode.Remove (this);
							currentNode.Parent.Add (this);
						}
						*/

						//find parent until it contain this agent
						QuadtreeNode pNode = currentNode.Parent;
						while (pNode != null) {

							if (IntersectWithBoundary (pNode.Boundary) == CollisionResult.Fit) {
								break;
							}

							pNode = pNode.Parent;
						}

						currentNode.Remove (this);
						if (pNode == null)//root node
							currentNode.rootQuadtree ().Add (this);
						else
							pNode.Add (this);

						break;
					case CollisionResult.None:
						currentNode.Remove (this);
						currentNode.rootQuadtree ().Add (this);//add from root quadtree
						break;
					}


					/*
					currentNode.Remove (this);
					currentNode.rootQuadtree().Add (this);
					*/
				}


				lastPosition = newPosition;
			}
		}

		protected virtual void AgentStart(){

			newPosition = new Vector2 (transform.position.x, transform.position.y);
			lastPosition = newPosition;
		}

		protected virtual void AgentUpdate(){

			newPosition = new Vector2 (transform.position.x, transform.position.y);
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

			currentNode = node;
		}
			
		public virtual Vector2 GetCenter (){

			return new Vector2(transform.position.x, transform.position.y);
		}


		public virtual GameObject GetGameObject (){

			return gameObject;
		}
	}
}


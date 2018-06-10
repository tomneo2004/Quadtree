using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	abstract public class BaseAgent : MonoBehaviour, IQuadtreeAgent{

		public virtual void Start(){
		}

		public virtual void Update(){
		}

		public abstract ConvexShape GetShape ();
		public abstract void BeforeAddToQuadtreeNode (QuadtreeNode node);
		public abstract void AfterAddToQuadtreeNode (QuadtreeNode node);
		public abstract Vector2 GetCenter ();
		public abstract GameObject GetGameObject ();
	}

	abstract public class QtAgent : BaseAgent {

		public Vector2  lastPosition;
		Vector2 newPosition;

		QuadtreeNode currentNode;
		public QuadtreeNode CurrentNode{ get{ return currentNode;}}

		// subclass not allow to use this method 
		sealed public override void Start () {

			AgentStart ();
		}

		// subclass not allow to use this method 
		sealed public override void Update () {

			BeforeAgentUpdate ();
			AgentUpdate ();
			UpdateAgentInQuadtree ();
		}

		/**
		 * Method to update agent in quadtree especially
		 * when agent moving around
		 **/
		void UpdateAgentInQuadtree(){

			if (lastPosition != newPosition) {

				if (currentNode != null) {

					//check collision between agent and node boundary
					CollisionResult result = this.GetShape().IntersectWithShape (currentNode.Boundary);

					switch (result) {

					case CollisionResult.Fit:
						if (!currentNode.IsLeaf) {//has child node

							//for all child nodes
							foreach (QuadtreeNode n in currentNode.AllNodes) {

								//see if agent in this child node
								if (n.Boundary.ContainPoint2D (this.GetCenter ())) {
								
									//if agent fit in this child node
									if (this.GetShape().IntersectWithShape (n.Boundary) == CollisionResult.Fit) {
									
										currentNode.Remove (this);
										currentNode.Add (this);
										break;
									}
								}
							}

						}
						break;
					case CollisionResult.Overlap:
						//find parent until it contain this agent
						QuadtreeNode pNode = currentNode.Parent;
						while (pNode != null) {

							if (this.GetShape().IntersectWithShape (pNode.Boundary) == CollisionResult.Fit) {
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
						
				}


				lastPosition = newPosition;
			}
		}

		/**
		 * Subclass can override to provide more functionality
		 * 
		 * Subclass muse call base
		 * 
		 * Replace MonoBehaviour's Start()
		 **/
		protected virtual void AgentStart(){

			newPosition = new Vector2 (transform.position.x, transform.position.y);
			lastPosition = newPosition;
		}

		/**
		 * Subclass can override to provide more functionality
		 * 
		 * Subclass muse call base
		 * 
		 * Replace MonoBehaviour's Update()
		 **/
		protected virtual void AgentUpdate(){

			newPosition = new Vector2 (transform.position.x, transform.position.y);
		}

		/**
		 * Subclass can override to provide more functionality
		 * 
		 * Subclass muse call base
		 * 
		 * Do anything necessary before agent start to update
		 **/
		protected virtual void BeforeAgentUpdate (){
		}

		public abstract override ConvexShape GetShape ();

		public override void BeforeAddToQuadtreeNode (QuadtreeNode node){
		
		}

		public override void AfterAddToQuadtreeNode (QuadtreeNode node){

			currentNode = node;
		}


		public abstract override GameObject GetGameObject ();
	}
}


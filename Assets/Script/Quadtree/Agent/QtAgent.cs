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

		/**
		 * Check if agent intersect with shape
		 * 
		 * Agent will automatically detect which shape is used to calculate collision
		 **/
		public virtual CollisionResult IntersectWithShape (ConvexShape shape){

			switch (shape.ShapeId) {
			case ConvexShapeID.Rectangle:
				ConvexRect rectShape = shape as ConvexRect;
				if (rectShape == null) {
					#if DEBUG
					Debug.LogError("Unable to down cast ConvexShape to ConvexRect");
					#endif
				}
				return ContactWithRectangle (rectShape);
			case ConvexShapeID.Circle:
				ConvexCircle circleShape = shape as ConvexCircle;
				if (circleShape == null) {
					#if DEBUG
					Debug.LogError("Unable to down cast ConvexShape to ConvexCircle");
					#endif
				}
				return ContactWIthCircle (circleShape);
			case ConvexShapeID.Unknow:
				#if DEBUG
				Debug.LogError("Unknow convex shape");
				#endif
				break;
			}

			return CollisionResult.None;
		}

		/**
		 * Subclass must override
		 **/
		protected abstract CollisionResult ContactWithRectangle (ConvexRect rect);

		/**
		 * Subclass must override
		 **/
		protected abstract CollisionResult ContactWIthCircle (ConvexCircle circle);

		public abstract void BeforeAddToQuadtreeNode (QuadtreeNode node);
		public abstract void AfterAddToQuadtreeNode (QuadtreeNode node);
		public abstract Vector2 GetCenter ();
		public abstract GameObject GetGameObject ();
	}

	abstract public class QtAgent : BaseAgent {

		public Vector2  lastPosition;
		Vector2 newPosition;

		QuadtreeNode currentNode;

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
					
					CollisionResult result = IntersectWithShape (currentNode.Boundary);

					switch (result) {

					case CollisionResult.Fit:
						if (!currentNode.IsLeaf) {//has child node

							//for all child nodes
							foreach (QuadtreeNode n in currentNode.AllNodes) {

								//see if agent in this child node
								if (n.Boundary.ContainPoint2D (this.GetCenter ())) {
								
									//if agent fit in this child node
									if (IntersectWithShape (n.Boundary) == CollisionResult.Fit) {
									
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

							if (IntersectWithShape (pNode.Boundary) == CollisionResult.Fit) {
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
			
		protected abstract override CollisionResult ContactWithRectangle (ConvexRect rect);
		protected abstract override CollisionResult ContactWIthCircle (ConvexCircle circle);

		public override void BeforeAddToQuadtreeNode (QuadtreeNode node){
		
		}

		public override void AfterAddToQuadtreeNode (QuadtreeNode node){

			currentNode = node;
		}


		public override GameObject GetGameObject (){

			return gameObject;
		}
	}
}


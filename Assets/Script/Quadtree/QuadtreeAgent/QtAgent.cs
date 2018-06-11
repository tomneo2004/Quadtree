using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	abstract public class BaseAgent : MonoBehaviour, IQuadtreeAgent{

		public virtual void Awake(){
		}

		public virtual void Start(){
		}

		public virtual void Update(){
		}

		public abstract ConvexShape GetShape ();
		public abstract void BeforeAddToQuadtreeNode ();
		public abstract void AfterAddToQuadtreeNode (QuadtreeNode node);
		public abstract Vector2 GetCenter ();
	}

	abstract public class QtAgent : BaseAgent {

		/**
		 * Last position of agent
		 **/
		public Vector2  lastPosition;

		/**
		 * New position of agent
		 * 
		 * THis must be update every time agent position is changed
		 **/
		Vector2 newPosition;

		/**
		 * Current quadtree node this agnet is in
		 **/
		QuadtreeNode currentNode;

		/**
		 * Get quadtree node this agent is in
		 **/
		public QuadtreeNode CurrentNode{ get{ return currentNode;}}

		/**
		 * Reference to Transform
		 **/
		protected Transform agentTransform;

		/**
		 * Reference to Rigidbody2D
		 **/
		protected Rigidbody2D agentRigidbody2D;

		/**
		 * Get agent's Rigidbody2D if there is one otherwise return null
		 **/
		public Rigidbody2D agRigidbody2D{ get{ return agentRigidbody2D;}}

		/**
		 * Reference to GameObject
		 **/
		protected GameObject agentGameObject;

		/**
		 * Get agent's GameObject if there is one otherwise return null
		 **/
		public GameObject agGameObject{ get{ return agentGameObject;}}


		sealed public override void Awake () {

			agentTransform = this.transform;
			agentRigidbody2D = GetComponent<Rigidbody2D> ();
			agentGameObject = this.gameObject;

			AgentAwake ();
		}

		// subclass not allow to use this method 
		sealed public override void Start () {

			newPosition = new Vector2 (agentTransform.position.x, agentTransform.position.y);
			lastPosition = newPosition;

			AgentStart ();
		}

		// subclass not allow to use this method 
		sealed public override void Update () {

			BeforeAgentUpdate ();

			//update new position
			newPosition = new Vector2 (agentTransform.position.x, agentTransform.position.y);

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
							IEnumerator er = currentNode.AllNodes.GetEnumerator ();
							while (er.MoveNext ()) {

								//if agent fit in this child
								if (this.GetShape ().IntersectWithShape ((er.Current as QuadtreeNode).Boundary) == CollisionResult.Fit) {

									//move to child node
									currentNode.Remove (this);
									(er.Current as QuadtreeNode).Add (this);
								}
							}

						}
						break;
					case CollisionResult.Overlap:
						//find parent until agent complete fit in
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
		 * Replace MonoBehaviour's Awake()
		 **/
		protected virtual void AgentAwake(){
		}

		/**
		 * Subclass can override to provide more functionality
		 * 
		 * Subclass muse call base
		 * 
		 * Replace MonoBehaviour's Start()
		 **/
		protected virtual void AgentStart(){
		}

		/**
		 * Subclass can override to provide more functionality
		 * 
		 * Subclass muse call base
		 * 
		 * Replace MonoBehaviour's Update()
		 **/
		protected virtual void AgentUpdate(){

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

		public override void BeforeAddToQuadtreeNode (){
		
		}

		public override void AfterAddToQuadtreeNode (QuadtreeNode node){

			//update node
			currentNode = node;
		}

		public abstract override Vector2 GetCenter ();
	}
}


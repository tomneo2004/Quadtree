using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NP.Convex.Collision;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	public interface IQuadtreeAgent{

		/**
		 * Return position in 2d in world space
		 **/
		Vector2 Position2D ();

		/**
		 * Intersect with quadtree node's boundary
		 * 
		 * Return CollisionResult
		 * 
		 * Param ConvexRect is in world space which is topleft corner as origin
		 **/
		CollisionResult IntersectWithBoundary (ConvexRect nodeBoundary);

		/**
		 * Return true if agent in query range
		 **/
		bool InQueryRange (IQuadtreeQuery query);

		/**
		 * Nofity when agent is about to be add to quadtree node
		 * 
		 * Param node is the quadtree node you call "Add" on
		 **/
		void BeforeAddToQuadtreeNode (QuadtreeNode node);

		/**
		 * Nofity when agent is added to quadtree node
		 * 
		 * Param node is the quadtree node this agent was added to
		 **/
		void AfterAddToQuadtreeNode (QuadtreeNode node);

	}

	public interface IQuadtreePointAgent : IQuadtreeAgent{

		/**
		 * Return point of this agent
		 **/
		Vector2 Point2D();
	}

	public interface IQuadtreeCircleAgent : IQuadtreeAgent{

		/**
		 * Return radius of circle of this agent
		 **/
		float Radius ();
	}

	public interface IQuadtreeQuery{

		/**
		 * Intersect with quadtree's boundary
		 * 
		 * Return true if query intersect with boundary
		 * 
		 * Param ConvexRect is in world space which is topleft corner as origin
		 **/
		bool IntersectWithBoundary (ConvexRect quadtreeBoundary);

		/**
		 * Intersect with certain element
		 * 
		 * Return true if element(agent) is in query range
		 **/
		bool IntersectWithElement (IQuadtreeAgent element);

		/**
		 * Return true if point type of agent is in range
		 **/
		bool PointAgentInRange (IQuadtreePointAgent pointAgent);

		bool CircleAgentInRange (IQuadtreeCircleAgent circleAgent);
	}

	public class QuadtreeNode{

		/**
		 * How many element can be stored in this quadtree node
		 * 
		 * Inclusive
		 * 
		 * Default is 4
		 **/
		public static int elementCapacity = 4;

		/**
		 * How deep is this QuadtreeNode
		 **/
		int depthIndex = 0;

		/**
		 * Return depth index of QuadtreeNode
		 **/
		public int DepthIndex{ get{ return depthIndex;}}

		/**
		 * Return depth of QuadtreeNode
		 **/
		public int Depth{ get{ return (depthIndex + 1);}}

		/**
		 * Is this a leaf otherwise it is node
		 * 
		 * Leaf does not have child node
		 **/
		public bool IsLeaf{ get{ return (nodes == null);}}

		/**
		 * Is this a root quadtree node
		 **/
		public bool IsRoot{ get{ return (parentNode == null);}}

		/**
		 * Where element to be stored
		 **/
		List<IQuadtreeAgent> elements;

		/**
		 * Store all elements that overlap more than 1 child node
		 **/
		List<IQuadtreeAgent> overlapElements;

		/**
		 * Parent quadtree node
		 **/
		QuadtreeNode parentNode = null;

		/**
		 * Get parent of this quadtree
		 * 
		 * Return null if no parent
		 **/
		public QuadtreeNode Parent{ get{ return parentNode;}}

		/**
		 * Child nodes under this quadtree node
		 * 
		 * Index 0 = TopLeft
		 * Index 1 = TopRight
		 * Index 2 = BottomRight
		 * Index 3 = BottomLeft
		 **/
		QuadtreeNode[] nodes;

		/**
		 * All elements in list should be add next frame not immediately
		 **/
		List<IQuadtreeAgent> elementsNextFrame;

		/**
		 * Boundary TopLeft is origin point
		 **/
		ConvexRect boundary;

		/**
		 * Get quadtree's boundary
		 * 
		 * The boundary is in world space and TopLeft corner as origin
		 **/
		public ConvexRect Boundary{ 
			get{ 

				return boundary;
			}
		}
			

		/**
		 * Get quadtree's center
		 **/
		public Vector2 Center{ get{ return boundary.center;}}

		/**
		 * Total nodes under root QuadtreeNode include child leaf
		 **/
		public int TotalElementCount{ get{ return rootQuadtree ().TotalElements ();}}

		/**
		 * Total nodes under this QuadtreeNode include child leaf
		 **/
		public int ElementCount{ get{ return TotalElements ();}}


		/**
		 * Color for drawing quadtree boundary
		 **/
		public Color debugDrawColor = Color.white;

		/**
		 * Constructor
		 **/
		QuadtreeNode(QuadtreeNode parent){

			parentNode = parent;
			elements = new List<IQuadtreeAgent> (elementCapacity);
			overlapElements = new List<IQuadtreeAgent> ();
			elementsNextFrame = new List<IQuadtreeAgent> ();

			//set depth to 0 if this is root
			if (parent == null)
				depthIndex = 0;
			else
				depthIndex = parent.depthIndex + 1;
		}

		/**
		 * Deconstructor
		 **/
		~QuadtreeNode(){

			overlapElements.Clear ();
			overlapElements = null;
			elements.Clear ();
			elements = null;

			nodes = null;
		}

		/**
		 * Draw Quadtree boundary
		 **/
		public void DebugDraw(float z){

			z = z + depthIndex;


			//boundary for drawing
			float offset = 0.0f;
			ConvexRect dBoundary = boundary;
			if (parentNode != null) {
				
				dBoundary.x += offset;
				dBoundary.y += offset;
				dBoundary.width -= offset*2;
				dBoundary.height -= offset*2;
			}
				
			//Draw child node
			if (nodes != null) {

				foreach (QuadtreeNode n in nodes) {

					n.DebugDraw (z);
				}
			}


			//Gizmos.color = debugDrawColor;
			Handles.color = debugDrawColor;
			Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

			//LT->RT
			Handles.DrawLine (new Vector3 (dBoundary.x, dBoundary.y, z),
				new Vector3 (dBoundary.x + dBoundary.width, dBoundary.y, z));

			//RT->RB
			Handles.DrawLine (new Vector3 (dBoundary.x + dBoundary.width, dBoundary.y, z),
				new Vector3 (dBoundary.x + dBoundary.width, dBoundary.y - dBoundary.height, z));
			
			//RB->LB
			Handles.DrawLine (new Vector3 (dBoundary.x + dBoundary.width, dBoundary.y - dBoundary.height, z),
				new Vector3 (dBoundary.x, dBoundary.y - dBoundary.height, z));

			//LB->LT
			Handles.DrawLine (new Vector3 (dBoundary.x, dBoundary.y - dBoundary.height, z),
				new Vector3 (dBoundary.x, dBoundary.y, z));
			

		}

		/**
		 * Create a root element
		 * 
		 * Given rect of boundary must have topleft corner as origin
		 **/
		public static QuadtreeNode createRootQuadtree(ConvexRect nodeBoundry){
		
			QuadtreeNode rootQuadtree = new QuadtreeNode (null);

			rootQuadtree.SetBoundary (nodeBoundry);

			return rootQuadtree;
		}

		/**
		 * Set boundary 
		 * 
		 * Center will automatically be calculated
		 **/
		private void SetBoundary(ConvexRect nodeBoundary){

			if (nodeBoundary == ConvexRect.zero)
				return;

			boundary = nodeBoundary;
		}

		/**
		 * Return number of element and overlap element under quadtree node include child node
		 **/
		private int TotalElements(bool includeOverlap = true){
				
			int numElement= elements.Count;

			if (includeOverlap)
				numElement += overlapElements.Count; 

			if (nodes == null)
				return numElement;

			foreach (QuadtreeNode n in nodes)
				numElement += n.TotalElements ();

			return numElement;
		}

		/**
		 * Return index of child node under this node
		 * 
		 * This is noly check for element's positon not boundary
		 * 
		 * Return -1 if not found
		 **/
		private int IndexOfNode(IQuadtreeAgent element){

			if ((element == null) || (nodes == null))
				return -1;

			for (int i = 0; i < nodes.Length; i++) {

				if (nodes [i].boundary.ContainPoint2D (element.Position2D ()))
					return i;
			}

			#if DEBUG
			Debug.LogError("Element does not located in this node fix me");
			#endif

			return -1;
		}

		/**
		 * Find root quadtree 
		 **/
		public QuadtreeNode rootQuadtree(){

			QuadtreeNode q = this;

			while (q.parentNode != null)
				q = q.parentNode;

			#if DEBUG
			if(q!=null)
				q.debugDrawColor = Color.green;
			#endif

			return q;
		}

		/**
		 * Must be called every frame
		 **/
		public void UpdateQuadtree(){

			if (elementsNextFrame != null && elementsNextFrame.Count > 0) {

				foreach (IQuadtreeAgent element in elementsNextFrame) {
				
					Add (element);
				}

				elementsNextFrame.Clear ();
			}
		}

		/**
		 * Add new element to quadtree at next frame
		 **/
		public void AddNextFrame(IQuadtreeAgent newElement){

			elementsNextFrame.Add (newElement);
		}
			
		/**
		 * Add element to quadtree immediately
		 **/
		public bool Add(IQuadtreeAgent newElement){

			if (newElement == null) {
			
				#if DEBUG
				Debug.LogError("Node does not implement IQuadtree");
				#endif
				return false;
			}

			//notify element is about to add it to certain quadtree node
			newElement.BeforeAddToQuadtreeNode (this);

			//If we have child node
			if (nodes != null) {

				int nodeIndex = IndexOfNode (newElement);

				//can't find index of node
				if (nodeIndex < 0)
					return false;

				//check if element's boundary fit in the 
				//child node otherwise element is overlapping multiple node
				CollisionResult iResult = newElement.IntersectWithBoundary(nodes[nodeIndex].boundary);

				switch (iResult) {
				case CollisionResult.Fit:
					//Element's boudnary fit in child node's boundary, add it to child node
					return nodes [nodeIndex].Add (newElement);

				case CollisionResult.Overlap:
					//Element's boudnary overlap multiple child node, add it to this node
					overlapElements.Add (newElement);
					return true;

				case CollisionResult.None:
					//Element's boundary not intersect with child node
					//Something went wrong
					#if DEBUG
					Debug.LogError("Unable to add element, element position within node but not intersect with any child node");
					#endif
					return false;
				}

				return false;
			}
				

			//Add element to this node
			elements.Add (newElement);

			//notify element it has been added to this quadtree node
			newElement.AfterAddToQuadtreeNode (this);

			//Split if needed
			if (elements.Count > elementCapacity) {

				Split ();

				//Distribute elements to corespond child node
				foreach (IQuadtreeAgent e in elements)
					Add (e);

				//clear elements
				elements.Clear();
			}

			return true;
		}
			

		/**
		 * Split quadtree node into 4
		 **/
		private void Split(){

			#if DEBUG
			Debug.Log("Split quadtree");
			#endif

			#if DEBUG
			if(nodes != null)
				Debug.LogWarning("Child node already exist before split");
			#endif

			ConvexRect newBoundary;
			float width = boundary.width / 2.0f;
			float height = boundary.height / 2.0f;

			nodes = new QuadtreeNode[4];

			//TopLeft(NorthWest)
			newBoundary = new ConvexRect(boundary.x, boundary.y, width, height);
			nodes[0] = new QuadtreeNode (this);
			nodes[0].SetBoundary (newBoundary);

			//TopRight(NorthEast)
			newBoundary = new ConvexRect(boundary.x+width, boundary.y, width, height);
			nodes[1] = new QuadtreeNode (this);
			nodes[1].SetBoundary (newBoundary);

			//BottomRight(SouthEast)
			newBoundary = new ConvexRect(boundary.x+width, boundary.y - height, width, height);
			nodes[2] = new QuadtreeNode (this);
			nodes[2].SetBoundary (newBoundary);

			//BottomLeft(SouthWest)
			newBoundary = new ConvexRect(boundary.x, boundary.y - height, width, height);
			nodes[3] = new QuadtreeNode (this);
			nodes[3].SetBoundary (newBoundary);


		}


		/**
		 * Find node that contain element
		 * 
		 * Return QuadtreeNode that has element
		 **/
		public QuadtreeNode FindNode(IQuadtreeAgent element){

			if (element == null)
				return null;

			if (!boundary.ContainPoint2D (element.Position2D ())) {

				//element never added to quadtree
				#if DEBUG
				Debug.LogWarning("Unable to find element, element never exist under this quadtree node");
				#endif

				return null;
			}


			//Search child node that contain element position
			if (nodes != null) {

				int nodeIndex = IndexOfNode (element);

				//also check element boundary within node boundary
				if (nodeIndex >= 0 
					&& (element.IntersectWithBoundary(nodes[nodeIndex].boundary) == CollisionResult.Fit))
					return nodes [nodeIndex].FindNode(element);
			}

			if (elements.Contains (element) || overlapElements.Contains (element))
				return this;

			//Something went wrong element within boundary but not in list
			return null;

		}

		/**
		 * Find element from root quadtree
		 * 
		 * Return QuadtreeNode that has element
		 **/
		public QuadtreeNode FindNodeFromRoot(IQuadtreeAgent element){

			if (element == null)
				return null;

			//If this quadtree is not root
			if (parentNode != null)
				return rootQuadtree ().FindNode (element);

			return FindNode (element);
		}

		//TODO CHECK ELEMENT BOUNDARY
		/**
		 * Find possible elements that might contact with given element
		 * under this node
		 * 
		 * Param includeSelf is true given element might be in the result, default is false
		 **/
		public List<IQuadtreeAgent> FindElements(IQuadtreeAgent element, bool includeSelf = false){

			List<IQuadtreeAgent> result = new List<IQuadtreeAgent> ();

			//If this node's boundary is not contacted with element boundary
			if (element.IntersectWithBoundary (boundary) == CollisionResult.None)
				return result;

			//Search child nodes
			if (nodes != null) {

				//for each child node
				foreach (QuadtreeNode n in nodes) {

					CollisionResult r = element.IntersectWithBoundary (n.boundary);

					if (r == CollisionResult.Overlap) {//could have more than one child node

						result.AddRange (n.FindElements (element));

					} else if (r == CollisionResult.Fit) {//can only fit into one child node

						result.AddRange (n.FindElements (element));
						break;
					}
				}

				/*
				int nodeIndex = IndexOfNode (element);

				if (nodeIndex >= 0) {

					QuadtreeNode node = nodes [nodeIndex];

					CollisionResult r = element.IntersectWithBoundary (node.boundary);

					//element boundary fit in this child boundary otherwise element overlap multiple nodes
					if (r == CollisionResult.Fit) {
					
						result.AddRange (node.FindElements (element));

					} else if(r == CollisionResult.Overlap) {

						foreach (QuadtreeNode n in nodes) {
						
							//if element intersect with child node
							if (element.IntersectWithBoundary (n.boundary) != CollisionResult.None)
								result.AddRange (n.FindElements (element));
								
						}
					}
				}
				*/
			}

			result.AddRange (elements);
			result.AddRange (overlapElements);

			if (!includeSelf) {
				result.Remove (element);
			}

			return result;
		}

		/**
		 * Find possible elements that might contact with given element
		 * from root node
		 * 
		 * Param includeSelf is true given element might be in the result, default is false
		 **/
		public List<IQuadtreeAgent> FindElementsFromRoot(IQuadtreeAgent element, bool includeSelf = false){

			//If not root then find root and find elements
			if (parentNode != null)
				return rootQuadtree ().FindElements (element, includeSelf);

			return FindElements (element, includeSelf);
		}


		/**
		 * Query nodes with in query shape
		 * 
		 **/
		public List<IQuadtreeAgent> QueryElements(IQuadtreeQuery query){

			if (query == null) {

				#if DEBUG
				Debug.LogError("Can't query nodes, given query is null");
				#endif
				return null;
			}

			//Return null if query is not intersect with quadtree boundary
			if (!query.IntersectWithBoundary (boundary))
				return null;

			//Return null if no element
			if (elements.Count <= 0)
				return null;
					
			//Check if any of element within query shape
			List<IQuadtreeAgent> results = new List<IQuadtreeAgent>();
			foreach (IQuadtreeAgent e in elements) {

				//Add element to result
				if (query.IntersectWithElement(e))
					results.Add (e);
			}
			foreach (IQuadtreeAgent overlapE in overlapElements) {

				//Add element to result
				if (query.IntersectWithElement(overlapE))
					results.Add (overlapE);
			}

			//If no child leaf
			if (nodes == null)
				return results;

			//Go through each child node
			List<IQuadtreeAgent> rElements;
			foreach(QuadtreeNode n in nodes){

				rElements = n.QueryElements (query);

				if (rElements != null) {

					foreach (IQuadtreeAgent elm in rElements)
						results.Add (elm);
				}
			}
				
			return results;
		}

		/**
		 * Query from root quadtree and nodes with in query shape
		 * 
		 **/
		public List<IQuadtreeAgent> QueryNodesFromRoot(IQuadtreeQuery query){
		
			//If not root then find root and query
			if (parentNode != null)
				return rootQuadtree ().QueryElements (query);

			return QueryElements (query);
		}

		public void LevelDesc(){

			Debug.Log ("level: " + Depth
				+ " is leaf: " + IsLeaf
				+ " elements: " + elements.Count
				+ " overlap elements: " + overlapElements.Count 
				+ " Center: " + boundary.center);


			if (nodes != null) {

				foreach (QuadtreeNode n in nodes)
					n.LevelDesc ();
			}
		}
	}
}
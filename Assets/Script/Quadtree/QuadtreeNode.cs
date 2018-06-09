using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NP.Convex.Collision;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	public interface IQuadtreeAgent{

		CollisionResult IntersectWithShape (ConvexShape shape);

		/**
		 * Intersect with quadtree node's boundary
		 * 
		 * Return CollisionResult
		 * 
		 * Param ConvexRect is in world space which is topleft corner as origin
		 **/
		CollisionResult IntersectWithBoundary (ConvexRect nodeBoundary);

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

		/**
		 * Return center point of agent
		 **/
		Vector2 GetCenter ();

		/**
		 * Return gameobject of agent
		 **/
		GameObject GetGameObject ();

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
		 * Flag to tell quadtree to orgnize tree next frame
		 **/
		bool organizeTree = false;

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
		 * Get all nodes under this quadtree nodex
		 **/
		public QuadtreeNode[] AllNodes{ get{ return nodes;}}

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

			InitNode (parent);
		}

		QuadtreeNode(QuadtreeNode parent, ConvexRect nodeBoundary){

			InitNode (parent);
			SetBoundary (nodeBoundary);
		}

		QuadtreeNode(QuadtreeNode parent, float x, float y , float width, float height){

			InitNode (parent);
			ConvexRect nodeBoundary = new ConvexRect (x, y, width, height);
			SetBoundary (nodeBoundary);
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
		 * Initilize quadtree node
		 **/
		void InitNode(QuadtreeNode parent){

			elements = new List<IQuadtreeAgent> (elementCapacity);
			overlapElements = new List<IQuadtreeAgent> ();
			elementsNextFrame = new List<IQuadtreeAgent> ();

			parentNode = parent;

			//set depth to 0 if this is root
			if (parent == null)
				depthIndex = 0;
			else
				depthIndex = parent.depthIndex + 1;
		}

		/**
		 * Draw Quadtree boundary
		 **/
		public void DebugDraw(float z, bool child = true){

			z = z + depthIndex;

			if (child) {
				//Draw child node
				if (nodes != null) {

					foreach (QuadtreeNode n in nodes) {

						n.DebugDraw (z, child);
					}
				}
			}


			//boundary for drawing
			float offset = 0.0f;
			ConvexRect dBoundary = boundary;
			if (parentNode != null) {

				dBoundary.x += offset;
				dBoundary.y += offset;
				dBoundary.width -= offset*2;
				dBoundary.height -= offset*2;
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
		 * Set boundary of this node
		 **/
		private void SetBoundary(ConvexRect nodeBoundary){

			if (nodeBoundary == ConvexRect.zero)
				return;

			boundary = nodeBoundary;
		}

		/**
		 * Return number of element and overlap element under quadtree node include child node
		 **/
		private int TotalElements(bool includeOverlap = true, bool deepNode = true){
				
			int numElement= elements.Count;

			if (includeOverlap)
				numElement += overlapElements.Count; 

			//check child nodes
			if (deepNode) {

				if (nodes == null)
					return numElement;

				foreach (QuadtreeNode n in nodes)
					numElement += n.TotalElements ();
			}


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

				if (nodes [i].boundary.ContainPoint2D (element.GetCenter()))
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

			//perform add elements
			if (elementsNextFrame != null && elementsNextFrame.Count > 0) {

				foreach (IQuadtreeAgent element in elementsNextFrame) {
				
					Add (element);
				}

				elementsNextFrame.Clear ();
			}

			//orgnize tree
			//ReOrganize();

			if (organizeTree) {

				ReOrganize ();

				organizeTree = false;
			}

		}

		/**
		 * Add new element to quadtree at next frame
		 * This will start from root quadtree node
		 **/
		public void AddNextFrame(IQuadtreeAgent newElement){

			elementsNextFrame.Add (newElement);
		}
			
		/**
		 * Add element to quadtree immediately under this
		 * quadtree node or deeper child node
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

			Debug.Log("Level "+ Depth);

			//If we have child node
			if (nodes != null) {

				int nodeIndex = IndexOfNode (newElement);

				//can't find index of node
				if (nodeIndex < 0) {

					#if DEBUG
					Debug.LogError("No child node found"+" node boundary center "+this.boundary.center
						+ "level "+Depth 
						+" xMin " + boundary.xMin + " xMax "+boundary.xMax
						+" yMin " + boundary.yMin + " yMax "+boundary.yMax
						+" element center "+newElement.GetCenter()
						+ " element last center "+ newElement.GetGameObject().GetComponent<QtAgent>().lastPosition);
					#endif
					return false;
				}

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
					//notify element it has been added to this quadtree node
					newElement.AfterAddToQuadtreeNode (this);
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

			/*
			CollisionResult r = newElement.IntersectWithBoundary (this.boundary);
			if (r == CollisionResult.Fit) {
				//Add element to this node
				elements.Add (newElement);
			}
			else {

				if (parentNode != null)
					parentNode.Add (newElement);
				else
					overlapElements.Add (newElement);
			}
			*/	
			elements.Add (newElement);

			//notify element it has been added to this quadtree node
			newElement.AfterAddToQuadtreeNode (this);

			//Split if needed
			if (elements.Count > elementCapacity) {

				Split ();

				//Distribute elements to corespond child node
				foreach (IQuadtreeAgent e in elements) {
				
					//if elemnt out of node boundary
					if (!Add (e)) {
						#if DEBUG
						Debug.LogWarning("Distribute element to child node fail, elemnt is out of node boundary." +
							"We need to add from root");
						#endif

						//add element from root
						rootQuadtree ().Add (e);
					}
						
				}

				//clear elements
				elements.Clear();
			}

			return true;
		}

		/**
		 * Remove element under this quadtree node
		 **/
		public bool Remove(IQuadtreeAgent element){

			bool removed = false;

			//If element in this node then remove it
			if (elements.Contains (element) || overlapElements.Contains (element)) {
			
				elements.Remove (element);
				overlapElements.Remove (element);

				removed = true;
			}

			if (!removed) {

				//search child
				if(nodes != null){

					int nodeIndex = IndexOfNode (element);

					if (nodeIndex >= 0) {

						CollisionResult r = element.IntersectWithBoundary (nodes [nodeIndex].boundary);

						switch (r) {

						case CollisionResult.Fit://can only fit in one child node
							removed = nodes [nodeIndex].Remove (element);
							break;
						}
					}
				}
			}

			if (removed == true)
				organizeTree = true;

			#if DEBUG
			if(!removed)
				Debug.LogError("Can not remove element");
			#endif

			return removed;
		}
			

		/**
		 * Split quadtree node into 4
		 **/
		private void Split(){

			#if DEBUG
			Debug.Log("Split quadtree");
			#endif

			#if DEBUG
			if(nodes != null){
				Debug.LogWarning("Child node already exist before split");
				return;
			}
			#endif

			ConvexRect newBoundary;
			float width = boundary.width / 2.0f;
			float height = boundary.height / 2.0f;

			nodes = new QuadtreeNode[4];

			//TopLeft(NorthWest)
			newBoundary = new ConvexRect(boundary.x, boundary.y, width, height);
			nodes[0] = new QuadtreeNode (this,newBoundary);


			//TopRight(NorthEast)
			newBoundary = new ConvexRect(boundary.x+width, boundary.y, width, height);
			nodes[1] = new QuadtreeNode (this, newBoundary);


			//BottomRight(SouthEast)
			newBoundary = new ConvexRect(boundary.x+width, boundary.y - height, width, height);
			nodes[2] = new QuadtreeNode (this, newBoundary);


			//BottomLeft(SouthWest)
			newBoundary = new ConvexRect(boundary.x, boundary.y - height, width, height);
			nodes[3] = new QuadtreeNode (this, newBoundary);



		}

		/**
		 * Re-organize tree
		 * 
		 * Child nodes will be removed if 4 child node is leaf and contain no objects
		 **/
		void ReOrganize(){

			if (nodes != null) {

				bool cleanChildeNodes = true;

				foreach (QuadtreeNode n in nodes) {

					//tell child node to re-organize
					n.ReOrganize ();

					//if child has 4 child nodes
					if (!n.IsLeaf) {

						cleanChildeNodes = false;
					}

					//if child has elements
					if (n.TotalElements (true, false) != 0) {

						cleanChildeNodes = false;
					}
						
				}


				if (cleanChildeNodes)
					nodes = null;
			}
		}


		/**
		 * Find node that contain element
		 * 
		 * Return QuadtreeNode that has element
		 **/
		public QuadtreeNode FindNode(IQuadtreeAgent element){

			if (element == null)
				return null;

			if (!boundary.ContainPoint2D (element.GetCenter())) {

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
			
		/**
		 * Delegate that used to compare agent need to be added for FindElements
		 * 
		 * return return false to remove agent
		 **/
		public delegate bool OnCompare(IQuadtreeAgent agent);
		/**
		 * Find possible elements that might contact with given element
		 * under this node
		 * 
		 * Param includeSelf is true given element might be in the result, default is false
		 **/
		public List<IQuadtreeAgent> FindElements(IQuadtreeAgent element, bool includeSelf = false, OnCompare compare = null){

			#if DEBUG
			if(element.IntersectWithBoundary(this.boundary) == CollisionResult.Overlap)
				Debug.LogWarning("Element overlap this node boundary result might not be corrent, recommend to use root node");
			#endif

			List<IQuadtreeAgent> result = new List<IQuadtreeAgent> ();

			//If this node's boundary is not contacted with element boundary
			if (element.IntersectWithBoundary (boundary) == CollisionResult.None)
				return result;

			//Search child nodes
			if (nodes != null) {

				//for each child node
				foreach (QuadtreeNode n in nodes) {

					CollisionResult r = element.IntersectWithBoundary (n.boundary);

					switch (r) {
					case CollisionResult.Overlap://could have more than one child node covered
						result.AddRange (n.FindElements (element, includeSelf, compare));
						break;
					case CollisionResult.Fit://can only fit into one child node
						//search deep down this child node
						result.AddRange (n.FindElements (element, includeSelf, compare));
						break;
					}

					//no need to search other child node
					if (r == CollisionResult.Fit)
						break;
				}
					
			}

			result.AddRange (elements);
			result.AddRange (overlapElements);

			if (!includeSelf) {
				result.Remove (element);
			}

			if (compare != null) {

				foreach (IQuadtreeAgent e in elements) {

					if (!compare (e))
						result.Remove (e);
				}

				foreach (IQuadtreeAgent e in overlapElements) {

					if (!compare (e))
						result.Remove (e);
				}
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
				+ " is root " + IsRoot
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
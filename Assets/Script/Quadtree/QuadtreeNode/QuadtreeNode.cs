using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NP.Convex.Collision;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	/**
	 * Common interface
	 **/
	public interface IQuadtreeBase{

		/**
		 * Get shape
		 **/
		ConvexShape GetShape ();
	}

	/**
	 * Interface for all kind of quadtree agent
	 **/
	public interface IQuadtreeAgent : IQuadtreeBase{

		/**
		 * Nofity when agent is about to be add to quadtree node
		 * 
		 **/
		void BeforeAddToQuadtreeNode ();

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

	}
		
	/**
	 * Interface for all kind of query
	 **/
	public interface IQuadtreeQuery : IQuadtreeBase{

	}

	public class QuadtreeNode{

		#region Properties
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
		#endregion

		#region Constructor
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
		#endregion

		#region Deconstructor
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
		#endregion

		#region Class methods
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
		#endregion

		#region Private methods
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
		 * Set boundary of this node
		 **/
		void SetBoundary(ConvexRect nodeBoundary){

			if (nodeBoundary == ConvexRect.zero)
				return;

			boundary = nodeBoundary;
		}

		/**
		 * Split quadtree node into 4
		 **/
		private void Split(){

			/*
			#if DEBUG
			Debug.Log("Split quadtree");
			#endif
			*/
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
		 * Return number of element and overlap element under quadtree node include child node
		 **/
		int TotalElements(bool includeOverlap = true, bool deepNode = true){

			int numElement= elements.Count;

			if (includeOverlap)
				numElement += overlapElements.Count; 

			//check child nodes
			if (deepNode) {

				if (nodes == null)
					return numElement;

				IEnumerator er = nodes.GetEnumerator ();
				while (er.MoveNext ()) {
				
					numElement += (er.Current as QuadtreeNode).TotalElements ();
				}

				//not optmized
//				foreach (QuadtreeNode n in nodes)
//					numElement += n.TotalElements ();
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
		int IndexOfNode(IQuadtreeAgent element){

			if ((element == null) || (nodes == null))
				return -1;

			IEnumerator er = nodes.GetEnumerator ();
			int nodeIndex = 0;
			while (er.MoveNext ()) {
			
				if ((er.Current as QuadtreeNode).boundary.ContainPoint2D (element.GetCenter ())) {

					return nodeIndex;
				}

				nodeIndex++;
			}

			#if DEBUG
			Debug.LogError("Element does not located in this node fix me");
			#endif

			return -1;
		}

		/**
		 * Get all elements under this quadtree node
		 * 
		 * Param overlap true will include elements that is overlapped
		 * 
		 * Param includeChild true will search deep down node's child
		 **/
		public List<IQuadtreeAgent> GetAllElements(bool overlap = true, bool includeChild = true){
		
			List<IQuadtreeAgent> result = new List<IQuadtreeAgent> ();

			if (includeChild && (nodes != null)) {
			
				IEnumerator er = nodes.GetEnumerator ();
				while (er.MoveNext ()) {
				
					result.AddRange ((er.Current as QuadtreeNode).GetAllElements (overlap, includeChild));
				}

				//not optmized
//				foreach (QuadtreeNode n in nodes) {
//
//					result.AddRange (n.GetAllElements (overlap, includeChild));
//				}
			}

			result.AddRange (elements);
			if (overlap)
				result.AddRange (overlapElements);

			return result;
		}

		/**
		 * Re-organize tree
		 * 
		 * Child nodes will be removed if 4 child node is leaf and contain no objects
		 **/
		void ReOrganize(){

			if (nodes != null) {

				bool cleanChildeNodes = true;

				IEnumerator er = nodes.GetEnumerator ();
				while (er.MoveNext ()) {
				
					//tell child node to re-organize
					(er.Current as QuadtreeNode).ReOrganize ();

					//if child has 4 child nodes
					if (!(er.Current as QuadtreeNode).IsLeaf) {

						cleanChildeNodes = false;
					}

					//if child has elements
					if ((er.Current as QuadtreeNode).TotalElements (true, false) != 0) {

						cleanChildeNodes = false;
					}


				}

				//not optmized
//				foreach (QuadtreeNode n in nodes) {
//
//					//tell child node to re-organize
//					n.ReOrganize ();
//
//					//if child has 4 child nodes
//					if (!n.IsLeaf) {
//
//						cleanChildeNodes = false;
//					}
//
//					//if child has elements
//					if (n.TotalElements (true, false) != 0) {
//
//						cleanChildeNodes = false;
//					}
//
//				}


				if (cleanChildeNodes)
					nodes = null;
			}
		}
		#endregion

		#region Public methods
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

				List<IQuadtreeAgent>.Enumerator er = elementsNextFrame.GetEnumerator ();
				while (er.MoveNext ()) {
				
					Add (er.Current);
				}

				//not optmized
//				foreach (IQuadtreeAgent element in elementsNextFrame) {
//				
//					Add (element);
//				}

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
			newElement.BeforeAddToQuadtreeNode ();

			//Debug.Log("Level "+ Depth);

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
						+ " element last center "+ (newElement as QtAgent).lastPosition);
					#endif
					return false;
				}

				//check if element's boundary fit in the 
				//child node otherwise element is overlapping multiple node
				CollisionResult iResult = newElement.GetShape().IntersectWithShape(nodes[nodeIndex].boundary);

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
				
			elements.Add (newElement);

			//notify element it has been added to this quadtree node
			newElement.AfterAddToQuadtreeNode (this);

			//Split if needed
			if (elements.Count > elementCapacity) {

				Split ();

				//Distribute elements to corespond child node
				List<IQuadtreeAgent>.Enumerator er = elements.GetEnumerator();
				while (er.MoveNext ()) {
				
					//if elemnt out of node boundary
					if (!Add (er.Current)) {
						#if DEBUG
						Debug.LogWarning("Distribute element to child node fail, elemnt is out of node boundary." +
							"We need to add from root");
						#endif

						//add element from root
						rootQuadtree ().Add (er.Current);
					}
				}

				//not optmized
//				foreach (IQuadtreeAgent e in elements) {
//				
//					//if elemnt out of node boundary
//					if (!Add (e)) {
//						#if DEBUG
//						Debug.LogWarning("Distribute element to child node fail, elemnt is out of node boundary." +
//							"We need to add from root");
//						#endif
//
//						//add element from root
//						rootQuadtree ().Add (e);
//					}
//						
//				}

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

						CollisionResult r = element.GetShape().IntersectWithShape (nodes [nodeIndex].boundary);

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
					&& (element.GetShape().IntersectWithShape(nodes[nodeIndex].boundary) == CollisionResult.Fit))
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
		 * Find elements that might contact with given element
		 * under this node.
		 * 
		 * 
		 * Param includeSelf is true given element might be in the result, default is false
		 * 
		 * Param upwardSearch true query will find the node that query shape complete fit in  in first
		 * place then start search downward from that node. It is recommend to leave value as true for 
		 * more accurate result.
		 * 
		 * Param compare a function you can provide and do addition check base on result it found
		 **/
		public List<IQuadtreeAgent> FindElements(IQuadtreeAgent element, bool upwardSearch = true, bool includeSelf = false, OnCompare compare = null){

			if (element == null) {

				#if DEBUG
				Debug.LogError("Can't find elements, given element is null");
				#endif
				return null;
			}

			/**
			 * Upward search from this node
			 * 
			 * This will go up until the node wihch element shape complete fit in
			 **/
			if (upwardSearch) {

				//If this is the node element shape complete fit in
				CollisionResult cResult = element.GetShape().IntersectWithShape(boundary);

				//we found the node then start from this node
				if (cResult == CollisionResult.Fit) {

					//downward search from parent if there is parent node
					//the reason from parent node is that parent node's overlap element
					//might be contact this shape
					if (parentNode != null) {

						//from parent node we start downward search
						return parentNode.FindElements (element, false, includeSelf, compare);
					}

				} else {//continue search upward

					//If there is a parent node otherwsie this is root node and start from
					//here downward search
					if (parentNode != null) {

						//Continue finding upward until the node element shape can totally fit in
						return parentNode.FindElements (element, true, includeSelf, compare);
					}
				}

			}

			/**
			 * Downward search from this node
			 * 
			 * Downward search only search elements in child node include this node
			 **/

			List<IQuadtreeAgent> result = new List<IQuadtreeAgent> ();

			CollisionResult r = element.GetShape ().IntersectWithShape (boundary);

			//if not contact with element shape 
			if (r == CollisionResult.None)
				return result;

			//search child nodes
			IEnumerator er = nodes.GetEnumerator();
			while (er.MoveNext ()) {
			
				//downward search child
				result.AddRange ((er.Current as QuadtreeNode).FindElements (element, false, includeSelf, compare));
			}

			//not optmized
//			if (nodes != null) {
//
//				foreach (QuadtreeNode n in nodes) {
//
//					//downward search child
//					result.AddRange (n.FindElements (element, false, includeSelf, compare));
//				}
//			}

			//add this node's elements
			result.AddRange (elements);
			result.AddRange (overlapElements);


			if (!includeSelf) {
				result.Remove (element);
			}

			if (compare != null) {
				
				List<IQuadtreeAgent> filterResult = new List<IQuadtreeAgent>();

				List<IQuadtreeAgent>.Enumerator qer = result.GetEnumerator ();
				while (qer.MoveNext ()) {
				
					if (compare (qer.Current))
						filterResult.Add (qer.Current);
				}

				//not optmized
//				foreach (IQuadtreeAgent e in result) {
//
//					if (compare (e))
//						filterResult.Add (e);
//						
//				}

				return filterResult;
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
		 * Query elements within or contact with query shape
		 * 
		 * You can create your own query with any kind of convex shape
		 * 
		 * Return list of elements contact with or within query shape
		 * 
		 * Param upwardSearch true query will find the node that query shape complete fit in  in first
		 * place then start search downward from that node. It is recommend to leave value as true for 
		 * more accurate result. 
		 **/
		public List<IQuadtreeAgent> QueryRange(IQuadtreeQuery query, bool upwardSearch = true){

			if (query == null) {

				#if DEBUG
				Debug.LogError("Can't query range, given query is null");
				#endif
				return null;
			}

			/**
			 * Upward search from this node
			 * 
			 * This will go up until the node wihch query shape complete fit in
			 **/
			if (upwardSearch) {

				//If this is the node query shape complete fit in
				CollisionResult cResult = query.GetShape().IntersectWithShape(boundary);

				//we found the node then start query from this node
				if (cResult == CollisionResult.Fit) {

					//downward search from parent if there is parent node
					//the reason from parent node is that parent node's overlap element
					//might be contact query shape
					if (parentNode != null) {

						//from parent node we start downward search
						return parentNode.QueryRange (query, false);
					}
						
				} else {//continue search upward

					//If there is a parent node otherwsie this is root node and start from
					//here downward search
					if (parentNode != null) {

						//Continue finding upward until the node query shape can totally fit in
						return parentNode.QueryRange (query, true);
					}
				}

			}

			/**
			 * Downward search from this node
			 * 
			 * Downward search only search elements in child node include this node
			 **/

			List<IQuadtreeAgent> result = new List<IQuadtreeAgent> ();

			CollisionResult r = query.GetShape ().IntersectWithShape (boundary);

			//if not contact with query shape which is not in range
			if (r == CollisionResult.None)
				return result;

			//search child nodes
			if (nodes != null) {

				IEnumerator er = nodes.GetEnumerator ();
				while (er.MoveNext ()) {
					result.AddRange ((er.Current as QuadtreeNode).QueryRange (query, false));
				}
					
			}

			//add this node's elements
			result.AddRange (elements);
			result.AddRange (overlapElements);

			//Check if any of element contact query shape
			List<IQuadtreeAgent> filterResult = new List<IQuadtreeAgent>();

			List<IQuadtreeAgent>.Enumerator resultEr = result.GetEnumerator();
			while(resultEr.MoveNext()){

				if(resultEr.Current.GetShape().IntersectWithShape(query.GetShape()) != CollisionResult.None){
				
					filterResult.Add(resultEr.Current);
				}
			}

			return filterResult;
		}

		/**
		 * Query from root quadtree and nodes with in query shape
		 * 
		 **/
		public List<IQuadtreeAgent> QueryRangeFromRoot(IQuadtreeQuery query){

			//If not root then find root and query
			if (parentNode != null)
				return rootQuadtree ().QueryRange (query);

			return QueryRange (query);
		}
		#endregion

		#region Debug
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
			Gizmos.color = debugDrawColor;
			//Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

			//LT->RT
			Gizmos.DrawLine (new Vector3 (dBoundary.x, dBoundary.y, z),
				new Vector3 (dBoundary.x + dBoundary.width, dBoundary.y, z));

			//RT->RB
			Gizmos.DrawLine (new Vector3 (dBoundary.x + dBoundary.width, dBoundary.y, z),
				new Vector3 (dBoundary.x + dBoundary.width, dBoundary.y - dBoundary.height, z));

			//RB->LB
			Gizmos.DrawLine (new Vector3 (dBoundary.x + dBoundary.width, dBoundary.y - dBoundary.height, z),
				new Vector3 (dBoundary.x, dBoundary.y - dBoundary.height, z));

			//LB->LT
			Gizmos.DrawLine (new Vector3 (dBoundary.x, dBoundary.y - dBoundary.height, z),
				new Vector3 (dBoundary.x, dBoundary.y, z));


		}
		/**
		 * Log all node's information
		 **/
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
		#endregion

	}
}
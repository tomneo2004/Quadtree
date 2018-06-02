using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NP.NPQuadtree{

	public class NodeBound{

		float _x;
		public float x {

			get {
				return _x;
			}

			set {
				_x = value;
				CalculateCenter ();
			}
		}

		float _y;
		public float y {

			get {
				return _y;
			}

			set {
				_y = value;
				CalculateCenter ();
			}
		}

		float _width;
		public float width {

			get {
				return _width;
			}

			set {
				_width = value;
				CalculateCenter ();
			}
		}

		float _height;
		public float height {

			get {
				return _height;
			}

			set {
				_height = value;
				CalculateCenter ();
			}
		}

		Vector2 _center;
		public Vector2 center{ get{ return _center;}}

		public Vector2 size{ get{ return new Vector2 (_width, _height);}}

		/**
		 * Get TopLeft corner position
		 **/
		public Vector2 TLCorner{ get{  return new Vector2 (_x, _y);}}

		/**
		 * Get TopRight corner position
		 **/
		public Vector2 TRCorner{ get{  return new Vector2 (_x + _width, _y);}}

		/**
		 * Get BottomRight corner position
		 **/
		public Vector2 BRCorner{ get{  return new Vector2 (_x + _width, _y - _height);}}

		/**
		 * Get BottomLeft corner position
		 **/
		public Vector2 BLCorner{ get{  return new Vector2 (_x, _y - _height);}}

		public float xMin{

			get{

				return Mathf.Min (_x, _x + _width);
			}
		}

		public float xMax{

			get{

				return Mathf.Max (_x, _x + _width);
			}
		}

		public float yMin{

			get{

				return Mathf.Min (_y, _y - _height);
			}
		}

		public float yMax{

			get{

				return Mathf.Max (_y, _y - _height);
			}
		}

		public Vector2 minCorner{

			get{

				return new Vector2 (xMin, yMin);
			}
		}

		public Vector2 maxCorner{

			get{

				return new Vector2 (xMax, yMax);
			}
		}

		public static NodeBound zero{

			get{
				return new NodeBound (0.0f, 0.0f, 0.0f, 0.0f);
			}
		}

		public NodeBound(float x, float y, float width, float height){

			_x = x;
			_y = y;
			_width = Mathf.Abs(width);
			_height = Mathf.Abs(height);
		}

		public NodeBound(Vector2 position, Vector2 size){

			_x = position.x;
			_y = position.y;
			_width = Mathf.Abs(size.x);
			_height = Mathf.Abs(size.y);
		}

		void CalculateCenter(){

			_center = new Vector2 (_x + _width / 2.0f, _y - _height / 2.0f);
		}

		public bool ContainPoint2D(Vector2 point){

			if (point.x >= xMin && point.x <= xMax
			   && point.y >= yMin && point.y <= yMax)
				return true;

			return false;
		}
	}


	public enum IntersectionResult{

		/**
		 * Whole object fit in node boundary
		 **/
		Fit,

		/**
		 * Part of object in node boundary or
		 * cover entire boundary
		 **/
		Overlap,

		/**
		 * Object not intersect or fit in node boundary
		 **/
		None
	}

	public interface IQuadtreeAgent{

		/**
		 * Return position in 2d in world space
		 **/
		Vector2 Position2D ();

		/**
		 * Intersect with quadtree node's boundary
		 * 
		 * Return NodeBoundIntersection
		 * 
		 * Param NodeBound is in world space which is topleft corner as origin
		 **/
		IntersectionResult IntersectWithBoundary (NodeBound nodeBoundary);

		/**
		 * Return true if agent in query range
		 **/
		bool InQueryRange (IQuadtreeQuery query);

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
		 * Param NodeBound is in world space which is topleft corner as origin
		 **/
		bool IntersectWithBoundary (NodeBound quadtreeBoundary);

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
		 * Boundary TopLeft is origin point
		 **/
		NodeBound boundary;

		/**
		 * Get quadtree's boundary
		 * 
		 * The boundary is in world space and TopLeft corner as origin
		 **/
		public NodeBound Boundary{ 
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
			NodeBound dBoundary = boundary;
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
		public static QuadtreeNode createRootQuadtree(NodeBound nodeBoundry){
		
			QuadtreeNode rootQuadtree = new QuadtreeNode (null);

			rootQuadtree.SetBoundary (nodeBoundry);

			return rootQuadtree;
		}

		/**
		 * Set boundary 
		 * 
		 * Center will automatically be calculated
		 **/
		private void SetBoundary(NodeBound nodeBoundary){

			if (nodeBoundary == NodeBound.zero)
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
		 * Add element to quadtree
		 **/
		public bool Add(IQuadtreeAgent newElement){

			if (newElement == null) {
			
				#if DEBUG
				Debug.LogError("Node does not implement IQuadtree");
				#endif
				return false;
			}

			//If we have child node
			if (nodes != null) {

				int nodeIndex = IndexOfNode (newElement);

				//can't find index of node
				if (nodeIndex < 0)
					return false;

				//check if element's boundary fit in the 
				//child node otherwise element is overlapping multiple node
				IntersectionResult iResult = newElement.IntersectWithBoundary(nodes[nodeIndex].boundary);

				switch (iResult) {
				case IntersectionResult.Fit:
					//Element's boudnary fit in child node's boundary, add it to child node
					return nodes [nodeIndex].Add (newElement);

				case IntersectionResult.Overlap:
					//Element's boudnary overlap multiple child node, add it to this node
					overlapElements.Add (newElement);
					return true;

				case IntersectionResult.None:
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

			NodeBound newBoundary;
			float width = boundary.width / 2.0f;
			float height = boundary.height / 2.0f;

			nodes = new QuadtreeNode[4];

			//TopLeft(NorthWest)
			newBoundary = new NodeBound(boundary.x, boundary.y, width, height);
			nodes[0] = new QuadtreeNode (this);
			nodes[0].SetBoundary (newBoundary);

			//TopRight(NorthEast)
			newBoundary = new NodeBound(boundary.x+width, boundary.y, width, height);
			nodes[1] = new QuadtreeNode (this);
			nodes[1].SetBoundary (newBoundary);

			//BottomRight(SouthEast)
			newBoundary = new NodeBound(boundary.x+width, boundary.y - height, width, height);
			nodes[2] = new QuadtreeNode (this);
			nodes[2].SetBoundary (newBoundary);

			//BottomLeft(SouthWest)
			newBoundary = new NodeBound(boundary.x, boundary.y - height, width, height);
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

				if (nodeIndex >= 0)
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
		 * Find possible elements that might contact with given element
		 * under this node
		 * 
		 * Param includeSelf is true given element might be in the result, default is false
		 **/
		public List<IQuadtreeAgent> FindElements(IQuadtreeAgent element, bool includeSelf = false){

			//element position on within this node
			if (!boundary.ContainPoint2D (element.Position2D ()))
				return null;

			List<IQuadtreeAgent> result = new List<IQuadtreeAgent> ();

			//Search child node
			if (nodes != null) {

				int nodeIndex = IndexOfNode (element);

				if (nodeIndex >= 0) {

					QuadtreeNode node = nodes [nodeIndex];

					IntersectionResult r = element.IntersectWithBoundary (node.boundary);

					//element boundary fit in this child boundary otherwise element overlap multiple nodes
					if (r == IntersectionResult.Fit) {
					
						result.AddRange (node.FindElements (element));

					} else if(r == IntersectionResult.Overlap) {

						foreach (QuadtreeNode n in nodes) {
						
							//if element intersect with child node
							if (element.IntersectWithBoundary (n.boundary) != IntersectionResult.None)
								result.AddRange (n.FindElements (element));
								
						}
					}
				}
					
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
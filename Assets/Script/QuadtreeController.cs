using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;
using UnityEngine.Profiling;

namespace NP.NPQuadtree{

	public class QuadtreeController : MonoBehaviour {

		public int capacity = 4;
		public Vector2 quadtreeSize;

		ConvexRect boundary;
		QuadtreeNode quadtree;

		public GameObject obj;

		public int objToAdd = 0;
		public float addInterval = 0.0f;

		QtAgent pickAgent;
		QtAgent placedAgent;
		List<QtAgent> cacheAgents = new List<QtAgent>();

		public float queryRadius = 0.2f;
		QtCircleQuery circleQuery;
		public Vector2 rectQuerySize;
		QtRectangleQuery rectQuery;
		bool drawQuery;
		List<IQuadtreeAgent> queryAgents = new List<IQuadtreeAgent>();

		public float slowMotion = 1.0f;

		public bool drawQuadtreeDebug = true;
		public bool drawAgentDebug = true;
		public bool drawQueryDebug = true;

		void Awake(){

			quadtreeSize = new Vector2(Camera.main.orthographicSize-1.0f, Camera.main.orthographicSize-1.0f);


			boundary = new ConvexRect(new Vector2(transform.position.x, transform.position.y),
				new Vector2(quadtreeSize.x, quadtreeSize.y)
			);

			QuadtreeNode.elementCapacity = capacity;
			quadtree = QuadtreeNode.createRootQuadtree (boundary);

			if (objToAdd > 0)
				StartCoroutine (AutoAddObject ());

		}

		IEnumerator AutoAddObject(){

			Debug.Log ("Adding... "+quadtree.TotalElementCount);

			for (int i = 0; i < objToAdd; i++) {

				Vector3 pos;
				GameObject go = Instantiate (obj);
				QtCircleAgent agnet = go.GetComponent<QtCircleAgent> ();

				pos = new Vector3 (Random.Range (boundary.x+agnet.Radius(), boundary.x + boundary.width - agnet.Radius()), 
					Random.Range (boundary.y - agnet.Radius(), boundary.y - boundary.height + agnet.Radius()), 
					transform.position.z);

				go.transform.position = pos;
				go.name = "Agent " + i;

				quadtree.Add (go.GetComponent<QtAgent> ());

				cacheAgents.Add (go.GetComponent<QtAgent> ());

				yield return new WaitForSeconds(addInterval);
			}

			Debug.Log ("Finish add "+quadtree.TotalElementCount);


			quadtree.LevelDesc ();
		}

		IEnumerator RemoveAgents(QtAgent[] agents){

			Debug.Log ("Removing... "+quadtree.TotalElementCount);

			foreach (QtAgent agent in agents) {

				quadtree.Remove (agent);
				GameObject.Destroy (agent.agGameObject);

				yield return new WaitForSeconds (addInterval);
			}

			Debug.Log ("Finish remove "+quadtree.TotalElementCount);


			quadtree.LevelDesc ();
		}

		// Use this for initialization
		void Start () {
		}

		void LateUpdate(){

			quadtree.UpdateQuadtree ();

			//quadtree.LevelDesc ();
		}

		// Update is called once per frame
		void Update () {

			slowMotion = Mathf.Clamp(slowMotion, 0.01f, 1.0f);
			Time.timeScale = slowMotion;
			Time.fixedDeltaTime = 0.02f * slowMotion;

			if (Input.GetButtonDown("Fire1")) {

				Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				pos.z = transform.position.z;

				GameObject go = Instantiate (obj);
				go.transform.position = pos;
				go.transform.rotation = Quaternion.identity;

				quadtree.Add (go.GetComponent<QtAgent> ());

				placedAgent = go.GetComponent<QtAgent>();

				if (go.GetComponent<Rigidbody2D> () != null) {

					int cornerIndex = Random.Range (0, quadtree.Boundary.AllCorners.Length);
					Vector2 corner = quadtree.Boundary.AllCorners [cornerIndex];
					Vector2 dir = (corner - placedAgent.GetCenter ()).normalized * 3.0f;
					go.GetComponent<Rigidbody2D> ().AddForce (dir);
				}

			}

			if (Input.GetButtonDown ("Fire2")) {

				Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				pos.z = transform.position.z;

				GameObject go = Instantiate (obj);
				go.transform.position = pos;
				go.GetComponent<SpriteRenderer> ().color = Color.black;

				quadtree.Add (go.GetComponent<QtAgent> ());

				pickAgent = go.GetComponent<QtAgent>();


				/*
				//clear prvious agent contact status
				foreach (IQuadtreeAgent a in queryAgents)
					a.GetGameObject ().GetComponent<QtCircleAgent> ().contact = false;

				List<IQuadtreeAgent> retriveAgents = pickAgent.CurrentNode.FindElements(pickAgent);
				foreach (IQuadtreeAgent agent in retriveAgents) {

					agent.GetGameObject ().GetComponent<QtCircleAgent> ().contact = true;

					queryAgents.Add (agent);
				}
				*/

			}

			if (Input.GetButtonDown ("Jump")) {

				QtAgent[] agents = GameObject.FindObjectsOfType<QtAgent> ();

				StartCoroutine (RemoveAgents (agents));

			}

			if (Input.GetButton ("Submit")) {
			
				Vector3 mPos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				Vector2 pos = new Vector2 (mPos.x, mPos.y);

				if (circleQuery == null) {

					circleQuery = new QtCircleQuery (pos, queryRadius);
				}

				if (rectQuery == null) {
				
					rectQuery = new QtRectangleQuery (pos, rectQuerySize);
				}

				((ConvexCircle)circleQuery.GetShape ()).Center = pos;
				((ConvexCircle)circleQuery.GetShape ()).Radius = queryRadius;

				((ConvexRect)rectQuery.GetShape ()).center = pos;
				((ConvexRect)rectQuery.GetShape ()).widthFromCenter = rectQuerySize.x;
				((ConvexRect)rectQuery.GetShape ()).heightFromCenter = rectQuerySize.y;

				//clear prvious agent contact status
				List<IQuadtreeAgent>.Enumerator er = queryAgents.GetEnumerator();
				while (er.MoveNext ()) {

					(er.Current as QtCircleAgent).contact = false;
				}
				queryAgents.Clear ();


				// quadtree query
				er = quadtree.QueryRange (circleQuery).GetEnumerator();
				//er = quadtree.QueryRange (rectQuery).GetEnumerator();
				while (er.MoveNext()) {

					(er.Current as QtCircleAgent).contact = true;
					queryAgents.Add (er.Current);

					Vector2 force = er.Current.GetCenter () - (circleQuery.GetShape () as ConvexCircle).Center;
					force = force.normalized * (100.0f * ((circleQuery.GetShape () as ConvexCircle).Radius - force.magnitude / (circleQuery.GetShape () as ConvexCircle).Radius));

					if((er.Current as QtAgent).agRigidbody2D){
					
						(er.Current as QtAgent).agRigidbody2D.AddForce(force);
					}

				}
					
				drawQuery = true;
			} else {

				drawQuery = false;


				//clear prvious agent contact status
				/*
				foreach (IQuadtreeAgent a in queryAgents){
					//a.GetGameObject ().GetComponent<QtCircleAgent> ().contact = false;
					(a as QtCircleAgent).contact = false;
				}
				*/
				List<IQuadtreeAgent>.Enumerator er = queryAgents.GetEnumerator();
				while (er.MoveNext ()) {

					(er.Current as QtCircleAgent).contact = false;
				}
				queryAgents.Clear ();
				
			}
				
		}

		void OnDrawGizmos(){


			if (quadtree != null && drawQuadtreeDebug)
				quadtree.DebugDraw (transform.position.z);
			

			if (drawAgentDebug) {

				foreach (QtCircleAgent agent in GameObject.FindObjectsOfType<QtCircleAgent>()) {

					agent.DebugDraw ();
				}
			}
				
			if (drawQuery && drawQueryDebug) {

				circleQuery.DrawQuery ();
				//rectQuery.DrawQuery();
			}


			/*
			if (placedAgent != null) {
				
				QuadtreeNode n = quadtree.FindNode (placedAgent);
				if (n != null) {

					n.debugDrawColor = Color.red;
					n.DebugDraw (transform.position.z, false);
				}

			}

			if (pickAgent != null) {

				Gizmos.color = Color.yellow;
				List<IQuadtreeAgent> foundAgent = quadtree.FindElements (pickAgent, false, delegate(IQuadtreeAgent agent) {

					float dist = (agent.GetCenter() - pickAgent.GetCenter()).magnitude;

					if(dist <= 10.0f)
						return true;

					return false;
				});

				foreach (IQuadtreeAgent a in foundAgent) {

					Gizmos.DrawLine (new Vector3 (a.Position2D ().x, a.Position2D ().y, transform.position.z),
						new Vector3 (pickAgent.Position2D ().x, pickAgent.Position2D ().y, transform.position.z));
				}
			}
			*/
				
		}
			
	}
}


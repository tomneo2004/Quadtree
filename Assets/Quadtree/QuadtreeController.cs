using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Collision;
using NP.Convex.Shape;

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

			for (int i = 0; i < objToAdd; i++) {

				Vector3 pos;
				GameObject go = Instantiate (obj);

				pos = new Vector3 (Random.Range (boundary.x, boundary.x + boundary.width), 
					Random.Range (boundary.y, boundary.y - boundary.height), 
					transform.position.z);

				go.transform.position = pos;
				go.name = "Agent " + i;

				quadtree.Add (go.GetComponent<QtAgent> ());

				yield return new WaitForSeconds(addInterval);
			}

			Debug.Log ("Finish add "+quadtree.TotalElementCount);


			quadtree.LevelDesc ();
		}

		// Use this for initialization
		void Start () {
		}

		void LateUpdate(){

			quadtree.UpdateQuadtree ();
		}

		// Update is called once per frame
		void Update () {

			if (Input.GetButtonDown("Fire1")) {

				if (placedAgent != null) {

					QuadtreeNode n = quadtree.FindNode (placedAgent);
					n.debugDrawColor = Color.white;
				}

				Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				pos.z = transform.position.z;

				GameObject go = Instantiate (obj);
				go.transform.position = pos;

				quadtree.Add (go.GetComponent<QtAgent> ());

				placedAgent = go.GetComponent<QtAgent>();

			}

			if (Input.GetButtonDown ("Fire2")) {

				Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				pos.z = transform.position.z;

				GameObject go = Instantiate (obj);
				go.transform.position = pos;
				go.GetComponent<SpriteRenderer> ().color = Color.black;

				quadtree.AddNextFrame (go.GetComponent<QtAgent> ());

				pickAgent = go.GetComponent<QtAgent>();


			}
				
		}

		void OnDrawGizmosSelected(){

			if (quadtree != null)
				quadtree.DebugDraw (transform.position.z);

			if (placedAgent != null) {
				
				QuadtreeNode n = quadtree.FindNode (placedAgent);
				if (n != null) {

					n.debugDrawColor = Color.red;
					n.DebugDraw (transform.position.z);
				}

			}

			if (pickAgent != null) {

				Gizmos.color = Color.yellow;
				List<IQuadtreeAgent> foundAgent = quadtree.FindElements (pickAgent);
				foreach (IQuadtreeAgent a in foundAgent) {

					Gizmos.DrawLine (new Vector3 (a.Position2D ().x, a.Position2D ().y, transform.position.z),
						new Vector3 (pickAgent.Position2D ().x, pickAgent.Position2D ().y, transform.position.z));
				}
			}
				
		}
			
	}
}


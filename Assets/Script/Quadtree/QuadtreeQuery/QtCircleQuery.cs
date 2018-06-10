using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	public class QtCircleQuery : QtQuery {

		public QtCircleQuery(Vector2 center, float radius){

			shape = new ConvexCircle (center, radius);
		}


		public void DrawQuery(){

			if (shape == null)
				return;



			ConvexCircle circle = shape as ConvexCircle;

			Gizmos.color = Color.yellow;
			float cx = circle.Radius*Mathf.Cos(0);
			float cy = circle.Radius*Mathf.Sin(0);
			Vector2 cpos = circle.Center + new Vector2 (cx, cy);
			Vector2 cnewPos = cpos;
			Vector2 clastPos = cpos;
			for(float theta = 0.1f; theta<Mathf.PI*2.0f; theta+=0.1f){
				cx = circle.Radius*Mathf.Cos(theta);
				cy = circle.Radius*Mathf.Sin(theta);
				cnewPos = circle.Center+ new Vector2(cx,cy);
				Gizmos.DrawLine(cpos,cnewPos);
				cpos = cnewPos;
			}
			Gizmos.DrawLine(cpos,clastPos);
		}
	}


}


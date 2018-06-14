using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	public class QtRectangleQuery : QtQuery {

		public QtRectangleQuery(Vector2 center, Vector2 size){

			shape = new ConvexRect (center, size);
		}

		public void DrawQuery(){

			if (shape == null)
				return;

			ConvexRect rect = shape as ConvexRect;

			Gizmos.color = Color.yellow;

			Vector2[] allCorners = rect.AllCorners;
			//LT->RT
			Gizmos.DrawLine (new Vector3 (allCorners[0].x, allCorners[0].y, 0),
				new Vector3 (allCorners[1].x, allCorners[1].y, 0));

			//RT->RB
			Gizmos.DrawLine (new Vector3 (allCorners[1].x, allCorners[1].y, 0),
				new Vector3 (allCorners[2].x, allCorners[2].y, 0));

			//RB->LB
			Gizmos.DrawLine (new Vector3 (allCorners[2].x, allCorners[2].y, 0),
				new Vector3 (allCorners[3].x, allCorners[3].y, 0));

			//LB->LT
			Gizmos.DrawLine (new Vector3 (allCorners[3].x, allCorners[3].y, 0),
				new Vector3 (allCorners[0].x, allCorners[0].y, 0));
		}
	}
		
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NP.NPQuadtree;
using NP.Convex.Shape;

namespace NP.NPQuadtree{

	public class QtQuery : IQuadtreeQuery {

		/**
		 *Query shape
		 **/
		protected ConvexShape shape;

		//Implement IQuadtreeQuery
		public virtual ConvexShape GetShape (){

			return shape;
		}

	}
}


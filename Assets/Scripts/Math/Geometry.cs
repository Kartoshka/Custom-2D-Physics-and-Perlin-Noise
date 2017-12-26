using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Provide a library of geometric intersection functions to determine whether two shapes intersected
namespace Kartoshka.Math
{
	/*
	 * Circle shape defined by a position and a radius
	 */
	public class Circle
	{
		public Vector2 Center
		{
			get { return this.m_center; }
		}

		public float Radius 
		{
			get { return this.m_radius; }
		}

		Vector2 m_center;
		float m_radius;

		public Circle(Vector2 center, float r){
			m_center = center;
			m_radius = r;
		}

	}

	/*
	 * Box/Rectangle shape defined by four vertices
	 */ 
	public class Box
	{
		public Vector2 tlCorner 
		{
			get { return this.tl; }
		}

		public Vector2 trCorner 
		{
			get { return this.tr; }
		}		

		public Vector2 blCorner 
		{
			get { return this.bl; }
		}		

		public Vector2 brCorner 
		{
			get { return this.br; }
		}

		public Vector2 Center
		{
			get { return (this.br - this.bl) * 0.5f + this.bl + (this.tl - this.bl) * 0.5f;}
		}

		Vector2 tl, tr, bl, br;

		public Box(Vector2 tl, Vector2 tr, Vector2 bl, Vector2 br){ 
			this.tl = tl;
			this.tr = tr;
			this.bl = bl;
			this.br = br;
		}
	}

	/*
	 * Edge defined by two bounding points
	 * We also calculate its parametric equation form 
	 */
	public class Edge
	{
		public Vector2 P1 {
			get { return this.m_p1; }
		}
		public Vector2 P2 {
			get { return this.m_p2; }
		}
		public Vector2 Dir {
			get { return m_p2 - m_p1; }	
		}

		public float Slope
		{
			get { return this.m_slope; }
		}

		public float Intercept
		{
			get { return this.m_intercept; }
		}

		Vector2 m_p1, m_p2;
		float m_slope;
		float m_intercept;

		public Edge(Vector2 p1, Vector2 p2)
		{
			this.m_p1 = p1;
			this.m_p2 = p2;

			this.m_slope = (p2.y-p1.y)/(p2.x-p1.x);
			this.m_intercept = p1.y - p1.x * m_slope;

		}
	}

	public struct SIntersectionInfo
	{
		public Vector2 intersectionPoint;
		public float intersectionDepth;
		public Vector2 intersectionNormal;
	}

	public static class Geometry {

		public static bool Intersect(Circle c, Box b, out SIntersectionInfo collInfo){
			collInfo = new SIntersectionInfo ();

			//Special case: Our center is inside the box, this is the easiest case to detect, hardest to handle 
			if (PointInBox (b, c.Center))
			{
				//CHEAT: project our center's circle onto every edge
				//       pick the closest one
				//       say that we interesected there (not accurate, can create bugs for fast moving objects or small circles)

				//Each edge of the box
					Edge[] edges = {
					new Edge (b.tlCorner, b.trCorner),
					new Edge (b.trCorner, b.brCorner),
					new Edge (b.brCorner, b.blCorner),
					new Edge (b.blCorner, b.tlCorner)
				};

				float smallestDistance = float.MaxValue;
				foreach (Edge e in edges)
				{
					Vector2 proj = (Vector2.Dot (c.Center - e.P1, e.Dir) / Vector2.Dot (e.Dir, e.Dir))*e.Dir;
					float dist = ((c.Center - e.P1) - proj).magnitude;
					//Each time we find an edge closer to our center, set ourselves to intersect there
					if (dist < smallestDistance)
					{
						smallestDistance = dist;
						collInfo.intersectionPoint = proj + e.P1;
						collInfo.intersectionDepth = (c.Center - collInfo.intersectionPoint).magnitude;
						collInfo.intersectionNormal = new Vector2 (-e.Dir.y, e.Dir.x).normalized;
					}
				}
				return true;
			} 
			//Otherwise there may be an intersection despite our center not being inside
			else
			{
				return LineIntersectCircle (c, new Edge (b.tlCorner, b.trCorner), out collInfo)
					|| LineIntersectCircle (c, new Edge (b.trCorner, b.brCorner), out collInfo)
					|| LineIntersectCircle (c, new Edge (b.brCorner, b.blCorner), out collInfo)
					|| LineIntersectCircle (c, new Edge (b.blCorner, b.tlCorner), out collInfo);
			}
		}

		//Test whether circle 1 intersects circle 2
		public static bool Intersect(Circle c1, Circle c2, out SIntersectionInfo collision)
		{
			float distanceCenter = (c1.Center - c2.Center).magnitude;
			collision = new SIntersectionInfo ();

			//Calculate whether the distance between centers is smaller than radius sum (consider edges touching a collision)
			if (distanceCenter <= c1.Radius + c2.Radius)
			{
				collision.intersectionDepth = c1.Radius + c2.Radius - distanceCenter;
				//Set the intersection point on the edge of the second circle towards the first circle center
				collision.intersectionPoint = c2.Center + (c1.Center - c2.Center).normalized * c2.Radius; 
				collision.intersectionNormal = (c1.Center - c2.Center).normalized;
				return true;
			} else
			{
				return false;
			}
		}

		public static bool PointInBox(Box b, Vector2 point)
		{
			//0 ≤ AP·AB ≤ AB·AB and 0 ≤ AP·AD ≤ AD·AD where ABCD are the vertices of the rectangle and P is the point 
			Vector2 tl_p = b.tlCorner - point;
			Vector2 tl_tr = b.tlCorner - b.trCorner;
			Vector2 tl_bl = b.tlCorner - b.blCorner;

			float tl_p_tl_tr = Vector2.Dot (tl_p, tl_tr);
			float tl_p_tl_bl = Vector2.Dot (tl_p, tl_bl);

			return (tl_p_tl_tr>=0 && tl_p_tl_tr <= Vector2.Dot(tl_tr,tl_tr)) && (tl_p_tl_bl >= 0 && tl_p_tl_bl <= Vector2.Dot(tl_bl,tl_bl));
		}


		public static bool LineIntersectCircle(Circle c, Edge e, out SIntersectionInfo collInfo)
		{
			float ratioProj = (Vector2.Dot (c.Center-e.P1, e.Dir) / Vector2.Dot (e.Dir, e.Dir));
			collInfo = new SIntersectionInfo ();
			//Verify if our projected point lies on the actual edge, aka whether it maps to a length between 0 and 1 of the edge
			if (ratioProj >= 0.0f && ratioProj <= 1.0f)
			{
				//Get the perpendicular of the projection triangle formed by the center and the edge starting at P1
				Vector2 perp = (c.Center - e.P1)- (ratioProj * e.Dir);
				//Then return whether the distance between the center and the edge is smaller than the radius
				if (perp.magnitude <= c.Radius)
				{
					//Set the collision depth and the collision point to be where the edge of the circle is past the edge
					collInfo.intersectionDepth = c.Radius - perp.magnitude;
					collInfo.intersectionPoint = c.Center - perp;
					collInfo.intersectionNormal = new Vector2 (-e.Dir.y, e.Dir.x).normalized;
					return true;
				}
			} else 
			{

				//If our projection isn't between the two points, check whether the edge points are within the radius of the center
				//The collision will occur at the edge points if there is one
				if((c.Center-e.P1).magnitude <= c.Radius)
				{

					collInfo.intersectionDepth = c.Radius - (c.Center - e.P1).magnitude;
					collInfo.intersectionPoint = e.P1;
					collInfo.intersectionNormal = new Vector2 (-e.Dir.y, e.Dir.x).normalized;
					return true;
				}
				else if ((c.Center-e.P2).magnitude <= c.Radius)
				{
					collInfo.intersectionDepth = c.Radius - (c.Center - e.P2).magnitude;
					collInfo.intersectionPoint = e.P2;
					collInfo.intersectionNormal = new Vector2 (-e.Dir.y, e.Dir.x).normalized;
					return true;
				}
			}

			return false;
		}

	}
}


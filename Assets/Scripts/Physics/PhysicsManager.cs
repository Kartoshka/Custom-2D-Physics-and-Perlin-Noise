using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kartoshka.Math;

public struct SCollisionInfo
{
	public CustomCollider otherObj;
	public Vector2 collisionPoint;
	public float collisionDepth;
	public Vector2 normal;

	public SCollisionInfo (CustomCollider other, Vector2 point, float depth,Vector2 norm)
	{
		otherObj = other;
		collisionPoint = point;
		collisionDepth = depth;
		normal = norm;
	}
}

//NOTE: We use the existing collider2D classes for the Editor integration (so we can visualize the collider easily)
// No logic utilizes the Unity physics engine.
public class PhysicsManager : MonoBehaviour
{

	private static PhysicsManager singleton;

	private List<CustomCollider> m_colliders;

	// Use this for initialization
	void Awake ()
	{
		if (singleton == null)
		{
			m_colliders = new List<CustomCollider> ();

			singleton = this;
		} else if (singleton != this)
		{
			Destroy (this);
		}
	}

	public void AddCollider (CustomCollider coll)
	{
		m_colliders.Add (coll);
	}

	public void RemoveCollider (CustomCollider coll)
	{
		m_colliders.Remove (coll);
	}

	public static PhysicsManager GetInstance ()
	{
		if (singleton == null)
		{
			GameObject holder = new GameObject ("Physics Manager");
			holder.AddComponent<PhysicsManager> ().Awake ();
		}
		return singleton;
	}


	//TODO: Have a more general way of checking collisions if I implement arbitrary shape collision
	//TODO: OctTree Spatial partitioning to reduce number of objects we check against each other
	public void LateUpdate ()
	{
		List<CustomCollider> tempList = new List<CustomCollider> (m_colliders);
		tempList.RemoveAll (item => item == null);
		tempList.Sort (delegate(CustomCollider x, CustomCollider y) {
			return x.updateOrder - y.updateOrder;
		});

		foreach (CustomCollider mainCollider in tempList)
		{
			//Ignore static objects or deactivated objects
			if (mainCollider.isStatic)
			{
				continue;
			}

			if (mainCollider.GetType () == typeof(CustomCircleCollider))
			{
				foreach (CustomCollider otherCollider in tempList)
				{
					//If we're comparing with ourselves skip, or if the other collider isn't active
					if (mainCollider == otherCollider)
					{
						continue;
					}

					// We use the convenient CollisionMatrix unity provides as a data structure to determine whether two objects should collide or not
					// Since we aren't using any unity physics, it won't affect anything else in the game accidentally 
					if (Physics2D.GetIgnoreLayerCollision (mainCollider.gameObject.layer, otherCollider.gameObject.layer))
					{
						continue;
					}

					Circle c1 = ((CustomCircleCollider)mainCollider).GetCircle ();
					bool collided = false;
					SIntersectionInfo inxInfo = new SIntersectionInfo ();
					//Get collision information if it applies
					if (otherCollider.GetType () == typeof(CustomCircleCollider))
					{
						Circle c2 = ((CustomCircleCollider)otherCollider).GetCircle ();
						collided = Geometry.Intersect (c1, c2, out inxInfo);

					} else if (otherCollider.GetType () == typeof(CustomRectangleCollider))
					{
						Box b = ((CustomRectangleCollider)otherCollider).GetBox ();
						collided = Geometry.Intersect (c1, b, out inxInfo);
					}

					if (collided)
					{
						//Case 0: Collisions are ignored because of IgnoreCollisionsMatrix
						//Case 1: Dynamic - Static Collision
						// Case 1.1 One of the two isTrigger : No displacement, notify both objects
						// Case 1.2 Neither are isTrigger    : Displace first Object by the full intersectionDepth, notify both objects
						//Case 2: Dynamic - Dynamic Collision
						// Case 2.1: One of the two isTrigger : No displacement, notify first object (second object will get notified once we get to it in the list)
						// Case 2.2: Neither object isTrigger : Displace each in opposite direction by half the intersectionDepth, notify both objects (because once we get to the second object, there should be no collision)

						bool triggerCollision = mainCollider.isTrigger || otherCollider.isTrigger;

						//Calculating the displacement amount, also used to set the intersection point of the second object
						Vector2 pushBack = c1.Center - inxInfo.intersectionPoint;
						pushBack = pushBack.normalized * inxInfo.intersectionDepth;

						SCollisionInfo mainCollInfo = new SCollisionInfo (otherCollider, inxInfo.intersectionPoint, inxInfo.intersectionDepth,pushBack.normalized);
						SCollisionInfo secondaryCollInfo = new SCollisionInfo (mainCollider, inxInfo.intersectionPoint + pushBack, inxInfo.intersectionDepth,-(pushBack.normalized));

						//Always notify first Object of the collision
						NotifyObjectCollision (mainCollider, mainCollInfo);

						//Case 1.2, 2.1, 2.2 notify all objects if it's not a trigger collision
						//Case 1.1 Notify both objects if it's a trigger, but second object is static
						if (!triggerCollision || otherCollider.isStatic)
						{
							NotifyObjectCollision (otherCollider, secondaryCollInfo);
						}
							
						//Never any displacement if it's a trigger
						if (!triggerCollision)
						{
							
							//Case 1.2
							if (otherCollider.isStatic)
							{
								//If the gameobject decided to self destruct or deactive when it was notified of the collision, no need to move it
								if (mainCollider != null && mainCollider.gameObject.activeInHierarchy)
								{
									mainCollider.transform.position += (Vector3)pushBack;
								}
							} 
							//Case 2.2
							else
							{
								//If either object was destroyed or deactivated when it was notified of a collision, no need to displace either of them
								if (mainCollider != null && otherCollider != null && mainCollider.gameObject.activeInHierarchy && otherCollider.gameObject.activeInHierarchy)
								{
									mainCollider.transform.position += (Vector3)pushBack * 0.5f;
									otherCollider.transform.position -= (Vector3)pushBack * 0.5f;
								}
		
							}
						}

					}
				}
			}
		}
	}


	public void NotifyObjectCollision (CustomCollider g, SCollisionInfo collInfo)
	{
		ICollideable[] listeners = g.gameObject.GetComponents<ICollideable> ();
		foreach (ICollideable l in listeners)
		{
			l.OnCollide (collInfo);
		}
	}
		
}

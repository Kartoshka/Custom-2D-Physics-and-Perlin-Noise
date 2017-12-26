using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kartoshka.Math;

//Refer to CustomRecangleCollider class for comments regarding the use of Unity's Collider2D class

//This class represents all the 2d circle colliders in our game
[RequireComponent(typeof(CircleCollider2D))]
public class CustomCircleCollider : CustomCollider {

	CircleCollider2D m_coll;
	// Use this for initialization
	void Start () 
	{
		base.Start ();
		//Cache the collider to avoid repeated calls of getcomponent
		m_coll = this.GetComponent<CircleCollider2D> ();
	}
	
	public Circle GetCircle()
	{
		float radius = m_coll.bounds.extents.x;
		Vector2 center = m_coll.offset + new Vector2 (this.transform.position.x, this.transform.position.y);
		return new Circle (center, radius);
			
	}
}

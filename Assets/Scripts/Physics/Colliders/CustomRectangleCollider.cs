using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kartoshka.Math;

//We use unity colliders only because they are nicely represented in the editor. 
//No collision code utilizes the unity physics engine or collision EXCEPT for the water mesh, which uses raycasts. 
//However, the water mesh was outside the scope of this assignment, and as such all assignment specified instructions do not utilize any built in functions 
// Other than the utility of the built in data structure
[RequireComponent(typeof(BoxCollider2D))]
public class CustomRectangleCollider : CustomCollider {


	BoxCollider2D m_collider;

	public void Start()
	{
		base.Start ();
		//Cache the collider to reduce calls 
		m_collider = this.GetComponent<BoxCollider2D> ();

	}

	public Box GetBox()
	{
		//Using bounds extents gives us the exact length (multiplied by all parent scaling) of half of the box width and height in the x and y coordinates of the vector2 respectively
		Vector2 size = m_collider.size*0.5f;
		//Calculate the center of the collider box
		Vector2 center = m_collider.offset + new Vector2 (this.transform.position.x, this.transform.position.y);

		Vector2 fwdVector = this.transform.rotation * new Vector2 (1.0F, 0.0f);

		Vector2 upVector = new Vector2 (-fwdVector.y, fwdVector.x);

		//Forward vector used to bound the width
		fwdVector *= size.x;
		//Up vector used to bound the height
		upVector  *= size.y;

		//Calculate the bound positions using the vectors
		Vector2 tl = center - fwdVector + upVector;
		Vector2 tr = center + fwdVector + upVector;
		Vector2 bl = center - fwdVector - upVector;
		Vector2 br = center + fwdVector - upVector;
		return new Box (tl, tr, bl, br);
	}
}

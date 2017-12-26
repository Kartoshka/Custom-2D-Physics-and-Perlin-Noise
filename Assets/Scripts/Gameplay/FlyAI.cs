using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomRigidBody))]
public class FlyAI : MonoBehaviour, ICollideable {

	public void OnCollide (SCollisionInfo collInfo)
	{
		//When fly colliders with water or an end zone, it disappears
		switch (collInfo.otherObj.gameObject.tag)
		{
		case "Water":
			Destroy (this.gameObject);
			break;
		}
	}

	public void DropFly()
	{
		//Fly will start dropping when notified
		CustomRigidBody rb = this.GetComponent<CustomRigidBody> ();
		rb.useGravity = true;
		rb.useWind = false;
		rb.bounceOnImpact = true;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EOW : MonoBehaviour, ICollideable {

	//Destroys anything which enters it
	public void OnCollide (SCollisionInfo collInfo)
	{
		Destroy (collInfo.otherObj.gameObject);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour, ICollideable {

	public float lifeTime = 4.0f;

	public void Start()
	{
		StartCoroutine (Kill());
	}

	public void OnCollide (SCollisionInfo collInfo)
	{
		if (collInfo.otherObj.tag == "Bug")
		{
			FlyAI fly = collInfo.otherObj.GetComponent<FlyAI> ();
			if (fly != null)
			{
				fly.DropFly ();
			}
			Destroy (this.gameObject);
		}
	}

	private IEnumerator Kill()
	{
		yield return new WaitForSeconds (lifeTime);
		Destroy (this.gameObject);
	}

}

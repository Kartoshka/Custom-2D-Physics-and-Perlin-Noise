using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager : MonoBehaviour, ICollideable {

	public Vector2 m_currentWind = Vector2.zero;

	public float minSpeed = 2;
	public float maxSpeed = 10;

	public float minUpdate = 1;
	public float maxUpdate = 2;

	public void Start()
	{
		StartCoroutine (UpdateSpeed (Random.Range (minUpdate, maxUpdate)));
	}

	private IEnumerator UpdateSpeed(float waitTime)
	{
		yield return new WaitForSeconds (waitTime);
		m_currentWind = new Vector2 (Random.Range (minSpeed, maxSpeed), 0);
		StartCoroutine (UpdateSpeed (Random.Range (minUpdate, maxUpdate)));
	}

	public void OnCollide (SCollisionInfo collInfo)
	{
		CustomRigidBody rb = collInfo.otherObj.GetComponent<CustomRigidBody> ();
		if (rb != null)
		{
			rb.SetWindVelocity (m_currentWind);
		}
	}
}

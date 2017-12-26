using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CustomCollider))]
public class CustomRigidBody : MonoBehaviour, ICollideable {


	[Header("Wind")]
	public bool useWind = false;
	public Vector2 m_wind = Vector2.zero;

	[Space(5)]

	[Header("Gravity")]
	public bool useGravity = false;
	[Range(0.0f, 10.0f)]
	public float mass = 1.0f;

	private const float GLOBAL_GRAV_COEFF = 3.0f;
	private float gravCoeff = GLOBAL_GRAV_COEFF;

	private static float gravDamping = 0.02f;

	[Space(5)]

	[Header("Bounce")]
	public bool bounceOnImpact = false;
	[Range(0.0f,1.0f)]
	public float bounceCoeff = 1.0f;

	private Vector2 m_inputVelocity = Vector2.zero;

	private Vector3 collisionPoint = Vector2.zero;

	// Use this for initialization
	void OnEnable () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (useGravity)
		{
			m_inputVelocity.y = m_inputVelocity.y*(1-gravDamping) - gravCoeff * mass * Time.deltaTime;
			gravCoeff = GLOBAL_GRAV_COEFF;
		}

		this.transform.position += (Vector3)GetActiveVelocity () * Time.deltaTime;
	}

	public void OnCollide (SCollisionInfo collInfo)
	{
		if (bounceOnImpact && !collInfo.otherObj.isTrigger)
		{
			//This is gonna be some weird jank.
			Vector2 currentVelocity = GetActiveVelocity();
			Vector2 normal = collInfo.normal;

			m_inputVelocity += Vector2.Dot (-currentVelocity, normal) * normal * (1+bounceCoeff);
		}

		collisionPoint = (Vector3)collInfo.collisionPoint;
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawSphere (collisionPoint, 0.1f);
	}

	public Vector2 GetActiveVelocity()
	{
		Vector2 finalVelocity = m_inputVelocity;

		if (useWind)
			finalVelocity += m_wind;
		
		return finalVelocity;
	}

	public void SetInputVelocity(Vector2 input)
	{
		m_inputVelocity += input;
	}

	public void SetWindVelocity(Vector2 wind)
	{
		m_wind = wind;
	}

}

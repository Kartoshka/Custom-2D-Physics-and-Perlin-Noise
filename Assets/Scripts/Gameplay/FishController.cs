using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishController : MonoBehaviour {

	//Units per second
	public float moveSpeed = 2.0f;
	//Degs per second
	public float rotateSpeed = 30.0f;

	private float rotation =0.0f;

	private bool canFire = true;
	public float fireRate = 0.5f;
	public float bulletSpeed = 1.0f;

	public GameObject bulletPrefab;

	private float facingDir = 1.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float xInput = Input.GetAxis ("Horizontal");
		float yInput = Input.GetAxis ("Vertical");


		if (xInput < 0)
		{
			facingDir = -1.0f;
		} else if(xInput>0)
		{
			facingDir = 1.0f;
		}

		rotation += yInput * rotateSpeed * Time.deltaTime;
		rotation = Mathf.Clamp (rotation, -60.0f, 85.0f);

		if (rotation >= 0)
		{
			this.transform.rotation =Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x,this.transform.rotation.eulerAngles.y, rotation));
		} else
		{
			this.transform.rotation =Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x,this.transform.rotation.eulerAngles.y, rotation + 360.0f));
		}

		this.gameObject.transform.position += new Vector3 (xInput, 0, 0) * moveSpeed * Time.deltaTime;


		this.gameObject.transform.localScale = new Vector3 (facingDir*Mathf.Abs (this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
		this.transform.rotation =Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x,this.transform.rotation.eulerAngles.y, facingDir*Mathf.Abs(this.transform.rotation.eulerAngles.z)));

		if (Input.GetButton ("Fire1") && canFire)
		{
			canFire = false;
			StartCoroutine (FireCounter (fireRate));

			GameObject g = Instantiate (bulletPrefab.gameObject, this.transform.position, this.transform.rotation);
			g.transform.localScale = new Vector3 (facingDir*Mathf.Abs (g.transform.localScale.x), g.transform.localScale.y, g.transform.localScale.z);

			Vector2 dir =Quaternion.Euler (new Vector3 (this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z)) * new Vector2 (1.0f, 0.0f);
			if (facingDir < 0)
			{
				dir *= -1;
			}

			foreach (CustomRigidBody rb in g.GetComponentsInChildren<CustomRigidBody>())
			{
				rb.SetInputVelocity (dir.normalized * bulletSpeed);
			}
		}

	}

	private IEnumerator FireCounter(float countdown)
	{
		yield return new WaitForSeconds (countdown);
		canFire = true;
	}
}

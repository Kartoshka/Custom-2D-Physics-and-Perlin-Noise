using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base clase, no use other than preventing an object from having multiple collider scripts
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public abstract class CustomCollider : MonoBehaviour {


	public bool isStatic = false;
	public bool isTrigger = false;
	public int  updateOrder = int.MaxValue;

	public void Start()
	{
		PhysicsManager.GetInstance ().AddCollider (this);
	}

	public void OnDisable()
	{
		//Potentially creating a memory leak of a single physics manager?
		//Not sure why
		PhysicsManager.GetInstance ().RemoveCollider (this);
	}
}

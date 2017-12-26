using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugSpawn : MonoBehaviour {

	public GameObject prefab;

	public float timeMin = 1.0f;
	public float timeMax = 4.0f;

	public float heightSpawnBox = 4.0f;
	void Start () 
	{
		StartCoroutine (SpawnBug(Random.Range(timeMin,timeMax)));
	}
	
	IEnumerator SpawnBug(float time)
	{
		yield return new WaitForSeconds (time);
		if (prefab != null)
		{
			Instantiate (prefab, this.transform.position + new Vector3 (0, Random.Range (-heightSpawnBox / 2, heightSpawnBox / 2), 0),Quaternion.identity);
		}
		StartCoroutine (SpawnBug(Random.Range(timeMin,timeMax)));
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kartoshka.Math.Noise;

//WaterLineGen will generate a randomized body of water between two specified points and above colliders on a specified layer
//The generation uses multiple octaves of Perlin noise
[RequireComponent(typeof(LineRenderer))]
public class WaterLineGen : MonoBehaviour {

	//Maximum height of the water from the starting point
	public float maxHeight =1.0f;

	//Line renderer used to render the water line
	private LineRenderer lr;
	//Prefab used to generate the water volume mesh
	public GameObject watermesh;

	//Points to start the waterline from
	public GameObject p1;
	public GameObject p2;

	//Points per unit distance in unity
	public int pointsPerUnit =25;

	//Flag used to force a regeneration of the water, used for debugging purposes
	public bool regenerate = false;

	//Each Octave defines a different Perlin noise amount that we are adding to 
	public Octave[] octaves;
	//Resulting Perlin noise that we use is stored in this array
	Perlin1D[] perlinGens;

	//The mesh objects which we generated that we store so we can clean up
	GameObject[] meshobjects;
	//The meshes for the water volume we generated 
	Mesh[] meshes;

	//Specify what later the colliders for the fishbowl are on so that we can raycast down and figure out how high we want to make our meshes.
	public LayerMask fishBowlMask;

	public void Start()
	{
		//Clean up any existing objects (in the case that we are being forced to regenerate)
		if (meshobjects != null && meshobjects.Length != 0)
		{
			foreach (GameObject g in meshobjects)
			{
				Destroy (g);
			}
		}
		initialize ();
		generateLine ();
	}

	public void Update()
	{
		if (regenerate)
		{
			regenerate = false;
			Start ();
		}
	}

	//Initialize all our perlin values
	private void initialize()
	{
		//Get all the information we need
		lr = this.GetComponent<LineRenderer> ();
		float width = Mathf.Abs(p2.transform.transform.position.x - p1.transform.position.x);

		lr.positionCount = Mathf.FloorToInt (pointsPerUnit * width);

		perlinGens = new Perlin1D[octaves.Length];
		//For every specified Octave, generate a new Perlin noise with a random seed
		for(int i=0;i<octaves.Length;i++)
		{
			//Since we know how many points we'll need for our line, we can specify it to the Perlin noise so that it doesn't calculate more than that
			perlinGens[i] = new Perlin1D ((Random.Range(0,1000000)), octaves[i].numOctaves, lr.positionCount, (int)(octaves[i].baseWl*pointsPerUnit));
		}
	}

	private void generateLine()
	{
		//Absolute positioning information or the points
		float runningX = p1.transform.position.x;
		float xMove = Mathf.Abs (p2.transform.transform.position.x - p1.transform.position.x) / lr.positionCount;
		//We start our line at the average y between the two points
		float yAvg = (p1.transform.position.y + p2.transform.position.y) / 2.0f;

		//Since we want to constrain our height to maxHeight, calculate the maxHeight attainable with all the octaves in order to rescale it after
		//TODO: Implement some sort of weighing (first octave 0.5, second 0.25, third 0.15, etc....)
		float maxOctaveHeight = 0;
		foreach (Octave o in octaves)
		{
			maxOctaveHeight += o.height;
		}

		meshes = new Mesh[lr.positionCount-1];
		meshobjects = new GameObject[lr.positionCount - 1];

		//For every point that we need to fill in our line
		for (int i = 0; i < lr.positionCount; i++, runningX+=xMove)
		{
			//Get the value from every Perlin noise function that we generated
			float height = 0;
			for(int p=0;p<octaves.Length;p++)
			{
				height += perlinGens [p].GetNoise (i) * octaves [p].height;
			}
			//Set the height to the value we got scaled to fit within the max height
			lr.SetPosition (i, new Vector3 (runningX, ((height)/maxOctaveHeight)*maxHeight+yAvg , -1));

			//Generate mesh for water volume
			if (i != 0)
			{
				meshes [i - 1] = new Mesh ();
				//vertex 0 is the top-left, 1 is the top-right, 2 is the bottom-left, and 3 is the top-right. We'll need to remember that for later.
				Vector3[] Vertices = new Vector3[4];
				Vertices [0] = lr.GetPosition (i - 1);
				Vertices[1] = lr.GetPosition (i);

				Vertices [0].z = 0;
				Vertices [1].z = 0;

				//Raycast down so that we know where to stop our volume
				RaycastHit2D hit0 = Physics2D.Raycast (Vertices [0], Vector2.down,float.MaxValue,fishBowlMask);
				RaycastHit2D hit1 = Physics2D.Raycast (Vertices [1], Vector2.down,float.MaxValue,fishBowlMask);

				Vertices [2] = new Vector3 (hit0.point.x, hit0.point.y, 0);
				Vertices [3] = new Vector3 (hit1.point.x, hit0.point.y, 0);

				//Specify the UVs for our mesh
				Vector2[] UVs = new Vector2[4];
				UVs[0] = new Vector2(0, 1);
				UVs[1] = new Vector2(1, 1);
				UVs[2] = new Vector2(0, 0);
				UVs[3] = new Vector2(1, 0);

				//Specify the triangles/normals for the mesh
				int[] tris = new int[6] { 0, 1, 3, 3, 2, 0 };

				//Assign all the calculated values to the mesh
				meshes[i-1].vertices = Vertices;
				meshes[i-1].uv = UVs;
				meshes[i-1].triangles = tris;

				//Instantiate new object and assign the mesh to it
				meshobjects[i-1] = Instantiate(watermesh,Vector3.zero,Quaternion.identity) as GameObject;
				meshobjects[i-1].GetComponent<MeshFilter>().mesh = meshes[i-1];
				meshobjects[i-1].transform.parent = transform;

				//Create a 2D box collider which matches the dimensions of our mesh volume
				BoxCollider2D boxColl =meshobjects [i-1].GetComponent<BoxCollider2D> ();
				//The size is the height between the top left and bottom left points
				boxColl.size = new Vector2 (xMove, Vertices [0].y - Vertices [2].y);
				//The width is the distance between top left and top right (aka the xMove we defined)
				//The position is the center of the mesh
				boxColl.offset = new Vector2 (Vertices [0].x+xMove*0.5f, (Vertices [0].y - Vertices [2].y)*0.5f + Vertices[2].y);

			}
	
		}
	}
		
}

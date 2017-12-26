using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Provide different interpolation functions
//Reference: https://codeplea.com/simple-interpolation
namespace Kartoshka.Math
{
	public static class Interpolation {

		//Linear (aka on a direct line) interpolation between the two points
		public static float Linear(float a, float b, float t)
		{
			return a + t * (b - a);
		}

		public static float Cosine(float a, float b, float t)
		{
			//Map the interpolation to a shifted and scaled cosine so that it returns a value between [0..1] instead of [-1 ... 1]
			return Linear (a, b, -0.5f * Mathf.Cos (Mathf.PI * t) + 0.5f);
		}

		public static float FastCosine(float a, float b, float t)
		{
			//Approximation of the cosine interpolation, faster computation that actually using the cosine function
			// Map the function to a cubic 
			return Linear(a,b,t*t*(3-2*t));
		}

		//Slow start and rapid increase (equivalent to someone accelerating constantly)
		public static float Acceleration(float a, float b, float t)
		{
			return Linear (a, b, t * t);
		}

		//Linear start but quadratic decrease towards the end (equivalent to someone starting faster and decelerating)
		public static float Decceleration(float a, float b, float t)
		{
			float x = 1 - t;
			return Linear (a, b, 1 - x*x);
		}
	}
}


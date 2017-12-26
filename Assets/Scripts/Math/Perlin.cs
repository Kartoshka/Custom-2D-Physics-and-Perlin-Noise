using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kartoshka.Math;
//Reference: https://codepen.io/Tobsta/post/procedural-generation-part-1-1d-perlin-noise
//Custom Library for Noise, currently only supports 1 dimensional perlin noise
namespace Kartoshka.Math.Noise
{		
	[Serializable]
	public struct Octave
	{
		public float height;
		public int numOctaves;
		public float baseWl;
	}


	public class Perlin1D {

		System.Random rng;

		int width = 0;

		int nOctaves;
		double[] noise;

		public Perlin1D(int seed, int numOctaves, int width, int baseWl)
		{
			nOctaves = numOctaves;
			this.width = width;

			//Instantiate a random number generator with the specified seed
			rng = new System.Random (seed);

			noise = new double[width];

			for (int i = 0; i < width; i++)
			{
				noise [i] = 0;
			}

			float amplitude = 1.0f;
			int waveLength = baseWl;

			for (int i = 0; i < numOctaves; i++)
			{
				double[] values = new double[width];

				double prevRng = 0;
				double nextRng = rng.NextDouble ();

				for (int x = 0; x < width; x++)
				{
					//If we have done a full period, generate a new random number and use it
					if (x % waveLength == 0)
					{
						prevRng = nextRng;
						nextRng = rng.NextDouble ();

						values [x] = prevRng * amplitude;
					}
					//Otherwise use cosine interpolation between the last generated point and next point to be generated
					else
					{
						values [x] = Interpolation.Cosine ((float)prevRng, (float)nextRng, (float)((x % waveLength)/ (float)waveLength)) * amplitude;
					}
					noise [x] += values [x];
				}

				//For every octave we halve the wavelength and halve the amplitude
				amplitude /= 2;
				waveLength = Mathf.Max(1,waveLength/2);
			}
		}



		public float GetNoise(int xOff)
		{
			if (xOff > this.width)
			{
				return 0;
			} else
			{
				//Return a value of perlin noise scaled to [0..1]
				return (float)this.noise [xOff]/(2.0f-Mathf.Pow(2,-this.nOctaves));	
			}
		}


	}
}


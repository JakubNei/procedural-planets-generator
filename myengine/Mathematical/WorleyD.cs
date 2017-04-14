using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using OpenTK;

namespace MyEngine
{
	/// <summary>
	/// Uses double instead of float.
	/// </summary>
	public class WorleyD
	{
		public enum DistanceFunction
		{
			Euclidian,
			Manhattan,
			Chebyshev
		};

		uint seed;
		DistanceFunction distanceFunction;

		public WorleyD(uint seed, DistanceFunction distanceFunction)
		{
			this.seed = seed;
			this.distanceFunction = distanceFunction;
		}



		//Sample single octave of 2D noise
		public double[] GetAt(Vector2d input, int returnArrayLen)
		{
			//Declare some values for later use
			uint numberFeaturePoints;
			Vector2d randomDiff, featurePoint;
			int cubeX, cubeY;

			double[] distanceArray = new double[returnArrayLen];

			//Initialize values in distance array to large values
			for (uint i = 0; i < returnArrayLen; i++)
				distanceArray[i] = 6666;

			//1. Determine which cube the evaluation point is in
			int evalCubeX = (int)Math.Floor(input.X);
			int evalCubeY = (int)Math.Floor(input.Y);

			for (int i = -1; i < 2; ++i)
			{
				for (int j = -1; j < 2; ++j)
				{
					cubeX = evalCubeX + i;
					cubeY = evalCubeY + j;

					//2. Generate a reproducible random number generator for the cube
					var random = new Random((int)FnvHash((uint)(cubeX), (uint)(cubeY), seed));

					//3. Determine how many feature points are in the cube
					numberFeaturePoints = ProbLookup((uint)random.Next(0, int.MaxValue));
					//4. Randomly place the feature points in the cube
					for (uint l = 0; l < numberFeaturePoints; ++l)
					{
						randomDiff.X = (double)random.Next() / 0x100000000;
						randomDiff.Y = (double)random.Next() / 0x100000000;
						featurePoint = new Vector2d(randomDiff.X + (double)cubeX, randomDiff.Y + (double)cubeY);

						//5. Find the feature point closest to the evaluation point. 
						//This is done by inserting the distances to the feature points into a sorted list
						Insert(distanceArray, returnArrayLen, Distance(input, featurePoint, distanceFunction));
					}
					//6. Check the neighboring cubes to ensure their are no closer evaluation points.
					// This is done by repeating steps 1 through 5 above for each neighboring cube
				}
			}

			return distanceArray;
		}



		//Sample single octave of 2D noise
		public double[] GetAt(Vector3d input, int returnArrayLen)
		{
			//Declare some values for later use
			uint numberFeaturePoints;
			Vector3d randomDiff, featurePoint;
			int cubeX, cubeY, cubeZ;

			double[] distanceArray = new double[returnArrayLen];

			//Initialize values in distance array to large values
			for (uint i = 0; i < returnArrayLen; i++)
				distanceArray[i] = 6666;

			//1. Determine which cube the evaluation point is in
			int evalCubeX = (int)Math.Floor(input.X);
			int evalCubeY = (int)Math.Floor(input.Y);
			int evalCubeZ = (int)Math.Floor(input.Z);

			for (int i = -1; i < 2; ++i)
			{
				for (int j = -1; j < 2; ++j)
				{
					for (int k = -1; k < 2; ++k)
					{

						cubeX = evalCubeX + i;
						cubeY = evalCubeY + j;
						cubeZ = evalCubeZ + k;

						//2. Generate a reproducible random number generator for the cube
						var random = new Random((int)FnvHash((uint)(cubeX), (uint)(cubeY), (uint)(cubeZ), seed));

						//3. Determine how many feature points are in the cube
						numberFeaturePoints = ProbLookup((uint)random.Next(0, int.MaxValue));
						//4. Randomly place the feature points in the cube
						for (uint l = 0; l < numberFeaturePoints; ++l)
						{
							randomDiff.X = (double)random.Next() / 0x100000000;
							randomDiff.Y = (double)random.Next() / 0x100000000;
							randomDiff.Z = (double)random.Next() / 0x100000000;
							featurePoint = new Vector3d(
								randomDiff.X + (double)cubeX,
								randomDiff.Y + (double)cubeY,
								randomDiff.Z + (double)cubeZ
							);

							//5. Find the feature point closest to the evaluation point. 
							//This is done by inserting the distances to the feature points into a sorted list
							Insert(distanceArray, returnArrayLen, Distance(input, featurePoint, distanceFunction));
						}
						//6. Check the neighboring cubes to ensure their are no closer evaluation points.
						// This is done by repeating steps 1 through 5 above for each neighboring cube
					}
				}
			}

			return distanceArray;
		}



		/// <summary>
		/// Given a uniformly distributed random number this function returns the number of feature points in a given cube.
		/// </summary>
		/// <param name="value">a uniformly distributed random number</param>
		/// <returns>The number of feature points in a cube.</returns>
		// Generated using mathmatica with "AccountingForm[N[Table[CDF[PoissonDistribution[4], i], {i, 1, 9}], 20]*2^32]"

		uint ProbLookup(uint value)
		{
			if (value < 393325350) return 1;
			if (value < 1022645910) return 2;
			if (value < 1861739990) return 3;
			if (value < 2700834071) return 4;
			if (value < 3372109335) return 5;
			if (value < 3819626178) return 6;
			if (value < 4075350088) return 7;
			if (value < 4203212043) return 8;
			return 9;
		}


		/// <summary>
		/// Inserts value into array using insertion sort. If the value is greater than the largest value in the array
		/// it will not be added to the array.
		/// </summary>
		/// <param name="arr">The array to insert the value into.</param>
		/// <param name="value">The value to insert into the array.</param>

		void Insert(double[] arr, int arrSize, double value)
		{
			double temp;
			for (int i = arrSize - 1; ; i--)
			{
				if (value > arr[i]) break;
				temp = arr[i];
				arr[i] = value;
				if (i + 1 < arrSize) arr[i + 1] = temp;
				if (i == 0) break;
			}
		}




		double Distance(Vector2d p1, Vector2d p2, DistanceFunction distanceFunction)
		{
			if (distanceFunction == DistanceFunction.Euclidian)
			{
				return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
			}
			if (distanceFunction == DistanceFunction.Manhattan)
			{
				return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
			}
			if (distanceFunction == DistanceFunction.Chebyshev)
			{
				Vector2d diff = p1 - p2;
				return Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
			}
			return 0.0f;
		}



		double Distance(Vector3d p1, Vector3d p2, DistanceFunction distanceFunction)
		{
			if (distanceFunction == DistanceFunction.Euclidian)
			{
				return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z);
			}
			if (distanceFunction == DistanceFunction.Manhattan)
			{
				return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) + Math.Abs(p1.Z - p2.Z);
			}
			if (distanceFunction == DistanceFunction.Chebyshev)
			{
				Vector3d diff = p1 - p2;
				return Math.Max(Math.Abs(diff.X), Math.Max(Math.Abs(diff.Y), Math.Abs(diff.Z)));
			}
			return 0.0f;
		}




		/// <summary>
		/// Constant used in FNV hash function.
		/// FNV hash: http://isthe.com/chongo/tech/comp/fnv/#FNV-source
		/// </summary>
		const uint FNV_OFFSET_BASIS = 2166136261;
		/// <summary>
		/// Constant used in FNV hash function
		/// FNV hash: http://isthe.com/chongo/tech/comp/fnv/#FNV-source
		/// </summary>
		const uint FNV_PRIME = 16777619;
		/// <summary>
		/// Hashes three integers into a single integer using FNV hash.
		/// FNV hash: http://isthe.com/chongo/tech/comp/fnv/#FNV-source
		/// </summary>
		/// <returns>hash value</returns>
		uint FnvHash(uint x, uint y, uint z, uint w)
		{ // derived by me
			return (uint)((((((((FNV_OFFSET_BASIS ^ (uint)x) * FNV_PRIME) ^ (uint)y) * FNV_PRIME) ^ (uint)z) * FNV_PRIME) ^ (uint)w) * FNV_PRIME);
		}
		uint FnvHash(uint x, uint y, uint z)
		{ // original
			return (uint)((((((FNV_OFFSET_BASIS ^ (uint)x) * FNV_PRIME) ^ (uint)y) * FNV_PRIME) ^ (uint)z) * FNV_PRIME);
		}
		uint FnvHash(uint x, uint y)
		{ // derived by me
			return (uint)((((FNV_OFFSET_BASIS ^ (uint)x) * FNV_PRIME) ^ (uint)y) * FNV_PRIME);
		}


	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using OpenTK;

namespace MyEngine
{
    public class Worley
    {
        public enum DistanceFunction
        {
            Euclidian,
            Manhattan,
            Chebyshev
        };

        uint seed;
        DistanceFunction distanceFunction;
        List<Vector2> points;

        public Worley(uint seed, DistanceFunction distanceFunction)
        {
            this.seed = seed;
            this.distanceFunction = distanceFunction;
        }



        //Sample single octave of 2D noise
        public float[] GetAt(Vector2 input, int returnArrayLen)
        {
            //Declare some values for later use
            uint numberFeaturePoints;
            Vector2 randomDiff, featurePoint;
            int cubeX, cubeY;

            float[] distanceArray = new float[returnArrayLen];

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
                    numberFeaturePoints = probLookup((uint)random.Next(0, int.MaxValue));
                    //4. Randomly place the feature points in the cube
                    for (uint l = 0; l < numberFeaturePoints; ++l)
                    {
                        randomDiff.X = (float)random.Next() / 0x100000000;
                        randomDiff.Y = (float)random.Next() / 0x100000000;
                        featurePoint = new Vector2(randomDiff.X + (float)cubeX, randomDiff.Y + (float)cubeY);

                        //5. Find the feature point closest to the evaluation point. 
                        //This is done by inserting the distances to the feature points into a sorted list
                        insert(distanceArray, returnArrayLen, Distance(input, featurePoint, distanceFunction));
                    }
                    //6. Check the neighboring cubes to ensure their are no closer evaluation points.
                    // This is done by repeating steps 1 through 5 above for each neighboring cube
                }
            }

            return distanceArray;
        }



        //Sample single octave of 2D noise
        public float[] GetAt(Vector3 input, int returnArrayLen)
        {
            //Declare some values for later use
            uint numberFeaturePoints;
            Vector3 randomDiff, featurePoint;
            int cubeX, cubeY, cubeZ;

            float[] distanceArray = new float[returnArrayLen];

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
                        numberFeaturePoints = probLookup((uint)random.Next(0, int.MaxValue));
                        //4. Randomly place the feature points in the cube
                        for (uint l = 0; l < numberFeaturePoints; ++l)
                        {
                            randomDiff.X = (float)random.Next() / 0x100000000;
                            randomDiff.Y = (float)random.Next() / 0x100000000;
                            randomDiff.Z = (float)random.Next() / 0x100000000;
                            featurePoint = new Vector3(
                                randomDiff.X + (float)cubeX,
                                randomDiff.Y + (float)cubeY,
                                randomDiff.Z + (float)cubeZ
                            );

                            //5. Find the feature point closest to the evaluation point. 
                            //This is done by inserting the distances to the feature points into a sorted list
                            insert(distanceArray, returnArrayLen, Distance(input, featurePoint, distanceFunction));
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
        
        uint probLookup(uint value)
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
        
        void insert(float[] arr, int arrSize, float value)
        {
            float temp;
            for (int i = arrSize - 1; ; i--)
            {
                if (value > arr[i]) break;
                temp = arr[i];
                arr[i] = value;
                if (i + 1 < arrSize) arr[i + 1] = temp;
				if (i == 0) break;
			}
        }



        
        float Distance(Vector2 p1, Vector2 p2, DistanceFunction distanceFunction)
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
                Vector2 diff = p1 - p2;
                return Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
            }
            return 0.0f;
        }


        
        float Distance(Vector3 p1, Vector3 p2, DistanceFunction distanceFunction)
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
                Vector3 diff = p1 - p2;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Neitri.Base
{
    public partial class WorldPos
    {
		/*
		public static WorldPos operator -(WorldPos left, Vector3d right)
		{
			left.insideSectorPosition -= right;
			left.MoveSectorIfNeeded();
			return left;
		}
		public static WorldPos operator -(WorldPos left, Vector3 right)
		{
			return left - right.ToVector3d();
		}
		public static WorldPos operator +(WorldPos left, Vector3d right)
		{
			left.insideSectorPosition += right;
			left.MoveSectorIfNeeded();
			return left;
		}
		public static WorldPos operator +(WorldPos left, Vector3 right)
		{
			return left + right.ToVector3d();
		}


		public static WorldPos operator +(Vector3d left, WorldPos right)
		{
			return right + left;
		}
		public static WorldPos operator +(Vector3 left, WorldPos right)
		{
			return right + left;
		}
		public Vector3d ToVector3d()
		{
			return new Vector3d(
				x + (sectorX * sectorCubeSideLength),
				y + (sectorY * sectorCubeSideLength),
				z + (sectorZ * sectorCubeSideLength)
			);
		}
		public Vector3 ToVector3()
		{
			return new Vector3(
				(float)(x + (sectorX * sectorCubeSideLength)),
				(float)(y + (sectorY * sectorCubeSideLength)),
				(float)(z + (sectorZ * sectorCubeSideLength))
			);
		}

		*/
	}
}

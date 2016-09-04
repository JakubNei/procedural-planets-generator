using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	/// <summary>
	/// Taken from: http://stackoverflow.com/questions/25821037/opentk-opengl-frustum-culling-clipping-too-soon
	/// </summary>
	public class Frustum
	{
		private readonly float[] _clipMatrix = new float[16];
		private readonly float[,] _frustum = new float[6, 4];

		public const int A = 0;
		public const int B = 1;
		public const int C = 2;
		public const int D = 3;

		public enum ClippingPlane : int
		{
			Right = 0,
			Left = 1,
			Bottom = 2,
			Top = 3,
			Back = 4,
			Front = 5
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void NormalizePlane(float[,] frustum, int side)
		{
			float magnitude = (float)Math.Sqrt((frustum[side, 0] * frustum[side, 0]) + (frustum[side, 1] * frustum[side, 1])
												+ (frustum[side, 2] * frustum[side, 2]));
			frustum[side, 0] /= magnitude;
			frustum[side, 1] /= magnitude;
			frustum[side, 2] /= magnitude;
			frustum[side, 3] /= magnitude;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool PointVsFrustum(float x, float y, float z)
		{
			for (int i = 0; i < 6; i++)
			{
				if (this._frustum[i, 0] * x + this._frustum[i, 1] * y + this._frustum[i, 2] * z + this._frustum[i, 3] <= 0.0f)
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool PointVsFrustum(Vector3 location)
		{
			for (int i = 0; i < 6; i++)
			{
				if (this._frustum[i, 0] * location.X + this._frustum[i, 1] * location.Y + this._frustum[i, 2] * location.Z + this._frustum[i, 3] <= 0.0f)
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SphereVsFrustum(float x, float y, float z, float radius)
		{
			for (int p = 0; p < 6; p++)
			{
				float d = _frustum[p, 0] * x + _frustum[p, 1] * y + _frustum[p, 2] * z + _frustum[p, 3];
				if (d <= -radius)
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SphereVsFrustum(Vector3 location, float radius)
		{
			for (int p = 0; p < 6; p++)
			{
				float d = _frustum[p, 0] * location.X + _frustum[p, 1] * location.Y + _frustum[p, 2] * location.Z + _frustum[p, 3];
				if (d <= -radius)
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool VolumeVsFrustum(float x, float y, float z, float width, float height, float length)
		{
			for (int i = 0; i < 6; i++)
			{
				if (_frustum[i, A] * (x - width) + _frustum[i, B] * (y - height) + _frustum[i, C] * (z - length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x + width) + _frustum[i, B] * (y - height) + _frustum[i, C] * (z - length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x - width) + _frustum[i, B] * (y + height) + _frustum[i, C] * (z - length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x + width) + _frustum[i, B] * (y + height) + _frustum[i, C] * (z - length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x - width) + _frustum[i, B] * (y - height) + _frustum[i, C] * (z + length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x + width) + _frustum[i, B] * (y - height) + _frustum[i, C] * (z + length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x - width) + _frustum[i, B] * (y + height) + _frustum[i, C] * (z + length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x + width) + _frustum[i, B] * (y + height) + _frustum[i, C] * (z + length) + _frustum[i, D] > 0)
					continue;
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool VolumeVsFrustum(Bounds volume)
		{
			for (int i = 0; i < 6; i++)
			{
				if (_frustum[i, A] * (volume.X - volume.Width) + _frustum[i, B] * (volume.Y - volume.Height) + _frustum[i, C] * (volume.Z - volume.Length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (volume.X + volume.Width) + _frustum[i, B] * (volume.Y - volume.Height) + _frustum[i, C] * (volume.Z - volume.Length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (volume.X - volume.Width) + _frustum[i, B] * (volume.Y + volume.Height) + _frustum[i, C] * (volume.Z - volume.Length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (volume.X + volume.Width) + _frustum[i, B] * (volume.Y + volume.Height) + _frustum[i, C] * (volume.Z - volume.Length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (volume.X - volume.Width) + _frustum[i, B] * (volume.Y - volume.Height) + _frustum[i, C] * (volume.Z + volume.Length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (volume.X + volume.Width) + _frustum[i, B] * (volume.Y - volume.Height) + _frustum[i, C] * (volume.Z + volume.Length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (volume.X - volume.Width) + _frustum[i, B] * (volume.Y + volume.Height) + _frustum[i, C] * (volume.Z + volume.Length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (volume.X + volume.Width) + _frustum[i, B] * (volume.Y + volume.Height) + _frustum[i, C] * (volume.Z + volume.Length) + _frustum[i, D] > 0)
					continue;
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool VolumeVsFrustum(Vector3 location, float width, float height, float length)
		{
			for (int i = 0; i < 6; i++)
			{
				if (_frustum[i, A] * (location.X - width) + _frustum[i, B] * (location.Y - height) + _frustum[i, C] * (location.Z - length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (location.X + width) + _frustum[i, B] * (location.Y - height) + _frustum[i, C] * (location.Z - length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (location.X - width) + _frustum[i, B] * (location.Y + height) + _frustum[i, C] * (location.Z - length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (location.X + width) + _frustum[i, B] * (location.Y + height) + _frustum[i, C] * (location.Z - length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (location.X - width) + _frustum[i, B] * (location.Y - height) + _frustum[i, C] * (location.Z + length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (location.X + width) + _frustum[i, B] * (location.Y - height) + _frustum[i, C] * (location.Z + length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (location.X - width) + _frustum[i, B] * (location.Y + height) + _frustum[i, C] * (location.Z + length) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (location.X + width) + _frustum[i, B] * (location.Y + height) + _frustum[i, C] * (location.Z + length) + _frustum[i, D] > 0)
					continue;
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CubeVsFrustum(float x, float y, float z, float size)
		{
			for (int i = 0; i < 6; i++)
			{
				if (_frustum[i, A] * (x - size) + _frustum[i, B] * (y - size) + _frustum[i, C] * (z - size) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x + size) + _frustum[i, B] * (y - size) + _frustum[i, C] * (z - size) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x - size) + _frustum[i, B] * (y + size) + _frustum[i, C] * (z - size) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x + size) + _frustum[i, B] * (y + size) + _frustum[i, C] * (z - size) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x - size) + _frustum[i, B] * (y - size) + _frustum[i, C] * (z + size) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x + size) + _frustum[i, B] * (y - size) + _frustum[i, C] * (z + size) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x - size) + _frustum[i, B] * (y + size) + _frustum[i, C] * (z + size) + _frustum[i, D] > 0)
					continue;
				if (_frustum[i, A] * (x + size) + _frustum[i, B] * (y + size) + _frustum[i, C] * (z + size) + _frustum[i, D] > 0)
					continue;
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CalculateFrustum(Matrix4 projectionMatrix, Matrix4 modelViewMatrix)
		{
			_clipMatrix[0] = (modelViewMatrix.M11 * projectionMatrix.M11) + (modelViewMatrix.M12 * projectionMatrix.M21) + (modelViewMatrix.M13 * projectionMatrix.M31) + (modelViewMatrix.M14 * projectionMatrix.M41);
			_clipMatrix[1] = (modelViewMatrix.M11 * projectionMatrix.M12) + (modelViewMatrix.M12 * projectionMatrix.M22) + (modelViewMatrix.M13 * projectionMatrix.M32) + (modelViewMatrix.M14 * projectionMatrix.M42);
			_clipMatrix[2] = (modelViewMatrix.M11 * projectionMatrix.M13) + (modelViewMatrix.M12 * projectionMatrix.M23) + (modelViewMatrix.M13 * projectionMatrix.M33) + (modelViewMatrix.M14 * projectionMatrix.M43);
			_clipMatrix[3] = (modelViewMatrix.M11 * projectionMatrix.M14) + (modelViewMatrix.M12 * projectionMatrix.M24) + (modelViewMatrix.M13 * projectionMatrix.M34) + (modelViewMatrix.M14 * projectionMatrix.M44);

			_clipMatrix[4] = (modelViewMatrix.M21 * projectionMatrix.M11) + (modelViewMatrix.M22 * projectionMatrix.M21) + (modelViewMatrix.M23 * projectionMatrix.M31) + (modelViewMatrix.M24 * projectionMatrix.M41);
			_clipMatrix[5] = (modelViewMatrix.M21 * projectionMatrix.M12) + (modelViewMatrix.M22 * projectionMatrix.M22) + (modelViewMatrix.M23 * projectionMatrix.M32) + (modelViewMatrix.M24 * projectionMatrix.M42);
			_clipMatrix[6] = (modelViewMatrix.M21 * projectionMatrix.M13) + (modelViewMatrix.M22 * projectionMatrix.M23) + (modelViewMatrix.M23 * projectionMatrix.M33) + (modelViewMatrix.M24 * projectionMatrix.M43);
			_clipMatrix[7] = (modelViewMatrix.M21 * projectionMatrix.M14) + (modelViewMatrix.M22 * projectionMatrix.M24) + (modelViewMatrix.M23 * projectionMatrix.M34) + (modelViewMatrix.M24 * projectionMatrix.M44);

			_clipMatrix[8] = (modelViewMatrix.M31 * projectionMatrix.M11) + (modelViewMatrix.M32 * projectionMatrix.M21) + (modelViewMatrix.M33 * projectionMatrix.M31) + (modelViewMatrix.M34 * projectionMatrix.M41);
			_clipMatrix[9] = (modelViewMatrix.M31 * projectionMatrix.M12) + (modelViewMatrix.M32 * projectionMatrix.M22) + (modelViewMatrix.M33 * projectionMatrix.M32) + (modelViewMatrix.M34 * projectionMatrix.M42);
			_clipMatrix[10] = (modelViewMatrix.M31 * projectionMatrix.M13) + (modelViewMatrix.M32 * projectionMatrix.M23) + (modelViewMatrix.M33 * projectionMatrix.M33) + (modelViewMatrix.M34 * projectionMatrix.M43);
			_clipMatrix[11] = (modelViewMatrix.M31 * projectionMatrix.M14) + (modelViewMatrix.M32 * projectionMatrix.M24) + (modelViewMatrix.M33 * projectionMatrix.M34) + (modelViewMatrix.M34 * projectionMatrix.M44);

			_clipMatrix[12] = (modelViewMatrix.M41 * projectionMatrix.M11) + (modelViewMatrix.M42 * projectionMatrix.M21) + (modelViewMatrix.M43 * projectionMatrix.M31) + (modelViewMatrix.M44 * projectionMatrix.M41);
			_clipMatrix[13] = (modelViewMatrix.M41 * projectionMatrix.M12) + (modelViewMatrix.M42 * projectionMatrix.M22) + (modelViewMatrix.M43 * projectionMatrix.M32) + (modelViewMatrix.M44 * projectionMatrix.M42);
			_clipMatrix[14] = (modelViewMatrix.M41 * projectionMatrix.M13) + (modelViewMatrix.M42 * projectionMatrix.M23) + (modelViewMatrix.M43 * projectionMatrix.M33) + (modelViewMatrix.M44 * projectionMatrix.M43);
			_clipMatrix[15] = (modelViewMatrix.M41 * projectionMatrix.M14) + (modelViewMatrix.M42 * projectionMatrix.M24) + (modelViewMatrix.M43 * projectionMatrix.M34) + (modelViewMatrix.M44 * projectionMatrix.M44);

			_frustum[(int)ClippingPlane.Right, 0] = _clipMatrix[3] - _clipMatrix[0];
			_frustum[(int)ClippingPlane.Right, 1] = _clipMatrix[7] - _clipMatrix[4];
			_frustum[(int)ClippingPlane.Right, 2] = _clipMatrix[11] - _clipMatrix[8];
			_frustum[(int)ClippingPlane.Right, 3] = _clipMatrix[15] - _clipMatrix[12];
			NormalizePlane(_frustum, (int)ClippingPlane.Right);

			_frustum[(int)ClippingPlane.Left, 0] = _clipMatrix[3] + _clipMatrix[0];
			_frustum[(int)ClippingPlane.Left, 1] = _clipMatrix[7] + _clipMatrix[4];
			_frustum[(int)ClippingPlane.Left, 2] = _clipMatrix[11] + _clipMatrix[8];
			_frustum[(int)ClippingPlane.Left, 3] = _clipMatrix[15] + _clipMatrix[12];
			NormalizePlane(_frustum, (int)ClippingPlane.Left);

			_frustum[(int)ClippingPlane.Bottom, 0] = _clipMatrix[3] + _clipMatrix[1];
			_frustum[(int)ClippingPlane.Bottom, 1] = _clipMatrix[7] + _clipMatrix[5];
			_frustum[(int)ClippingPlane.Bottom, 2] = _clipMatrix[11] + _clipMatrix[9];
			_frustum[(int)ClippingPlane.Bottom, 3] = _clipMatrix[15] + _clipMatrix[13];
			NormalizePlane(_frustum, (int)ClippingPlane.Bottom);

			_frustum[(int)ClippingPlane.Top, 0] = _clipMatrix[3] - _clipMatrix[1];
			_frustum[(int)ClippingPlane.Top, 1] = _clipMatrix[7] - _clipMatrix[5];
			_frustum[(int)ClippingPlane.Top, 2] = _clipMatrix[11] - _clipMatrix[9];
			_frustum[(int)ClippingPlane.Top, 3] = _clipMatrix[15] - _clipMatrix[13];
			NormalizePlane(_frustum, (int)ClippingPlane.Top);

			_frustum[(int)ClippingPlane.Back, 0] = _clipMatrix[3] - _clipMatrix[2];
			_frustum[(int)ClippingPlane.Back, 1] = _clipMatrix[7] - _clipMatrix[6];
			_frustum[(int)ClippingPlane.Back, 2] = _clipMatrix[11] - _clipMatrix[10];
			_frustum[(int)ClippingPlane.Back, 3] = _clipMatrix[15] - _clipMatrix[14];
			NormalizePlane(_frustum, (int)ClippingPlane.Back);

			_frustum[(int)ClippingPlane.Front, 0] = _clipMatrix[3] + _clipMatrix[2];
			_frustum[(int)ClippingPlane.Front, 1] = _clipMatrix[7] + _clipMatrix[6];
			_frustum[(int)ClippingPlane.Front, 2] = _clipMatrix[11] + _clipMatrix[10];
			_frustum[(int)ClippingPlane.Front, 3] = _clipMatrix[15] + _clipMatrix[14];
			NormalizePlane(_frustum, (int)ClippingPlane.Front);
		}
	}
}

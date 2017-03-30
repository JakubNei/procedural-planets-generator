using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;

namespace MyEngine
{
	public partial class Mesh : IDisposable
	{
		public BufferObjectVector3 Vertices { get; private set; }
		public BufferObjectVector3 Normals { get; private set; }
		public BufferObjectVector3 Tangents { get; private set; }
		public BufferObjectVector2 UVs { get; private set; }
		public BufferObjectInt TriangleIndicies { get; private set; }

		public MyFile file;

		bool recalculateBounds = true;

		/// <summary>
		/// Mesh space bounds of the mesh.
		/// </summary>
		public Bounds Bounds
		{
			get
			{
				if (recalculateBounds)
				{
					recalculateBounds = false;
					lock (Vertices)
					{
						if (Vertices.Count > 0)
						{
							bounds = new Bounds(Vertices[0]);
							bounds.Encapsulate(Vertices);
						}
						else
						{
							bounds = new Bounds(Vector3.Zero);
						}
					}
				}
				return bounds;
			}
			set
			{
				recalculateBounds = false;
				bounds = value;
			}
		}

		public string Name { get; set; }

		Bounds bounds;

		bool isOnGPU = false;

		public VertexArrayObject VertexArray { get; private set; }

		public Mesh(string name = "unnamed mesh")
		{
			this.Name = name;

			Vertices = new BufferObjectVector3();
			Normals = new BufferObjectVector3();
			Tangents = new BufferObjectVector3();
			UVs = new BufferObjectVector2();
			TriangleIndicies = new BufferObjectInt()
			{
				Target = BufTarget.ControlElementArray,
			};

			VertexArray = new VertexArrayObject();
			VertexArray.AddVertexBuffer("vertices", Vertices);
			VertexArray.AddVertexBuffer("normals", Normals);
			VertexArray.AddVertexBuffer("tangents", Tangents);
			VertexArray.AddVertexBuffer("uvs", UVs);
			VertexArray.AddVertexBuffer("triangleIndicies", TriangleIndicies);
			VertexArray.OnChanged += () => { isOnGPU = false; };
		}

		public void RecalculateBounds()
		{
			lock (this)
			{
				recalculateBounds = true;
			}
		}

		public void NotifyDataChanged()
		{
			lock (this)
			{
				isOnGPU = false;
			}
		}


		public void Draw(bool drawWithTesselationSupport = false)
		{
			if (isOnGPU == false)
			{
				UploadDataToGpu();
			}
			GL.BindVertexArray(VertexArray.VaoHandle); MyGL.Check();
			GL.DrawElements(drawWithTesselationSupport ? PrimitiveType.Patches : PrimitiveType.Triangles, TriangleIndicies.Count, DrawElementsType.UnsignedInt, IntPtr.Zero); MyGL.Check();
			GL.BindVertexArray(0); MyGL.Check();
		}

		public void EnsureIsOnGpu()
		{
			if (!isOnGPU)
				UploadDataToGpu();
		}

		public void UploadDataToGpu()
		{
			if (!HasNormals())
				RecalculateNormals();

			if (!HasTangents())
				RecalculateTangents();

			//VertexArrayObj.Dispose(); // causes access violation if we try to reupload
			if (VertexArray.VaoHandle == -1) VertexArray.CreateBuffer();
			VertexArray.UploadDataToGpu();

			isOnGPU = true;
		}

		public bool HasTangents()
		{
			return Tangents != null && Vertices.Count == Tangents.Count;
		}

		public bool HasNormals()
		{
			return Normals != null && Vertices.Count == Normals.Count;
		}

		public bool HasUVs()
		{
			return UVs != null && Vertices.Count == UVs.Count;
		}

		public void Dispose()
		{
			VertexArray.Dispose();
		}

		~Mesh()
		{
			GC.SuppressFinalize(this);
			fromFinalizer.Enqueue(this);
		}

		static ConcurrentQueue<Mesh> fromFinalizer = new ConcurrentQueue<Mesh>();
		public static void ProcessFinalizerQueue()
		{
			Mesh toDispose = null;
			while (fromFinalizer.TryDequeue(out toDispose))
				toDispose.Dispose();
		}

	}
}
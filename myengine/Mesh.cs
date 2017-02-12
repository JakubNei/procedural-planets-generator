using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MyEngine
{
	public partial class Mesh : IDisposable
	{
		public VertexBufferObject<Vector3> Vertices { get; private set; }
		public VertexBufferObject<Vector3> Normals { get; private set; }
		public VertexBufferObject<Vector3> Tangents { get; private set; }
		public VertexBufferObject<Vector2> UVs { get; private set; }
		public VertexBufferObject<int> TriangleIndicies { get; private set; }

		public Asset asset;

		bool recalculateBounds = true;

		/// <summary>
		/// Local space bounds of the mesh.
		/// </summary>
		public Bounds Bounds
		{
			get
			{
				if (recalculateBounds)
				{
					recalculateBounds = false;
					if (Vertices.Count > 0)
					{
						bounds = new Bounds(Vertices[0], Vector3.Zero);
						foreach (var point in Vertices)
						{
							bounds.Encapsulate(point);
						}
					}
					else
					{
						bounds = new Bounds(Vector3.Zero, Vector3.Zero);
					}
				}

				return bounds;
			}
		}

		Bounds bounds;

		bool isOnGPU = false;

		public VertexArrayObject VertexArray { get; private set; }

		public Mesh()
		{
			Vertices = new VertexBufferObjectVector3();
			Normals = new VertexBufferObjectVector3();
			Tangents = new VertexBufferObjectVector3();
			UVs = new VertexBufferObjectVector2();
			TriangleIndicies = new VertexBufferObjectInt()
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

		public void Draw()
		{
			lock (this)
			{
				if (isOnGPU == false)
				{
					UploadDataToGpu();
				}
				GL.BindVertexArray(VertexArray.handle);
				GL.DrawElements(PrimitiveType.Triangles, TriangleIndicies.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
				GL.BindVertexArray(0);
			}
		}

		void UploadDataToGpu()
		{
			if (!HasNormals())
			{
				RecalculateNormals();
			}
			if (!HasTangents())
			{
				RecalculateTangents();
			}

			//VertexArrayObj.Dispose(); // causes access violation if we try to reupload
			if (VertexArray.handle == -1) VertexArray.CreateBuffer();
			VertexArray.SendDataToGpu();

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

		public class VertexArrayObject : IDisposable
		{
			public event Action OnChanged;

			Dictionary<string, IVertexBufferObject> nameToVbo = new Dictionary<string, IVertexBufferObject>();
			List<string> vboNamesToBind = new List<string>();

			public int nextLayoutIndex = 0;

			public int handle = -1;

			public void AddVertexBuffer(string name, IVertexBufferObject vbo)
			{
				if (nameToVbo.ContainsKey(name) && nameToVbo[name] == vbo) return;
				nameToVbo[name] = vbo;
				if (vbo.UsesLayoutIndex)
				{
					vbo.LayoutIndex = nextLayoutIndex;
					nextLayoutIndex++;
				}
				vboNamesToBind.Add(name);
				vbo.OnChanged += () => { OnChanged?.Invoke(); };
				OnChanged?.Invoke();
			}

			public IVertexBufferObject GetVertexArrayBufferObject(string name)
			{
				return nameToVbo[name];
			}

			public void CreateBuffer()
			{
				if (handle == -1)
				{
					handle = GL.GenVertexArray();
				}

				GL.BindVertexArray(handle);
				foreach (var vboName in vboNamesToBind)
				{
					var vbo = nameToVbo[vboName];
					vbo.BindBufferToVAO();
				}
				vboNamesToBind.Clear();
				GL.BindVertexArray(0);
			}

			/// <summary>
			/// Delete buffers
			/// </summary>
			public void Dispose()
			{
				if (handle != -1)
				{
					GL.DeleteVertexArray(handle);
					handle = -1;
				}
				foreach (var kvp in nameToVbo)
				{
					kvp.Value.DeleteBuffer();
				}
			}

			public void SendDataToGpu()
			{
				foreach (var kvp in nameToVbo)
				{
					kvp.Value.SendDataToGpu(kvp.Key);
				}
			}
		}

		public interface IVertexBufferObject
		{
			event Action OnChanged;

			int Count { get; }
			int Handle { get; set; }
			int LayoutIndex { get; set; }
			bool UsesLayoutIndex { get; }

			void CreateBuffer();

			void SendDataToGpu(string myName);

			void BindBufferToVAO();

			void DeleteBuffer();

			/// <summary>
			/// Takes one data at index and adds them again at the end.
			/// </summary>
			/// <param name="index"></param>
			void Duplicate(VertexIndex index);
		}

		public enum BufTarget
		{
			Array,
			ControlElementArray,
		}

		public class VertexBufferObjectVector3 : VertexBufferObject<Vector3>
		{
			public VertexBufferObjectVector3()
			{
				ElementType = typeof(float);
				DataStrideInElementsNumber = 3;
			}
		}
		public class VertexBufferObjectVector2 : VertexBufferObject<Vector2>
		{
			public VertexBufferObjectVector2()
			{
				ElementType = typeof(float);
				DataStrideInElementsNumber = 2;
			}
		}
		public class VertexBufferObjectInt : VertexBufferObject<int>
		{
			public VertexBufferObjectInt()
			{
				ElementType = typeof(int);
				DataStrideInElementsNumber = 1;
			}
		}



		public class VertexBufferObject<T> : List<T>, IVertexBufferObject where T : struct
		{
			public event Action OnChanged;

			public int Handle { get; set; }
			public int LayoutIndex { get; set; }
			public BufTarget Target { get; set; }

			BufferTarget GL_BufferTarget
			{
				get
				{
					if (Target == BufTarget.ControlElementArray) return BufferTarget.ElementArrayBuffer;
					if (Target == BufTarget.Array) return BufferTarget.ArrayBuffer;
					return BufferTarget.ArrayBuffer;
				}
			}

			public int NumberOfElements { get { return this.Count; } }
			public Type ElementType { get; set; }

			VertexAttribPointerType GL_PointerType
			{
				get
				{
					if (ElementType == typeof(byte)) return VertexAttribPointerType.Byte;
					if (ElementType == typeof(short)) return VertexAttribPointerType.Short;
					if (ElementType == typeof(ushort)) return VertexAttribPointerType.UnsignedInt;
					if (ElementType == typeof(int)) return VertexAttribPointerType.Int;
					if (ElementType == typeof(uint)) return VertexAttribPointerType.UnsignedInt;
					if (ElementType == typeof(float)) return VertexAttribPointerType.Float;
					if (ElementType == typeof(double)) return VertexAttribPointerType.Double;
					throw new Exception(MemberName.For(() => ElementType) + " of type:" + ElementType + " is not supported");
				}
			}

			public int DataStrideInElementsNumber { get; set; }

			public int DataSizeOfOneElementInBytes
			{
				get
				{
					return System.Runtime.InteropServices.Marshal.SizeOf(new T());
				}
			}

			public bool UsesLayoutIndex
			{
				get
				{
					return GL_BufferTarget != BufferTarget.ElementArrayBuffer;
				}
			}

			public int offset;

			public VertexBufferObject()
			{
				Handle = -1;
			}

			public void CreateBuffer()
			{
				if (Handle == -1) Handle = GL.GenBuffer();
			}

			public void SendDataToGpu(string myName)
			{
				CreateBuffer();
				int sizeFromGpu;
				GL.BindBuffer(GL_BufferTarget, Handle);
				var arr = this.ToArray();
				var size = NumberOfElements * DataSizeOfOneElementInBytes;
				GL.BufferData(GL_BufferTarget, (IntPtr)(size), arr, BufferUsageHint.StaticDraw);
				GL.GetBufferParameter(GL_BufferTarget, BufferParameterName.BufferSize, out sizeFromGpu);
				// if (size != sizeFromGpu) Debug.Error(myName + " size mismatch size=" + GL_BufferTarget + " sizeFromGpu=" + sizeFromGpu);
			}

			public void BindBufferToVAO()
			{
				CreateBuffer();
				if (UsesLayoutIndex)
				{
					GL.EnableVertexAttribArray(LayoutIndex);
				}
				GL.BindBuffer(GL_BufferTarget, Handle);
				if (UsesLayoutIndex)
				{
					//GL.VertexAttribPointer(Shader.positionLocation, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
					GL.VertexAttribPointer(LayoutIndex, DataStrideInElementsNumber, GL_PointerType, false, DataSizeOfOneElementInBytes, offset);
				}
			}

			public void DeleteBuffer()
			{
				if (Handle != -1) GL.DeleteBuffer(Handle);
			}

			public void SetData(IList<T> data)
			{
				this.Clear();
				this.AddRange(data);
				OnChanged?.Invoke();
			}

			public void SetData(T[] data)
			{
				this.Clear();
				this.AddRange(data);
				OnChanged?.Invoke();
			}

			public void SetData(IEnumerable<T> data)
			{
				this.Clear();
				this.AddRange(data);
				OnChanged?.Invoke();
			}

			public void Duplicate(VertexIndex index)
			{
				Add(this[index.vertexIndex]);
			}
		}
	}
}
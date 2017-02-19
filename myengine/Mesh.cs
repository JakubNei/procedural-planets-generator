using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections;
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
			set
			{
				recalculateBounds = false;
				bounds = value;
			}
		}

		Bounds bounds;

		bool isOnGPU = false;

		public VertexArrayObject VertexArray { get; private set; }

		public Mesh()
		{
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
			GL.BindVertexArray(VertexArray.VaoHandle); My.Check();
			GL.DrawElements(drawWithTesselationSupport ? PrimitiveType.Patches : PrimitiveType.Triangles, TriangleIndicies.Count, DrawElementsType.UnsignedInt, IntPtr.Zero); My.Check();
			GL.BindVertexArray(0); My.Check();
		}

		void UploadDataToGpu()
		{
			if (!HasNormals())
				RecalculateNormals();

			if (!HasTangents())
				RecalculateTangents();

			//VertexArrayObj.Dispose(); // causes access violation if we try to reupload
			if (VertexArray.VaoHandle == -1) VertexArray.CreateBuffer();
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

			Dictionary<string, IBufferObject> nameToVbo = new Dictionary<string, IBufferObject>();
			List<string> vboNamesToBind = new List<string>();

			public int nextLayoutIndex = 0;

			public int VaoHandle = -1;

			public void AddVertexBuffer(string name, IBufferObject vbo)
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

			public IBufferObject GetVertexArrayBufferObject(string name)
			{
				return nameToVbo[name];
			}

			public void CreateBuffer()
			{
				if (VaoHandle == -1)
				{
					VaoHandle = GL.GenVertexArray(); My.Check();
				}
				foreach (var vboName in vboNamesToBind)
				{
					var vbo = nameToVbo[vboName];
					vbo.CreateBuffer();
				}

				GL.BindVertexArray(VaoHandle); My.Check();
				foreach (var vboName in vboNamesToBind)
				{
					var vbo = nameToVbo[vboName];
					vbo.BindBufferToVAO();
				}
				vboNamesToBind.Clear();
				GL.BindVertexArray(0); My.Check();
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); My.Check();
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0); My.Check();
			}

			/// <summary>
			/// Delete buffers
			/// </summary>
			public void Dispose()
			{
				if (VaoHandle != -1)
				{
					GL.DeleteVertexArray(VaoHandle); My.Check();
					VaoHandle = -1;
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

		public interface IBufferObject
		{
			event Action OnChanged;

			int Count { get; }
			int VboHandle { get; set; }
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

		public class BufferObjectVector3 : BufferObject<Vector3>
		{
			public BufferObjectVector3()
			{
				ElementType = typeof(float);
				DataStrideInElementsNumber = 3;
			}

			public void SetData(IntPtr ptr, int countOfVector3s)
			{
				var countOfFloats = countOfVector3s * 3;
				var data = new float[countOfFloats];
				Marshal.Copy(ptr, data, 0, countOfFloats);
				this.Clear();
				for (int i = 0; i < countOfFloats; i += 3)
				{
					this.Add(new Vector3(data[i], data[i + 1], data[i + 2]));
				}
			}
		}
		public class BufferObjectVector2 : BufferObject<Vector2>
		{
			public BufferObjectVector2()
			{
				ElementType = typeof(float);
				DataStrideInElementsNumber = 2;
			}
		}
		public class BufferObjectInt : BufferObject<int>
		{
			public BufferObjectInt()
			{
				ElementType = typeof(int);
				DataStrideInElementsNumber = 1;
			}
		}



		public class BufferObject<T> : List<T>, IBufferObject where T : struct
		{
			public event Action OnChanged;

			public int VboHandle { get; set; }
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

			int offset = 0;

			public BufferObject()
			{
				VboHandle = -1;
			}

			public void CreateBuffer()
			{
				if (VboHandle == -1)
				{
					VboHandle = GL.GenBuffer(); My.Check();
				}
			}

			public void SendDataToGpu(string myName)
			{
				CreateBuffer();
				int sizeFromGpu;
				GL.BindBuffer(GL_BufferTarget, VboHandle); My.Check();
				var arr = this.ToArray();
				var size = NumberOfElements * DataSizeOfOneElementInBytes;
				GL.BufferData(GL_BufferTarget, size, arr, BufferUsageHint.StaticDraw); My.Check(); // BufferUsageHint explained: http://www.informit.com/articles/article.aspx?p=1377833&seqNum=7
				GL.GetBufferParameter(GL_BufferTarget, BufferParameterName.BufferSize, out sizeFromGpu); My.Check();
				// if (size != sizeFromGpu) Debug.Error(myName + " size mismatch size=" + GL_BufferTarget + " sizeFromGpu=" + sizeFromGpu);
				GL.BindBuffer(GL_BufferTarget, 0);
			}

			public void BindBufferToVAO()
			{
				if (UsesLayoutIndex)
				{
					GL.EnableVertexAttribArray(LayoutIndex); My.Check();
				}
				GL.BindBuffer(GL_BufferTarget, VboHandle); My.Check();
				if (UsesLayoutIndex)
				{
					//GL.VertexAttribPointer(Shader.positionLocation, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0); My.Check();
					GL.VertexAttribPointer(LayoutIndex, DataStrideInElementsNumber, GL_PointerType, false, DataSizeOfOneElementInBytes, offset); My.Check();
				}
			}

			public void DeleteBuffer()
			{
				if (VboHandle != -1)
				{
					GL.DeleteBuffer(VboHandle); My.Check();
				}
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
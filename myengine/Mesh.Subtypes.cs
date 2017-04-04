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
	public partial class Mesh
	{
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
					VaoHandle = GL.GenVertexArray(); MyGL.Check();
				}
				foreach (var vboName in vboNamesToBind)
				{
					var vbo = nameToVbo[vboName];
					vbo.CreateBuffer();
				}

				GL.BindVertexArray(VaoHandle); MyGL.Check();
				foreach (var vboName in vboNamesToBind)
				{
					var vbo = nameToVbo[vboName];
					vbo.BindBufferToVAO();
				}
				vboNamesToBind.Clear();
				GL.BindVertexArray(0); MyGL.Check();
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); MyGL.Check();
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0); MyGL.Check();
			}

			/// <summary>
			/// Delete buffers
			/// </summary>
			public void Dispose()
			{
				if (VaoHandle != -1)
				{
					GL.DeleteVertexArray(VaoHandle); MyGL.Check();
					VaoHandle = -1;
				}
				foreach (var kvp in nameToVbo)
				{
					kvp.Value.DeleteBuffer();
				}
			}

			public void UploadDataToGpu()
			{
				foreach (var kvp in nameToVbo)
				{
					kvp.Value.UploadDataToGPU();
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

			void UploadDataToGPU();

			void DownloadDataFromGPU();

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

			public override unsafe void SetData(IntPtr ptr, int countOfVector3s)
			{
				lock (this)
				{
					var p = (float*)ptr.ToPointer();
					for (int i = 0; i < countOfVector3s; i++)
					{
						this[i] = new Vector3(*p++, *p++, *p++);
					}
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
			public override void SetData(IntPtr ptr, int countOfVector3s)
			{
				var countOfFloats = countOfVector3s * 3;
				var data = new float[countOfFloats];
				Marshal.Copy(ptr, data, 0, countOfFloats);
				lock (this)
				{
					this.Clear();
					for (int i = 0; i < countOfFloats; i += 2)
					{
						this.Add(new Vector2(data[i], data[i + 1]));
					}
				}
			}
		}
		public class BufferObjectInt : BufferObject<int>
		{
			public BufferObjectInt()
			{
				ElementType = typeof(int);
				DataStrideInElementsNumber = 1;
			}

			public override void SetData(IntPtr ptr, int countOfVector3s)
			{
				var countOfFloats = countOfVector3s * 3;
				var data = new int[countOfFloats];
				Marshal.Copy(ptr, data, 0, countOfFloats);
				lock (this)
				{
					this.Clear();
					this.AddRange(data);
				}
			}
		}



		public abstract class BufferObject<T> : List<T>, IBufferObject where T : struct
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
					VboHandle = GL.GenBuffer(); MyGL.Check();
				}
			}

			public void UploadDataToGPU()
			{
				CreateBuffer();
				int sizeFromGpu;
				GL.BindBuffer(GL_BufferTarget, VboHandle); MyGL.Check();
				var arr = this.ToArray();
				var size = NumberOfElements * DataSizeOfOneElementInBytes;
				GL.BufferData(GL_BufferTarget, size, arr, BufferUsageHint.StreamRead); MyGL.Check(); // BufferUsageHint explained: http://www.informit.com/articles/article.aspx?p=1377833&seqNum=7
				GL.GetBufferParameter(GL_BufferTarget, BufferParameterName.BufferSize, out sizeFromGpu); MyGL.Check();
				// if (size != sizeFromGpu) Log.Error(myName + " size mismatch size=" + GL_BufferTarget + " sizeFromGpu=" + sizeFromGpu);
				GL.BindBuffer(GL_BufferTarget, 0); MyGL.Check();
			}

			public void BindBufferToVAO()
			{
				if (UsesLayoutIndex)
				{
					GL.EnableVertexAttribArray(LayoutIndex); MyGL.Check();
				}
				GL.BindBuffer(GL_BufferTarget, VboHandle); MyGL.Check();
				if (UsesLayoutIndex)
				{
					//GL.VertexAttribPointer(Shader.positionLocation, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0); My.Check();
					GL.VertexAttribPointer(LayoutIndex, DataStrideInElementsNumber, GL_PointerType, false, DataSizeOfOneElementInBytes, offset); MyGL.Check();
				}
			}

			public void DeleteBuffer()
			{
				if (VboHandle != -1)
				{
					GL.DeleteBuffer(VboHandle); MyGL.Check();
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
				Add(this[index.Index]);
			}

			public void DownloadDataFromGPU()
			{
				GL.BindBuffer(BufferTarget.ShaderStorageBuffer, VboHandle); MyGL.Check();
				//var intPtr = GL.MapBufferRange(BufferTarget.ShaderStorageBuffer, new IntPtr(), Count, BufferAccessMask.MapReadBit); MyGL.Check();
				var intPtr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly); MyGL.Check();
				SetData(intPtr, Count);
				GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer); MyGL.Check();
			}
			/*
			public void DownloadDataFromGPU(ushort splitToPartsCount, ushort partIndex)
			{
				GL.BindBuffer(BufferTarget.ShaderStorageBuffer, VboHandle); MyGL.Check();
				var intPtr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly); MyGL.Check();
				SetData(intPtr, Count);
				GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer); MyGL.Check();
			}*/
			public abstract void SetData(IntPtr intPtr, int count);

		}

	}
}

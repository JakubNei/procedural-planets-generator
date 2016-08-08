using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections;

namespace MyEngine
{
    /*
    class Test
    {
        int vertices_data_version;
        DataAccessibleList<Vector3>.Data vertices_data = new DataAccessibleList<Vector3>.Data();
        DataAccessibleList<Vector3> vertices;
        public DataAccessibleList<Vector3> Vertices { get { return vertices; } }


        int vertices_data_version;
        DataAccessibleList<Vector3>.Data vertices_data = new DataAccessibleList<Vector3>.Data();
        DataAccessibleList<Vector3> vertices;
        public DataAccessibleList<Vector3> Vertices { get { return vertices; } }


        int vertices_data_version;
        DataAccessibleList<Vector3>.Data vertices_data = new DataAccessibleList<Vector3>.Data();
        DataAccessibleList<Vector3> vertices;
        public DataAccessibleList<Vector3> Vertices { get { return vertices; } }


        int vertices_data_version;
        DataAccessibleList<Vector3>.Data vertices_data = new DataAccessibleList<Vector3>.Data();
        DataAccessibleList<Vector3> vertices;
        public DataAccessibleList<Vector3> Vertices { get { return vertices; } }

        bool HasChanged()
        {
            return 
                vertices_data._version != vertices_data_version;
        }
    }
    */


    public partial class Mesh : IDisposable
    {
        static Mesh skyBox;
        public static Mesh SkyBox
        {
            get
            {
                if (skyBox == null)
                {
                    skyBox = Factory.GetMesh("internal/skybox.obj");
                }
                return skyBox;
            }
        }

        static Mesh m_Quad;
        public static Mesh Quad
        {
            get
            {
                if (m_Quad == null)
                {
                    m_Quad = Factory.GetMesh("internal/quad.obj");
                }
                return m_Quad;
            }
        }

        public enum ChangedFlags
        {
            Bounds,
            VisualRepresentation
        }

        public VertexBufferObject<Vector3> Vertices { get; private set; }
        public VertexBufferObject<Vector3> Normals { get; private set; }
        public VertexBufferObject<Vector3> Tangents { get; private set; }
        public VertexBufferObject<Vector2> Uvs { get; private set; }
        public VertexBufferObject<int> TriangleIndicies { get; private set; }

        public Asset asset;

        public event Action<ChangedFlags> OnDataChanged;
        void RaiseOnChanged(ChangedFlags flags)
        {
            if (OnDataChanged != null) OnDataChanged(flags);
        }


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
                        RaiseOnChanged(ChangedFlags.Bounds);
                    }
                    else
                    {
                        bounds = new Bounds(Vector3.Zero, Vector3.Zero);
                        RaiseOnChanged(ChangedFlags.Bounds);
                    }
                }

                return bounds;
            }
        }
        Bounds bounds;



        public bool IsRenderable
        {
            get
            {
                return true;
            }
        }


        bool isOnGPU = false;

        public VertexArrayObject VertexArrayObj { get; private set; }

        public Mesh()
        {
            Vertices = new VertexBufferObject<Vector3>()
            {
                ElementType = typeof(float),
                DataStrideInElementsNumber = 3,
            };
            Normals = new VertexBufferObject<Vector3>()
            {
                ElementType = typeof(float),
                DataStrideInElementsNumber = 3,
            };
            Tangents = new VertexBufferObject<Vector3>()
            {
                ElementType = typeof(float),
                DataStrideInElementsNumber = 3,
            };
            Uvs = new VertexBufferObject<Vector2>()
            {
                ElementType = typeof(float),
                DataStrideInElementsNumber = 2,
            };
            TriangleIndicies = new VertexBufferObject<int>()
            {
                Target = BufTarget.ControlElementArray,
                ElementType = typeof(int),
                DataStrideInElementsNumber = 1,
            };

            VertexArrayObj = new VertexArrayObject();
            VertexArrayObj.AddVertexBufferObject("vertices", Vertices);
            VertexArrayObj.AddVertexBufferObject("normals", Normals);
            VertexArrayObj.AddVertexBufferObject("tangents", Tangents);
            VertexArrayObj.AddVertexBufferObject("uvs", Uvs);
            VertexArrayObj.AddVertexBufferObject("triangleIndicies", TriangleIndicies);
            VertexArrayObj.OnChanged += () => { isOnGPU = false; };
        }

        public void RecalculateBounds()
        {
            recalculateBounds = true;
        }

        public void Draw()
        {
            if (isOnGPU == false)
            {
                UploadDataToGpu();
            }
            GL.BindVertexArray(VertexArrayObj.handle);
            GL.DrawElements(PrimitiveType.Triangles, TriangleIndicies.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        public void UploadDataToGpu()
        {
            if (!HasNormals())
            {
                RecalculateNormals();
            }
            if (!HasTangents())
            {
                RecalculateTangents();
            }

            VertexArrayObj.Dispose();
            VertexArrayObj.CreateBufferAndBindVBOs();
            VertexArrayObj.SendDataToGpu();

            isOnGPU = true;
            RaiseOnChanged(ChangedFlags.VisualRepresentation);
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
            return Uvs != null && Vertices.Count == Uvs.Count;
        }



        public void Dispose()
        {
            VertexArrayObj.Dispose();
        }


        public class VertexArrayObject : IDisposable
        {
            public event Action OnChanged;

            public Dictionary<string, IVertexBufferObject> nameToVbo = new Dictionary<string, IVertexBufferObject>();
            List<string> vboNamesToBind = new List<string>();

            public int nextLayoutIndex = 0;

            public int handle = -1;

            public void AddVertexBufferObject(string name, IVertexBufferObject vbo)
            {
                if (nameToVbo.ContainsKey(name) && nameToVbo[name] == vbo) return;
                nameToVbo[name] = vbo;
                if (vbo.UsesLayoutIndex)
                {
                    vbo.LayoutIndex = nextLayoutIndex;
                    nextLayoutIndex++;
                }
                vboNamesToBind.Add(name);
                vbo.OnChanged += () => { OnChanged.Raise(); };
                OnChanged.Raise();
            }
            public void CreateBufferAndBindVBOs()
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
        }
        public enum BufTarget
        {
            Array,
            ControlElementArray,
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
                if (size != sizeFromGpu) Debug.Error(myName + " size mismatch size=" + GL_BufferTarget + " sizeFromGpu=" + sizeFromGpu);
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
                OnChanged.Raise();
            }
            public void SetData(T[] data)
            {
                this.Clear();
                this.AddRange(data);
                OnChanged.Raise();
            }
            public void SetData(IEnumerable<T> data)
            {
                this.Clear();
                this.AddRange(data);
                OnChanged.Raise();
            }
        }



    }
}
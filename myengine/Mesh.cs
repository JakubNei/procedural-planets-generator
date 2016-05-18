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


    public class Mesh : IDisposable
    {
        static Mesh m_SkyBox;
        public static Mesh SkyBox
        {
            get
            {
                if(m_SkyBox == null)
                {
                    m_SkyBox = Factory.GetMesh("internal/skybox.obj");
                }
                return m_SkyBox;
            }
        }

        static Mesh m_Quad;
        public static Mesh Quad
        {
            get
            {
                if(m_Quad == null)
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
        public VertexBufferObject<Vector3> normals { get; private set; }
        public VertexBufferObject<Vector3> tangents { get; private set; }
        public VertexBufferObject<Vector2> uvs { get; private set; }
        public VertexBufferObject<int> triangleIndicies { get; private set; }

        public Asset asset;

        public event Action<ChangedFlags> OnChanged;
        void RaiseOnChanged(ChangedFlags flags)
        {
            if (OnChanged != null) OnChanged(flags);
        }


        bool recalculateBounds = true;

        /// <summary>
        /// Local space bounds of the mesh.
        /// </summary>
        public Bounds bounds
        {
            get
            {
                if (recalculateBounds)
                {
                    recalculateBounds = false;
                    if (Vertices.Count > 0)
                    {
                        _bounds = new Bounds(Vertices[0], Vector3.Zero);
                        foreach (var point in Vertices)
                        {
                            _bounds.Encapsulate(point);
                        }
                        RaiseOnChanged(ChangedFlags.Bounds);
                    }
                    else
                    {
                        _bounds = new Bounds(Vector3.Zero, Vector3.Zero);
                        RaiseOnChanged(ChangedFlags.Bounds);
                    }
                }

                return _bounds;
            }
        }
        Bounds _bounds;



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
            normals = new VertexBufferObject<Vector3>()
            {
                ElementType = typeof(float),
                DataStrideInElementsNumber = 3,
            };
            tangents = new VertexBufferObject<Vector3>()
            {
                ElementType = typeof(float),
                DataStrideInElementsNumber = 3,
            };
            uvs = new VertexBufferObject<Vector2>()
            {
                ElementType = typeof(float),
                DataStrideInElementsNumber = 2,
            };
            triangleIndicies = new VertexBufferObject<int>()
            {
                Target = BufTarget.ControlElementArray,
                ElementType = typeof(int),
                DataStrideInElementsNumber = 1,
            };

            VertexArrayObj = new VertexArrayObject();
            VertexArrayObj.AddVertexBufferObject("vertices", Vertices);
            VertexArrayObj.AddVertexBufferObject("normals", normals);
            VertexArrayObj.AddVertexBufferObject("tangents", tangents);
            VertexArrayObj.AddVertexBufferObject("uvs", uvs);
            VertexArrayObj.AddVertexBufferObject("triangleIndicies", triangleIndicies);
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
            GL.DrawElements(PrimitiveType.Triangles, triangleIndicies.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
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
            return tangents != null && Vertices.Count == tangents.Count;
        }
        public bool HasNormals()
        {
            return normals != null && Vertices.Count == normals.Count;
        }
        public bool HasUVs()
        {
            return uvs != null && Vertices.Count == uvs.Count;
        }


        public static void CalculateNormals(IList<int> inTriangleIndicies, IList<Vector3> inPositions, IList<Vector3> outNormals)
        {
            int verticesNum = inPositions.Count;
            int indiciesNum = inTriangleIndicies.Count;


            int[] counts = new int[verticesNum];

            outNormals.Clear();
            if (outNormals is List<Vector3>)
            {
                (outNormals as List<Vector3>).Capacity = verticesNum;
            }

            for (int i = 0; i < verticesNum; i++)
            {
                outNormals.Add(Vector3.Zero);
            }

            for (int i = 0; i <= indiciesNum - 3; i += 3)
            {

                int ai = inTriangleIndicies[i];
                int bi = inTriangleIndicies[i + 1];
                int ci = inTriangleIndicies[i + 2];

                if (ai < verticesNum && bi < verticesNum && ci < verticesNum)
                {
                    Vector3 av = inPositions[ai];
                    Vector3 n = Vector3.Normalize(Vector3.Cross(
                        inPositions[bi] - av,
                        inPositions[ci] - av
                    ));

                    outNormals[ai] += n;
                    outNormals[bi] += n;
                    outNormals[ci] += n;

                    counts[ai]++;
                    counts[bi]++;
                    counts[ci]++;
                }
            }

            for (int i = 0; i < verticesNum; i++)
            {
                outNormals[i] /= counts[i];
            }


        }
        public void RecalculateNormals()
        {
            CalculateNormals(triangleIndicies, Vertices, normals);
        }

        public void RecalculateTangents()
        {
            if (HasUVs() == false) uvs.Resize(Vertices.Count);

            //partialy stolen from http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-13-normal-mapping/

            int verticesNum = Vertices.Count;
            int indiciesNum = triangleIndicies.Count;

            int[] counts = new int[verticesNum];

            tangents.Clear();
            tangents.Capacity = verticesNum;

            for (int i = 0; i < verticesNum; i++)
            {
                counts[i] = 0;
                tangents.Add(Vector3.Zero);
            }

            for (int i = 0; i <= indiciesNum - 3; i += 3)
            {

                int ai = triangleIndicies[i];
                int bi = triangleIndicies[i + 1];
                int ci = triangleIndicies[i + 2];

                if (ai < verticesNum && bi < verticesNum && ci < verticesNum)
                {
                    Vector3 av = Vertices[ai];
                    Vector3 deltaPos1 = Vertices[bi] - av;
                    Vector3 deltaPos2 = Vertices[ci] - av;

                    Vector2 auv = uvs[ai];
                    Vector2 deltaUV1 = uvs[bi] - auv;
                    Vector2 deltaUV2 = uvs[ci] - auv;

                    float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                    Vector3 t = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;


                    tangents[ai] += t;
                    tangents[bi] += t;
                    tangents[ci] += t;

                    counts[ai]++;
                    counts[bi]++;
                    counts[ci]++;
                }
            }

            for (int i = 0; i < verticesNum; i++)
            {
                tangents[i] /= counts[i];
            }


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
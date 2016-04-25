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

    public interface IMesh
    {
        void Draw();
        void UploadDataToGpu();
    }

    public class Mesh : IUnloadable, IMesh
    {
        public enum ChangedFlags
        {
            Bounds,
            VisualRepresentation
        }

        public VertexBufferObject<Vector3> vertices { get; private set; }
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
                    if (vertices.Count > 0)
                    {
                        _bounds = new Bounds(vertices[0], Vector3.Zero);
                        foreach (var point in vertices)
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
            vertices = new VertexBufferObject<Vector3>()
            {
                bufferTarget = BufferTarget.ArrayBuffer,
                pointerType = VertexAttribPointerType.Float,
                dataStrideInElementsNumber = 3,
            };
            normals = new VertexBufferObject<Vector3>()
            {
                bufferTarget = BufferTarget.ArrayBuffer,
                pointerType = VertexAttribPointerType.Float,
                dataStrideInElementsNumber = 3,
            };
            tangents = new VertexBufferObject<Vector3>()
            {
                bufferTarget = BufferTarget.ArrayBuffer,
                pointerType = VertexAttribPointerType.Float,
                dataStrideInElementsNumber = 3,
            };
            uvs = new VertexBufferObject<Vector2>()
            {
                bufferTarget = BufferTarget.ArrayBuffer,
                pointerType = VertexAttribPointerType.Float,
                dataStrideInElementsNumber = 2,
            };
            triangleIndicies = new VertexBufferObject<int>()
            {
                bufferTarget = BufferTarget.ElementArrayBuffer,
                pointerType = VertexAttribPointerType.Float,
                dataStrideInElementsNumber = 1,
            };

            VertexArrayObj = new VertexArrayObject();
            VertexArrayObj.AddVertexBufferObject("vertices", vertices);
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

            VertexArrayObj.DeleteBuffer();
            VertexArrayObj.CreateBufferAndBindVBOs();
            VertexArrayObj.SendDataToGpu();

            isOnGPU = true;
            RaiseOnChanged(ChangedFlags.VisualRepresentation);
        }

        public bool HasTangents()
        {
            return tangents != null && vertices.Count == tangents.Count;
        }
        public bool HasNormals()
        {
            return normals != null && vertices.Count == normals.Count;
        }
        public bool HasUVs()
        {
            return uvs != null && vertices.Count == uvs.Count;
        }


        public void RecalculateNormals()
        {
            if (HasNormals() == false) normals.Resize(vertices.Count);

            int verticesNum = vertices.Count;
            int indiciesNum = triangleIndicies.Count;


            int[] counts = new int[verticesNum];

            for (int i = 0; i < verticesNum; i++)
            {
                counts[i] = 0;
            }

            for (int i = 0; i < indiciesNum - 3; i += 3)
            {

                int ai = triangleIndicies[i];
                int bi = triangleIndicies[i + 1];
                int ci = triangleIndicies[i + 2];

                if (ai < verticesNum && bi < verticesNum && ci < verticesNum)
                {
                    Vector3 av = vertices[ai];
                    Vector3 n = Vector3.Normalize(Vector3.Cross(
                        vertices[bi] - av,
                        vertices[ci] - av
                    ));

                    normals[ai] += n;
                    normals[bi] += n;
                    normals[ci] += n;

                    counts[ai]++;
                    counts[bi]++;
                    counts[ci]++;
                }
            }

            for (int i = 0; i < verticesNum; i++)
            {
                normals[i] /= counts[i];
            }



        }

        public void RecalculateTangents()
        {
            if (HasTangents() == false) tangents.Resize(vertices.Count);
            if (HasUVs() == false) uvs.Resize(vertices.Count);

            //partialy stolen from http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-13-normal-mapping/

            int verticesNum = vertices.Count;
            int indiciesNum = triangleIndicies.Count;

            int[] counts = new int[verticesNum];

            for (int i = 0; i < verticesNum; i++)
            {
                counts[i] = 0;
            }

            for (int i = 0; i < indiciesNum - 3; i += 3)
            {

                int ai = triangleIndicies[i];
                int bi = triangleIndicies[i + 1];
                int ci = triangleIndicies[i + 2];

                if (ai < verticesNum && bi < verticesNum && ci < verticesNum)
                {
                    Vector3 av = vertices[ai];
                    Vector3 deltaPos1 = vertices[bi] - av;
                    Vector3 deltaPos2 = vertices[ci] - av;

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



        public void Unload()
        {
            VertexArrayObj.DeleteBuffer();
        }


        public class VertexArrayObject
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
            public void DeleteBuffer()
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
        public class VertexBufferObject<T> : List<T>, IVertexBufferObject where T : struct
        {
            public event Action OnChanged;
            public int Handle { get; set; }
            public int LayoutIndex { get; set; }

            public BufferTarget bufferTarget;
            public int NumberOfElements { get { return this.Count; } }
            public VertexAttribPointerType pointerType;
            public bool normalized = false;
            public int dataStrideInElementsNumber;
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
                    return bufferTarget != BufferTarget.ElementArrayBuffer;
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
                GL.BindBuffer(bufferTarget, Handle);
                var arr = this.ToArray();
                var size = NumberOfElements * DataSizeOfOneElementInBytes;
                GL.BufferData(bufferTarget, (IntPtr)(size), arr, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(bufferTarget, BufferParameterName.BufferSize, out sizeFromGpu);
                if (size != sizeFromGpu) Debug.Error(myName+" size mismatch size=" + bufferTarget + " sizeFromGpu=" + sizeFromGpu);
            }
            public void BindBufferToVAO()
            {
                CreateBuffer();
                if (UsesLayoutIndex)
                {
                    GL.EnableVertexAttribArray(LayoutIndex);
                }
                GL.BindBuffer(bufferTarget, Handle);
                if (UsesLayoutIndex)
                {
                    //GL.VertexAttribPointer(Shader.positionLocation, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
                    GL.VertexAttribPointer(LayoutIndex, dataStrideInElementsNumber, pointerType, normalized, DataSizeOfOneElementInBytes, offset);
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
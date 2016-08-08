using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine.Mathematical
{
    public class SoftwareOcclusionCuller
    {
        int width;
        int height;

        float[,] buffer;

        Matrix4 transformMatrix;

        /// <summary>
        /// Screen is split into tiles from left to right.
        /// </summary>
        public class Tile
        {

        }
        List<Tile> tiles = new List<Tile>();

        public void Init()
        {
            buffer = new float[width, height];
        }

        public void Add(Mesh mesh)
        {
            var indicies = mesh.TriangleIndicies;
            var vertices = mesh.Vertices;

            for(int i=0; i<indicies.Count; i+=3)
            {
                var a = Vector3.TransformVector(vertices[indicies[i + 0]], transformMatrix);
                var b = Vector3.TransformVector(vertices[indicies[i + 1]], transformMatrix);
                var c = Vector3.TransformVector(vertices[indicies[i + 2]], transformMatrix);
            }

        }
    }
}

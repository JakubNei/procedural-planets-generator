using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine.Components
{

    [Flags]
    public enum RenderingMode
    {
        DontRender = 0,
        RenderGeometry = (1 << 1),
        CastShadows = (1 << 2),
        RenderGeometryAndCastShadows = (1 << 1) | (1 << 2),
    }

    public class MeshRenderer : Renderer
    {
        public override bool ShouldRenderGeometry
        {
            get
            {
                return base.ShouldRenderGeometry && Mesh != null && Mesh.IsRenderable && Material != null && Material.GBufferShader != null;
            }
        }

        public override bool ShouldCastShadows
        {
            get
            {
                return base.ShouldCastShadows && Mesh != null && Mesh.IsRenderable && Material != null && Material.DepthGrabShader != null;
            }
        }



    


        Mesh m_mesh;
        public Mesh Mesh
        {
            set
            {
                if (m_mesh != value)
                {
                    if (m_mesh != null) m_mesh.OnChanged -= OnMeshHasChanged;
                    m_mesh = value;
                    if (m_mesh != null) m_mesh.OnChanged += OnMeshHasChanged;
                    ShouldRenderGeometryOrShouldCastShadowsHasChanged();
                }
            }
            get
            {
                return m_mesh;
            }
        }



        static readonly Vector3[] extentsTransformsToEdges = {
                                                                 new Vector3( 1, 1, 1),
                                                                 new Vector3( 1, 1,-1),
                                                                 new Vector3( 1,-1, 1),
                                                                 new Vector3( 1,-1,-1),
                                                                 new Vector3(-1, 1, 1),
                                                                 new Vector3(-1, 1,-1),
                                                                 new Vector3(-1,-1, 1),
                                                                 new Vector3(-1,-1,-1),
                                                             };

        public MeshRenderer(Entity entity) : base(entity)
        {
            Material = new Material();
        }


        public override Bounds GetBounds(WorldPos viewPointPos)
        {
            var relativePos = viewPointPos.Towards(Entity.Transform.Position).ToVector3();
            if (Mesh == null)
            {
                return new Bounds(relativePos, Vector3.Zero);
            }

            var boundsCenter = relativePos + (Mesh.bounds.Center * Entity.Transform.Scale).RotateBy(Entity.Transform.Rotation);
            var boundsExtents = (Mesh.bounds.Extents * Entity.Transform.Scale).RotateBy(Entity.Transform.Rotation);

            var bounds = new Bounds(boundsCenter, Vector3.Zero);
            for (int i = 0; i < 8; i++)
            {
                bounds.Encapsulate(boundsCenter + boundsExtents.CompomentWiseMult(extentsTransformsToEdges[i]));
            }
            return bounds;
        }


        public override void UploadUBOandDraw(Camera camera, UniformBlock ubo)
        {
            var modelMat = this.Entity.Transform.GetLocalToWorldMatrix(camera.Transform.Position);
            var modelViewMat = modelMat * camera.GetRotationMatrix();
            ubo.model.modelMatrix = modelMat;
            ubo.model.modelViewMatrix = modelViewMat;
            ubo.model.modelViewProjectionMatrix = modelViewMat * camera.GetProjectionMat();
            ubo.modelUBO.UploadData();
            Mesh.Draw();
        }

        void OnMeshHasChanged(Mesh.ChangedFlags flags)
        {
            ShouldRenderGeometryOrShouldCastShadowsHasChanged();
        }

    }
}
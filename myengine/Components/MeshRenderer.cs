using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine.Components
{
    public class MeshRenderer : Renderer
    {
        public override bool ShouldRenderGeometry { get { return base.ShouldRenderGeometry && Material.gBufferShader != null; } }
        public override bool ShouldCastShadows { get { return base.ShouldCastShadows && Material.depthGrabShader != null; } }


        Mesh m_mesh;
        public Mesh Mesh
        {
            set
            {
                m_mesh = value;
                m_mesh.OnChanged += OnMeshHasChanges;
                Entity.RaiseOnChanged(ChangedFlags.VisualRepresentation);
            }
            get
            {
                m_mesh.OnChanged -= OnMeshHasChanges;
                return m_mesh;
            }
        }
        MaterialPBR m_material;
        public new MaterialPBR Material
        {
            set
            {
                base.material = value;
                m_material = value;
                Entity.RaiseOnChanged(ChangedFlags.VisualRepresentation);
            }

            get
            {
                return m_material;
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
            Material = new MaterialPBR()
            {
                gBufferShader = Factory.GetShader("internal/deferred.gBuffer.standart.shader"),
                depthGrabShader = Factory.GetShader("internal/depthGrab.standart.shader"),
            };
        }

        /// <summary>
        /// World space bounds of the associatd mesh and transform.
        /// </summary>
        public override Bounds bounds
        {
            get
            {
                if(Mesh == null)
                {
                    return new Bounds(Entity.Transform.Position, Vector3.Zero);
                }

                var boundsPos = Entity.Transform.Position + (Mesh.bounds.center * Entity.Transform.Scale).RotateBy(Entity.Transform.Rotation);
                var boundsExtents = (Mesh.bounds.Extents * Entity.Transform.Scale).RotateBy(Entity.Transform.Rotation);

                var bounds = new Bounds(boundsPos, Vector3.Zero);
                for (int i = 0; i < 8; i++)
                {
                    bounds.Encapsulate(boundsPos + boundsExtents.CompomentWiseMult(extentsTransformsToEdges[i]));
                }
                return bounds;

                /*var bounds = mesh.bounds;
                bounds.center = bounds.center * transform.scale + transform.position;
                bounds.extents *= transform.scale;
                return bounds;*/
            }
        }


        override internal void UploadUBOandDraw(Camera camera, UniformBlock ubo)
        {
            var modelMat = this.Entity.Transform.LocalToWorldMatrix;
            var modelViewMat = modelMat * camera.GetViewMat();
            ubo.model.modelMatrix = modelMat;
            ubo.model.modelViewMatrix = modelViewMat;
            ubo.model.modelViewProjectionMatrix = modelViewMat * camera.GetProjectionMat();
            ubo.modelUBO.UploadData();
            Mesh.Draw();
        }

        void OnMeshHasChanges(ChangedFlags flags)
        {
            Entity.RaiseOnChanged(flags);
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine.Components
{
    public enum RenderStatus
    {
        NotRendered = 0,
        Rendered = (1 << 1),
        RenderedForced = (1 << 2),
        Visible = (1 << 3),
        RenderedAndVisible = (1 << 1) | (1 << 3),
        Unknown = (1 << 9),
    }
    public interface IRenderable
    {
        Material Material { get; }
        bool ForcePassFrustumCulling { get; }
        bool ShouldRenderGeometry { get; }
        bool ShouldCastShadows { get; }
        Bounds GetBounds(WorldPos viewPointPos);
        void UploadUBOandDraw(Camera camera, UniformBlock ubo);
        void SetCameraRenderStatus(Camera camera, RenderStatus renderStatus);

        /*
        bool ShouldRenderGeometry { get; }
        bool ShouldCastShadows { get; }
        Material Material { get; }
        */
    }

    public abstract class Renderer : Component, IRenderable
    {
        public virtual bool ShouldRenderGeometry
        {
            get
            {
                return RenderingMode.HasFlag(RenderingMode.RenderGeometry);
            }
        }

        public virtual bool ShouldCastShadows
        {
            get
            {
                return RenderingMode.HasFlag(RenderingMode.CastShadows);
            }
        }


        RenderingMode m_RenderingMode = RenderingMode.RenderGeometryAndCastShadows;
        public virtual RenderingMode RenderingMode
        {
            get
            {
                return m_RenderingMode;
            }
            set
            {
                if (m_RenderingMode != value)
                {
                    m_RenderingMode = value;
                    ShouldRenderGeometryOrShouldCastShadowsHasChanged();
                }

            }
        }

        Material m_material;
        public virtual Material Material
        {
            set
            {
                lock (this)
                {
                    if (m_material != value)
                    {
                        m_material = value;
                        ShouldRenderGeometryOrShouldCastShadowsHasChanged();
                    }
                }
            }
            get
            {
                lock (this)
                {
                    return m_material;
                }
            }
        }


        Dictionary<Camera, RenderStatus> cameraToRenderStatus = new Dictionary<Camera, RenderStatus>();

        public virtual bool ForcePassFrustumCulling { get; set; }

        bool last_ShouldRenderGeometry = false;
        bool last_ShouldCastShadows = false;


        public Renderer(Entity entity) : base(entity)
        {
            ShouldRenderGeometryOrShouldCastShadowsHasChanged();
        }
        public abstract Bounds GetBounds(WorldPos viewPointPos);

        public virtual void UploadUBOandDraw(Camera camera, UniformBlock ubo)
        {
        }

        public virtual void SetCameraRenderStatus(Camera camera, RenderStatus renderStatus)
        {
            cameraToRenderStatus[camera] = renderStatus;
        }
        public virtual RenderStatus GetCameraRenderStatus(Camera camera)
        {
            return cameraToRenderStatus.GetValue(camera, RenderStatus.Unknown);
        }


        protected virtual void ShouldRenderGeometryOrShouldCastShadowsHasChanged()
        {
            if (last_ShouldRenderGeometry != ShouldRenderGeometry)
            {
                if (last_ShouldRenderGeometry) Entity.Scene.DataToRender.RemoveGeometry(this);
                if (ShouldRenderGeometry) Entity.Scene.DataToRender.AddGeometry(this);

                last_ShouldRenderGeometry = ShouldRenderGeometry;
            }

            if (last_ShouldCastShadows != ShouldCastShadows)
            {
                if (last_ShouldCastShadows) Entity.Scene.DataToRender.RemoveShadowCaster(this);
                if (ShouldCastShadows) Entity.Scene.DataToRender.AddShadowCaster(this);

                last_ShouldCastShadows = ShouldCastShadows;
            }
        }

    }
}
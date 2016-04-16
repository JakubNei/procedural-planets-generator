using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine.Components
{
    public abstract class Renderer : Component
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

        public virtual Bounds bounds { set; get; }
        public virtual bool AllowsFrustumCulling { get; set; }

        bool last_ShouldRenderGeometry = false;
        bool last_ShouldCastShadows = false;

        public Renderer(Entity entity) : base(entity)
        {
            AllowsFrustumCulling = true;
            ShouldRenderGeometryOrShouldCastShadowsHasChanged();
        }

        public virtual void UploadUBOandDraw(Camera camera, UniformBlock ubo)
        {
        }


        protected virtual void ShouldRenderGeometryOrShouldCastShadowsHasChanged()
        {
            if (last_ShouldRenderGeometry != ShouldRenderGeometry)
            {
                if (last_ShouldRenderGeometry) Entity.Scene.RemoveGeometry(this);
                if (ShouldRenderGeometry) Entity.Scene.AddGeometry(this);

                last_ShouldRenderGeometry = ShouldRenderGeometry;
            }

            if (last_ShouldCastShadows != ShouldCastShadows)
            {
                if (last_ShouldCastShadows) Entity.Scene.RemoveShadowCaster(this);
                if (ShouldCastShadows) Entity.Scene.AddShadowCaster(this);

                last_ShouldCastShadows = ShouldCastShadows;
            }
        }

    }
}
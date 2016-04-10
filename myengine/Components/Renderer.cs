using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine.Components
{
    public abstract class Renderer : Component
    {

        public virtual bool ShouldRenderGeometry { get; set; }
        public virtual bool ShouldCastShadows { get; set; }

        public Material material;
        public virtual Bounds bounds { set; get; }
        public bool CanBeFrustumCulled = true;

        public Renderer(Entity entity) : base(entity)
        {
            ShouldRenderGeometry = true;
            ShouldCastShadows = true;
        }

        public void HideIn(float seconds)
        {
            ShouldRenderGeometry = false;
            ShouldCastShadows = false;
        }
        public void ShowIn(float seconds)
        {
            ShouldRenderGeometry = true;
            ShouldCastShadows = true;
        }
        public float GetVisibility()
        {
            return ShouldRenderGeometry ? 1 : 0;
        }

        virtual internal void UploadUBOandDraw(Camera camera, UniformBlock ubo)
        {
        }
    }
}
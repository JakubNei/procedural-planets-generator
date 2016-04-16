using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine
{
    public class Material
    {
        Shader m_gBufferShader;
        public virtual Shader GBufferShader
        {
            get
            {
                return m_gBufferShader;
            }
            set
            {
                //if (value == null) throw new NullReferenceException("can not set " + MemberName.For(() => GBufferShader) + " to null");
                m_gBufferShader = value;
            }
        }
        Shader m_depthGrabShader;
        public virtual Shader DepthGrabShader
        {
            get
            {
                return m_depthGrabShader;
            }
            set
            {
                if (value == null) throw new NullReferenceException("can not set " + MemberName.For(() => DepthGrabShader) + " to null");
                m_depthGrabShader = value;
            }
        }

        public virtual UniformsManager Uniforms { get; private set; }

        public Material()
        {
            //if (GBufferShader == null) GBufferShader = Shader.DefaultGBufferShader; //BUG: if you uncoment this line planet chunks materials will suddenly delayed randomly be assigned the shader
            if (DepthGrabShader == null) DepthGrabShader = Shader.DefaultDepthGrabShader;
            this.Uniforms = new UniformsManager();
        }

        public virtual void BeforeBindCallback()
        {

        }

        public virtual Material MakeCopy()
        {
            //var m = new Material()
            //{
            //    GBufferShader = GBufferShader,
            //    DepthGrabShader = DepthGrabShader,
            //};

            var m = new Material();
            m.GBufferShader = GBufferShader;
            m.DepthGrabShader = DepthGrabShader;
            Uniforms.SendAllUniformsTo(m.Uniforms);
            return m;
        }

    }
}

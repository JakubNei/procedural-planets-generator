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
        Shader gBufferShader;
        public Shader GBufferShader
        {
            get
            {
                return gBufferShader;
            }
            set
            {
                if (value == null) throw new NullReferenceException("can not set " + MemberName.For(() => GBufferShader) + " to null");
                gBufferShader = value;
            }
        }
        Shader depthGrabShader;
        public Shader DepthGrabShader
        {
            get
            {
                return depthGrabShader;
            }
            set
            {
                if (value == null) throw new NullReferenceException("can not set " + MemberName.For(() => DepthGrabShader) + " to null");
                depthGrabShader = value;
            }
        }

        public UniformsManager Uniforms { get; private set; }

        public Material()
        {
            if (gBufferShader == null) gBufferShader = Shader.DefaultGBufferShader;
            if (depthGrabShader == null) depthGrabShader = Shader.DefaultDepthGrabShader;
            this.Uniforms = new UniformsManager();
        }

        public Material MakeCopy()
        {
            var m = new Material()
            {
                gBufferShader = gBufferShader,
                depthGrabShader = depthGrabShader,
            };
            Uniforms.SendAllUniformsTo(m.Uniforms);
            return m;
        }
        
    }
}

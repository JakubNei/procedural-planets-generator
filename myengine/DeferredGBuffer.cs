using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace MyEngine
{
    internal class DeferredGBuffer : IUnloadable
    {
        internal int frameBufferObjectHandle;

        internal Texture2D[] textures;
        bool readFirstFinalTexture = true;
        internal Texture2D finalTextureToRead { get { if (readFirstFinalTexture) return finalTexture1; else return finalTexture2; } }
        internal Texture2D finalTextureToWriteTo { get { if (!readFirstFinalTexture) return finalTexture1; else return finalTexture2; } }
        internal Texture2D finalTexture1 { get { return textures[4]; } }
        internal Texture2D finalTexture2 { get { return textures[5]; } }

        internal Texture2D depthTexture;

        DrawBuffersEnum[] buffers;
        enum GBufferTextures
        {
            Albedo = 0,
            Position = 1,
            Normal = 2,
            Data = 3,
        }
        int width;
        int height;

      
        public DeferredGBuffer(int width, int height)
        {
            this.width = width;
            this.height = height;

            // create frame buffer object
            frameBufferObjectHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObjectHandle);

            var texturesNum = System.Enum.GetValues(typeof(GBufferTextures)).Length + 2;
            int[] textureHandles = new int[texturesNum];
            textures = new Texture2D[texturesNum];

            // create textures for fbo
            GL.GenTextures(textureHandles.Length, textureHandles);

            List<DrawBuffersEnum> bufs = new List<DrawBuffersEnum>();
            for(int i=0; i<textureHandles.Length; i++)
            {
                var t = new Texture2D(textureHandles[i]);
                textures[i] = t;
                GL.BindTexture(TextureTarget.Texture2D, t.GetNativeTextureID());
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr(0));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
                GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, t.GetNativeTextureID(), 0);
                bufs.Add(DrawBuffersEnum.ColorAttachment0 + i);
            }

            depthTexture = new Texture2D(GL.GenTexture());
            GL.BindTexture(TextureTarget.Texture2D, depthTexture.GetNativeTextureID());
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, new IntPtr(0));
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture.GetNativeTextureID(), 0);
            /*
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth32fStencil8, width, height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, new IntPtr(0));
            GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, depthTexture.GetNativeTextureID(), 0);
            */

            buffers = bufs.ToArray();

            var status = GL.CheckFramebufferStatus(FramebufferTarget.DrawFramebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete) Debug.Error(status);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0); //restore default FBO
        }
        public void Unload()
        {
            GL.DeleteFramebuffer(frameBufferObjectHandle);
            depthTexture.Unload();
            foreach (var t in textures) t.Unload();
        }

        internal void BindAllFrameBuffersForDrawing()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBufferObjectHandle);
            GL.DrawBuffers(buffers.Length, buffers);
        }
        internal void BindGBufferTexturesTo(Shader shader)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.CullFace(CullFaceMode.Back);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBufferObjectHandle);

            // draw to the one we are not reading
            if(readFirstFinalTexture) GL.DrawBuffer(DrawBufferMode.ColorAttachment5);
            else GL.DrawBuffer(DrawBufferMode.ColorAttachment4);

            shader.SetUniform("gBufferUniform.depthBuffer", depthTexture);
            shader.SetUniform("gBufferUniform.final", finalTextureToRead);            

            for (int i = 0; i < textures.Length - 2; i++)
            {
                shader.SetUniform("gBufferUniform." + ((GBufferTextures)i).ToString().ToLower(), textures[i]);

                /* // !!!!! WASTED AROUND 10 HOURS, BEFOURE I FOUND THE BUG IS HERE OMFG !!!!!! old wrong code below, when used i could not make diffuse lighting in deferred lighting pass
                if (shader.SetParam("G" + enums[i].ToLower(), texturingUnit))
                {
                    var textureHandle = textures[i];
                    GL.ActiveTexture(TextureUnit.Texture0 + texturingUnit);
                    GL.BindTexture(TextureTarget.Texture2D, textureHandle);
                    texturingUnit++;
                }*/

        }

    }

        internal void SwapFinalTextureTarget()
        {
            readFirstFinalTexture = !readFirstFinalTexture;
        }

        internal void FinalDrawToScreen()
        {

        }

        internal void DebugDrawContents()
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, frameBufferObjectHandle);

            int qh = height / 4;
            int qw = width / 4;

            int x = 0;
            int y = 0;

            DrawBufferToScreen(ReadBufferMode.ColorAttachment0, qw * 0, qh * 0);
            if(readFirstFinalTexture) DrawBufferToScreen(ReadBufferMode.ColorAttachment4, qw * 1, qh * 0);
            else DrawBufferToScreen(ReadBufferMode.ColorAttachment5, qw * 1, qh * 0);
            if (readFirstFinalTexture) DrawBufferToScreen(ReadBufferMode.ColorAttachment5, qw * 2, qh * 0);
            else DrawBufferToScreen(ReadBufferMode.ColorAttachment4, qw * 2, qh * 0);
            DrawBufferToScreen(ReadBufferMode.ColorAttachment1, qw * 0, qh * 1);
            DrawBufferToScreen(ReadBufferMode.ColorAttachment2, qw * 1, qh * 1);
            DrawBufferToScreen(ReadBufferMode.ColorAttachment3, qw * 0, qh * 2);

        }
        void DrawBufferToScreen(ReadBufferMode buffer, int x, int y)
        {
            int qh = height / 4;
            int qw = width / 4;
            GL.ReadBuffer(buffer);
            GL.BlitFramebuffer(0, 0, width, height, x, y, x+qw, y+qh, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
        }

    }
}

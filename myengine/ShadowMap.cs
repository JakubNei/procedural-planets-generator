using MyEngine.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
	// http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/
	public class ShadowMap : IDisposable
	{
		public int FrameBufferObjectHandle { get; private set; }
		public Camera ShadowViewCamera { get; set; }
		public Texture2D DepthMap { get; set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		private Light light;

		public ShadowMap(Light light, int width, int height)
		{
			this.light = light;

			ShadowViewCamera = light.Entity.AddComponent<Camera>();
			ShadowViewCamera.SetSize(width, height);
			ShadowViewCamera.Orthographic = true;
			ShadowViewCamera.OrthographicSize = 50;

			this.Width = width;
			this.Height = height;

			// create frame buffer object
			FrameBufferObjectHandle = GL.GenFramebuffer(); MyGL.Check();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferObjectHandle); MyGL.Check();

			DepthMap = new Texture2D(GL.GenTexture());

			GL.BindTexture(TextureTarget.Texture2D, DepthMap.GetNativeTextureID()); MyGL.Check();

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, new IntPtr(0)); MyGL.Check();

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear); MyGL.Check();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear); MyGL.Check();
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp); MyGL.Check();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp); MyGL.Check();

			// breaks it, but should enable hardware 4 pcf sampling
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRToTexture);
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.DepthTextureMode, (int)All.Intensity);

			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, DepthMap.GetNativeTextureID(), 0); MyGL.Check();
			GL.DrawBuffer(DrawBufferMode.None); MyGL.Check();
			var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer); MyGL.Check();
			if (status != FramebufferErrorCode.FramebufferComplete) throw new GLError(status);

			GL.ReadBuffer(ReadBufferMode.None); MyGL.Check();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); MyGL.Check(); //restore default FBO
		}

		public void Dispose()
		{
			GL.DeleteFramebuffer(FrameBufferObjectHandle); MyGL.Check();
			DepthMap.Dispose();
		}

		public void Clear()
		{
			//GL.Clear(ClearBufferMask.DepthBufferBit);
			float clearDepth = 1.0f;
			GL.ClearBuffer(ClearBuffer.Depth, 0, ref clearDepth); MyGL.Check();
		}

		public void FrameBufferForWriting()
		{
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FrameBufferObjectHandle); MyGL.Check();
		}

		public void BindUniforms(Shader shader)
		{
			var shadowMatrix = this.ShadowViewCamera.GetRotationMatrix() * this.ShadowViewCamera.GetProjectionMatrix();

			// projection matrix is in range -1 1, but it is rendered into rexture which is in range 0 1
			// so lets move it from -1 1 into 0 1 range, since we are reading from texture
			shadowMatrix *= Matrix4.CreateScale(new Vector3(0.5f, 0.5f, 0.5f));
			shadowMatrix *= Matrix4.CreateTranslation(new Vector3(0.5f, 0.5f, 0.5f));

			shader.Uniforms.Set("shadowMap.level0", DepthMap);
			shader.Uniforms.Set("shadowMap.viewProjectionMatrix", shadowMatrix);
		}

		// doesnt work, depth buffer is copied into target depth buffer, so it doesnt show
		/*internal void DrawContents()
		{
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, frameBufferObjectHandle); My.Check();

			GL.ReadBuffer(ReadBufferMode.None); My.Check();
			GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest); My.Check();
		} */
	}
}
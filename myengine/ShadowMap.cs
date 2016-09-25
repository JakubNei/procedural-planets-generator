using MyEngine.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
	// http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/
	public class ShadowMap : IDisposable
	{
		public int frameBufferObjectHandle;
		public Camera shadowViewCamera;
		public Texture2D depthMap;
		int width;
		int height;
		Light light;

		[Dependency]
		Debug debug;

		ShadowMap(Light light, int width, int height)

		{
			this.light = light;

			shadowViewCamera = light.Entity.AddComponent<Camera>();
			shadowViewCamera.SetSize(width, height);
			shadowViewCamera.orthographic = true;
			shadowViewCamera.orthographicSize = 50;

			this.width = width;
			this.height = height;

			// create frame buffer object
			frameBufferObjectHandle = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObjectHandle);

			depthMap = new Texture2D(GL.GenTexture());

			GL.BindTexture(TextureTarget.Texture2D, depthMap.GetNativeTextureID());

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, new IntPtr(0));

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

			// breaks it, but should enable hardware 4 pcf sampling
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRToTexture);
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.DepthTextureMode, (int)All.Intensity);

			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthMap.GetNativeTextureID(), 0);
			GL.DrawBuffer(DrawBufferMode.None);
			var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			if (status != FramebufferErrorCode.FramebufferComplete) debug.Error(status);

			GL.ReadBuffer(ReadBufferMode.None);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //restore default FBO
		}

		public void Dispose()
		{
			GL.DeleteFramebuffer(frameBufferObjectHandle);
			depthMap.Dispose();
		}

		public void Clear()
		{
			//GL.Clear(ClearBufferMask.DepthBufferBit);
			float clearDepth = 1.0f;
			GL.ClearBuffer(ClearBuffer.Depth, 0, ref clearDepth);
		}

		public void FrameBufferForWriting()
		{
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBufferObjectHandle);
		}

		public void BindUniforms(Shader shader)
		{
			var shadowMatrix = this.shadowViewCamera.GetRotationMatrix() * this.shadowViewCamera.GetProjectionMat();

			// projection matrix is in range -1 1, but it is rendered into rexture which is in range 0 1
			// so lets move it from -1 1 into 0 1 range, since we are reading from texture
			shadowMatrix *= Matrix4.CreateScale(new Vector3(0.5f, 0.5f, 0.5f));
			shadowMatrix *= Matrix4.CreateTranslation(new Vector3(0.5f, 0.5f, 0.5f));

			shader.Uniforms.Set("shadowMap.level0", depthMap);
			shader.Uniforms.Set("shadowMap.viewProjectionMatrix", shadowMatrix);
		}

		// doesnt work, depth buffer is copied into target depth buffer, so it doesnt show
		/*internal void DrawContents()
		{
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, frameBufferObjectHandle);

			GL.ReadBuffer(ReadBufferMode.None);
			GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
		} */
	}
}
using MyEngine.Components;
using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
	public class DeferredGBuffer : System.IDisposable
	{
		[Dependency]
		Factory factory;

		[Dependency]
		Debug debug;

		int frameBufferObjectHandle;

		Texture2D[] textures;
		bool readFirstFinalTexture = true;
		public Texture2D finalTextureToRead { get { if (readFirstFinalTexture) return finalTexture1; else return finalTexture2; } }
		public Texture2D finalTextureToWriteTo { get { if (readFirstFinalTexture == false) return finalTexture1; else return finalTexture2; } }
		Texture2D finalTexture1 { get { return textures[4]; } }
		Texture2D finalTexture2 { get { return textures[5]; } }

		Texture2D depthTexture;

		DrawBuffersEnum[] buffers;

		enum GBufferTextures
		{
			Albedo = 0,
			Position = 1,
			Normal = 2,
			Data = 3,
			Final1 = 4,
			Final2 = 5,
		}

		int width;
		int height;

		public DeferredGBuffer(int width, int height)
		{
			this.width = width;
			this.height = height;

			var texturesNum = System.Enum.GetValues(typeof(GBufferTextures)).Length + 2;
			int[] textureHandles = new int[texturesNum];
			textures = new Texture2D[texturesNum];

			// create textures for fbo
			GL.GenTextures(textureHandles.Length, textureHandles);

			List<DrawBuffersEnum> bufs = new List<DrawBuffersEnum>();
			for (int i = 0; i < textureHandles.Length; i++)
			{
				var t = new Texture2D(textureHandles[i]);
				textures[i] = t;
				GL.BindTexture(TextureTarget.Texture2D, t.GetNativeTextureID());
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr(0));
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
				bufs.Add(DrawBuffersEnum.ColorAttachment0 + i);
			}

			depthTexture = new Texture2D(GL.GenTexture());
			GL.BindTexture(TextureTarget.Texture2D, depthTexture.GetNativeTextureID());
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, new IntPtr(0));
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			//GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

			// create frame buffer object
			frameBufferObjectHandle = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObjectHandle);
			for (int i = 0; i < bufs.Count; i++) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, textures[i].GetNativeTextureID(), 0);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture.GetNativeTextureID(), 0);

			/*
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth32fStencil8, width, height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, new IntPtr(0));
            GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, depthTexture.GetNativeTextureID(), 0);
            */

			buffers = bufs.ToArray();

			var status = GL.CheckFramebufferStatus(FramebufferTarget.DrawFramebuffer);
			if (status != FramebufferErrorCode.FramebufferComplete) debug.Error(status);

			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0); //restore default FBO
		}

		public void BindAllFrameBuffersForDrawing()
		{
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBufferObjectHandle);
			GL.DrawBuffers(buffers.Length, buffers);
		}

		public void BindForLightPass(Shader shader)
		{
			readFirstFinalTexture = true;

			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			GL.CullFace(CullFaceMode.Back);

			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBufferObjectHandle);

			GL.DrawBuffer(DrawBufferMode.ColorAttachment4);
			shader.Uniforms.Set("gBufferUniform.depth", depthTexture);

			for (int i = 0; i < textures.Length - 2; i++)
			{
				shader.Uniforms.Set("gBufferUniform." + ((GBufferTextures)i).ToString().ToLower(), textures[i]);
			}
		}

		public void BindForPostProcessEffects(IPostProcessEffect postProcess)
		{
			var shader = postProcess.Shader;
			var generateMipMaps = postProcess.RequiresGBufferMipMaps;

			// generate mip maps for final texture, so it can be used in post processing effects, many post processing effects require blurred texture
			if (generateMipMaps)
			{
				/*GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, finalTextureToWriteTo.GetNativeTextureID());
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);*/

				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, finalTextureToRead.GetNativeTextureID());
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			}

			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			GL.CullFace(CullFaceMode.Back);

			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBufferObjectHandle);

			// draw to the one we are not reading
			if (readFirstFinalTexture == false) GL.DrawBuffer(DrawBufferMode.ColorAttachment4);
			else GL.DrawBuffer(DrawBufferMode.ColorAttachment5);

			shader.Uniforms.Set("gBufferUniform.depth", depthTexture);
			shader.Uniforms.Set("gBufferUniform.final", finalTextureToRead);

			for (int i = 0; i < textures.Length - 2; i++)
			{
				shader.Uniforms.Set("gBufferUniform." + ((GBufferTextures)i).ToString().ToLower(), textures[i]);
			}

			readFirstFinalTexture = !readFirstFinalTexture;
		}

		public void DebugDrawContents()
		{
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, frameBufferObjectHandle);

			int qh = height / 4;
			int qw = width / 4;

			int x = 0;
			int y = 0;

			if (readFirstFinalTexture) DrawBufferToQuarterOfScreen(ReadBufferMode.ColorAttachment4, qw * 1, qh * 0);
			else DrawBufferToQuarterOfScreen(ReadBufferMode.ColorAttachment5, qw * 1, qh * 0);
			if (readFirstFinalTexture) DrawBufferToQuarterOfScreen(ReadBufferMode.ColorAttachment5, qw * 2, qh * 0);
			else DrawBufferToQuarterOfScreen(ReadBufferMode.ColorAttachment4, qw * 2, qh * 0);

			DrawBufferToQuarterOfScreen(ReadBufferMode.ColorAttachment0, qw * 0, qh * 0);
			DrawBufferToQuarterOfScreen(ReadBufferMode.ColorAttachment1, qw * 0, qh * 1);
			DrawBufferToQuarterOfScreen(ReadBufferMode.ColorAttachment2, qw * 1, qh * 1);
			DrawBufferToQuarterOfScreen(ReadBufferMode.ColorAttachment3, qw * 0, qh * 2);
		}

		public void DebugDrawNormal()
		{
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, frameBufferObjectHandle);
			GL.ReadBuffer(ReadBufferMode.ColorAttachment2);
			GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
		}

		void DrawBufferToQuarterOfScreen(ReadBufferMode buffer, int x, int y)
		{
			int qh = height / 4;
			int qw = width / 4;
			GL.ReadBuffer(buffer);
			GL.BlitFramebuffer(0, 0, width, height, x, y, x + qw, y + qh, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
		}

		void DebugDrawTexture(Texture2D texture, float valueScale = 1, float valueOffset = 0)
		{
			DebugDrawTexture(texture, Vector4.One, Vector4.Zero, valueScale, valueOffset);
		}

		void DebugDrawTexture(Texture2D texture, Vector4 positionScale, Vector4 positionOffset, float valueScale = 1, float valueOffset = 0)
		{
			var debugDrawTextureShader = factory.GetShader("internal/debugDrawTexture.shader");

			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.Blend);

			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
			GL.Viewport(0, 0, width, height);
			//GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			debugDrawTextureShader.Uniforms.Set("debugDrawTexture", texture);
			debugDrawTextureShader.Uniforms.Set("debugDrawTexturePositionScale", positionScale);
			debugDrawTextureShader.Uniforms.Set("debugDrawTexturePositionOffset", positionOffset);
			debugDrawTextureShader.Uniforms.Set("debugDrawTextureScale", valueScale);
			debugDrawTextureShader.Uniforms.Set("debugDrawTextureOffset", valueOffset);
			debugDrawTextureShader.Uniforms.Set("debugDrawTextureGamma", 0.1f);

			debugDrawTextureShader.Bind();

			var quadMesh = factory.GetMesh("internal/quad.obj");
			quadMesh.Draw();
		}

		public void Dispose()
		{
			GL.DeleteFramebuffer(frameBufferObjectHandle);
			depthTexture.Dispose();
			foreach (var t in textures) t.Dispose();
		}
	}
}
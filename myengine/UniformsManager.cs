using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MyEngine
{
	public class UniformsData
	{
		Dictionary<string, Texture> uniformTexturesData = new Dictionary<string, Texture>();
		Dictionary<string, object> uniformStructsData = new Dictionary<string, object>();

		HashSet<string> uniformStructsChanged = new HashSet<string>();
		HashSet<string> uniformTexturesChanged = new HashSet<string>();


		public void SendAllUniformsTo(UniformsData uniformManager)
		{
			foreach (var kvp in uniformStructsData) uniformManager.GenericSet(kvp.Key, kvp.Value);
			foreach (var kvp in uniformTexturesData) uniformManager.Set(kvp.Key, kvp.Value);
		}

		/// <summary>
		/// Uploads all uniforms marked as changed into GPU at uniform locations from shader.
		/// Again upload all texture uniform locations.
		/// </summary>
		/// <param name="shader"></param>
		public void UploadChangedUniforms(Shader shader)
		{
			if (uniformStructsChanged.Count > 0)
			{
				foreach (var name in uniformStructsChanged)
				{
					TryUploadStructType(shader, name, uniformStructsData[name]);
				}
				uniformStructsChanged.Clear();
			}

			int texturingUnit = 0;
			foreach (var kvp in uniformTexturesData)
			{
				if (TryUploadStructType(shader, kvp.Key, texturingUnit))
				{
					TryUploadTextureType(shader, kvp.Key, kvp.Value, texturingUnit);
					texturingUnit++;
				}
			}
			if (uniformTexturesChanged.Count > 0) uniformTexturesChanged.Clear();
		}





		public void MarkAllUniformsAsChanged()
		{
			lock (this)
			{
				foreach (var kvp in uniformStructsData) uniformStructsChanged.Add(kvp.Key);
				foreach (var kvp in uniformTexturesData) uniformTexturesChanged.Add(kvp.Key);
			}
		}

		public void Set(string name, Texture data) => GenericSet(name, data);
		public void Set(string name, Matrix4 data) => GenericSet(name, data);
		public void Set(string name, bool data) => GenericSet(name, data);
		public void Set(string name, int data) => GenericSet(name, data);
		public void Set(string name, float data) => GenericSet(name, data);
		public void Set(string name, double data) => GenericSet(name, data);
		public void Set(string name, Vector2 data) => GenericSet(name, data);
		public void Set(string name, Vector3 data) => GenericSet(name, data);
		public void Set(string name, Vector3d data) => GenericSet(name, data);
		public void Set(string name, Vector4 data) => GenericSet(name, data);

		public void GenericSet<T>(string name, T data)
		{
			lock (this)
			{
				if (data is Texture)
				{
					Texture oldTex;
					if (uniformTexturesData.TryGetValue(name, out oldTex) == false || oldTex.Equals(data) == false)
					{
						uniformTexturesData[name] = data as Texture;
						uniformTexturesChanged.Add(name);
					}
				}
				else
				{
					object oldObj = null;
					if (uniformStructsData.TryGetValue(name, out oldObj) == false || oldObj.Equals(data) == false)
					{
						uniformStructsData[name] = data;
						uniformStructsChanged.Add(name);
					}
				}
			}
		}

		public T Get<T>(string name, T defaultValue = default(T))
		{
			lock (this)
			{
				object obj = null;
				if (uniformStructsData.TryGetValue(name, out obj))
				{
					try
					{
						return (T)obj;
					}
					catch
					{

					}
				}

				Texture tex;
				if (uniformTexturesData.TryGetValue(name, out tex))
				{
					try
					{
						return (T)((object)tex);
					}
					catch
					{

					}
				}

				return defaultValue;
			}
		}


		bool TryUploadStructType(Shader shader, string name, object o)
		{

			var location = shader.GetUniformLocation(name);
			if (location == -1) return false;
			if (o is Matrix4)
			{
				var u = (Matrix4)o;
				GL.UniformMatrix4(location, false, ref u); MyGL.Check();
				return true;
			}
			if (o is bool)
			{
				var u = (bool)o;
				GL.Uniform1(location, u ? 1 : 0); MyGL.Check();
				return true;
			}
			if (o is int)
			{
				var u = (int)o;
				GL.Uniform1(location, u); MyGL.Check();
				return true;
			}
			if (o is float)
			{
				var u = (float)o;
				GL.Uniform1(location, u); MyGL.Check();
				return true;
			}
			if (o is double)
			{
				var u = (double)o;
				GL.Uniform1(location, u); MyGL.Check();
				return true;
			}
			if (o is Vector2)
			{
				var u = (Vector2)o;
				GL.Uniform2(location, ref u); MyGL.Check();
				return true;
			}
			if (o is Vector3d)
			{
				var u = (Vector3d)o;
				MyGL.Uniform3(location, ref u); MyGL.Check();
				return true;
			}
			if (o is Vector3)
			{
				var u = (Vector3)o;
				GL.Uniform3(location, ref u); MyGL.Check();
				return true;
			}
			if (o is Vector4)
			{
				var u = (Vector4)o;
				GL.Uniform4(location, ref u); MyGL.Check();
				return true;
			}
			return false;
		}



		bool TryUploadTextureType(Shader shader, string name, object o, int texturingUnit)
		{
			if (o is Texture2D)
			{
				var u = (Texture2D)o;
				SendTexture(name, u, texturingUnit);
				return true;
			}
			if (o is Cubemap)
			{
				var u = (Cubemap)o;
				SendTexture(name, u, texturingUnit);
				return true;
			}

			return false;
		}
		void SendTexture(string name, Texture2D texture2D, int texturingUnit)
		{
			GL.ActiveTexture(TextureUnit.Texture0 + texturingUnit); MyGL.Check();
			GL.BindTexture(TextureTarget.Texture2D, texture2D.GetNativeTextureID()); MyGL.Check();
		}
		void SendTexture(string name, Cubemap cubeMap, int texturingUnit)
		{
			GL.ActiveTexture(TextureUnit.Texture0 + texturingUnit); MyGL.Check();
			GL.BindTexture(TextureTarget.TextureCubeMap, cubeMap.GetNativeTextureID()); MyGL.Check();
		}


	}

}
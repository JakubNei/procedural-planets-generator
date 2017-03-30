using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace MyEngine
{
	public class ShaderBuilder
	{
		string prependSource;

		
		readonly FileSystem FileSystem;

		public ShaderBuilder(FileSystem fileSystem)
		{
            FileSystem = fileSystem;
		}

		void Prepend(MyFile name)
		{
			using (var fs = new System.IO.StreamReader(name.GetDataStream()))
				prependSource += fs.ReadToEnd();
		}

		public void LoadAndParse(MyFile file)
		{
			string source = file.ReadAllText();

			source = ReplaceIncludeDirectiveWithFileContents(source, file.Folder);

			// the contents above first shader part (program) [VertexShader] are shared amongs all parts (prepended to all parts)
			string prependContents = "";
			{
				ShaderType shaderType = ShaderType.VertexShader;
				int startOfTag = GetClosestShaderTypeTagPosition(source, 0, ref shaderType);
                if (startOfTag > 0)
                {
                    prependContents += source.Substring(0, startOfTag - 1);
                    source = source.Substring(startOfTag);
                }
			}

			int currentStartingLine = prependContents.Count('\n');

			foreach (ShaderType type in Enum.GetValues(typeof(ShaderType)))
			{
				ShaderType shaderType = ShaderType.VertexShader;
				int startOfTag = GetClosestShaderTypeTagPosition(source, 0, ref shaderType);

				if (startOfTag != -1)
				{
					var tagLength = shaderType.ToString().Length + 2;

					ShaderType notUsed = ShaderType.VertexShader;
					int endOfShaderPart = GetClosestShaderTypeTagPosition(source, startOfTag + tagLength, ref notUsed);
					if (endOfShaderPart == -1) endOfShaderPart = source.Length;

					var startOfShaderPart = startOfTag + tagLength;
					string shaderPart = source.Substring(
						startOfShaderPart,
						endOfShaderPart - startOfShaderPart
					);

					AttachShader(prependContents + "\n#line " + currentStartingLine + "\n" + shaderPart, shaderType, file.VirtualPath);

					currentStartingLine += shaderPart.Count('\n');

					source = source.Substring(endOfShaderPart);
				}
			}
		}

		HashSet<string> f = new HashSet<string>();

		string GetIncludeFileContents(string virtualPath, MyFolder folder)
		{
			var file = FileSystem.FindFile(virtualPath, folder);

			string source = file.ReadAllText();

			source = ReplaceIncludeDirectiveWithFileContents(source, FileSystem.GetFolder(file));

			return source;
		}

		string ReplaceIncludeDirectiveWithFileContents(string source, MyFolder folder)
		{
			while (true)
			{
				var start = source.IndexOf("[include");
				if (start != -1)
				{
					var fileNameStart = start + "[include".Length + 1;
					var end = source.IndexOf("]", fileNameStart);
					if (end != -1)
					{
						var fileName = source.Substring(fileNameStart, end - fileNameStart).Trim();
						source =
							source.Substring(0, start) +
							GetIncludeFileContents(fileName, folder) +
							source.Substring(end + 1);
					}
				}
				else
				{
					break;
				}
			}
			return source;
		}

		int GetClosestShaderTypeTagPosition(string source, int offset, ref ShaderType shaderType)
		{
			int startOfTag = -1;
			foreach (ShaderType type in System.Enum.GetValues(typeof(ShaderType)))
			{
				string tag = "[" + type.ToString() + "]";

				int thisStartOfTag = source.IndexOf(tag, offset);
				if (thisStartOfTag != -1)
				{
					if (startOfTag == -1 || thisStartOfTag < startOfTag)
					{
						shaderType = type;
						startOfTag = thisStartOfTag;
					}
				}
			}
			return startOfTag;
		}

		public struct ShaderBuildResult
		{
			public string shaderContents;
			public ShaderType shaderType;
			public string filePath;
		}

		public List<ShaderBuildResult> buildResults = new List<ShaderBuildResult>();

		void AttachShader(string shaderContents, ShaderType shaderType, string filePath)
		{
			buildResults.Add(new ShaderBuildResult()
			{
				shaderType = shaderType,
				shaderContents = shaderContents,
				filePath = filePath,
			});
		}
	}
}
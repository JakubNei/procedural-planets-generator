using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyEngine
{
	public class ShaderBuilder
	{
		string prependSource;

		public List<FileExisting> includedFiles = new List<FileExisting>();

		readonly FileSystem FileSystem;

		public ShaderBuilder(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
		}

		void Prepend(FileExisting name)
		{
			using (var fs = new System.IO.StreamReader(name.OpenReadWrite()))
				prependSource += fs.ReadToEnd();
		}

		public void LoadAndParse(FileExisting file)
		{
			string source = file.ReadAllText();

			source = AddLineMarkers(source, file);
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

					AttachShader(prependContents + shaderPart, shaderType, file.VirtualPath);

					source = source.Substring(endOfShaderPart);
				}
			}
		}

		HashSet<string> f = new HashSet<string>();

		string AddLineMarkers(string text, FileExisting file)
		{
			if (!includedFiles.Contains(file))
				includedFiles.Add(file);


			var fileId = includedFiles.IndexOf(file);


			var lines = text.Split('\n').ToList();

			int macrosInserted = 0;

			lines.Insert(1, "#line " + 2 + " " + fileId);
			macrosInserted++;
			
			for (int i = 0; i < lines.Count; i++)
			{
				var line = lines[i];
				if (ShouldMarkLine(line))
				{
					lines.Insert(i + 1, "#line " + (i + 2 - macrosInserted) + " " + fileId);
					macrosInserted++;
				}
			}

			return lines.Join("\n");
		}

		bool ShouldMarkLine(string line)
		{
			if (line.Contains("[include")) return true;

			foreach (ShaderType type in System.Enum.GetValues(typeof(ShaderType)))
			{
				string tag = "[" + type.ToString() + "]";
				if (line.Contains(tag)) return true;
			}

			return false;
		}

		string GetIncludeFileContents(string virtualPath, FolderExisting folder)
		{
			var file = FileSystem.FindFile(virtualPath, folder);
			var source = file.ReadAllText();
			source = AddLineMarkers(source, file);
			source = ReplaceIncludeDirectiveWithFileContents(source, FileSystem.GetFolder(file));
			return source;
		}

		string ReplaceIncludeDirectiveWithFileContents(string source, FolderExisting folder)
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
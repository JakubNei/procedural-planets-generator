using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine
{
    public partial class Shader
    {
        public class ShaderBuilder
        {
            string prependSource;

            void Prepend(ResourcePath name)
            {
                using (var fs = new System.IO.StreamReader(name))
                    prependSource += fs.ReadToEnd();
            }
            string ReadAll(Stream stream, int numOfRetries = 5)
            {
                string text = "";
                while (numOfRetries > 0)
                {
                    try
                    {
                        using (var sr = new StreamReader(stream, Encoding.Default))
                            text = sr.ReadToEnd();
                        numOfRetries = 0;
                    }
                    catch (IOException e)
                    {
                        numOfRetries--;
                    }
                }
                return text;
            }
            public void Load(ResourcePath resource)
            {
                var filePath = (string)resource;

                string source = "";
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    source = ReadAll(fs);



                ResourcePath prependAllRes = "internal/prependAll.shader";
                string prependAllFileContents = "";

                using (var fs = new FileStream(prependAllRes, FileMode.Open, FileAccess.Read, FileShare.Read))
                    prependAllFileContents = ReadAll(fs);

                string prependContents = "";
                {
                    ShaderType shaderType = ShaderType.VertexShader;
                    int startOfTag = GetClosestShaderTypeTagPosition(source, 0, ref shaderType);
                    if (startOfTag == -1)
                    {
                        Debug.Error("Shader part start not found " + filePath);
                        return;
                    }
                    prependContents += source.Substring(0, startOfTag - 1);
                    source = source.Substring(startOfTag);
                }

                int currentStartingLine = prependContents.Split('\n').Length;

                prependContents = prependAllFileContents + prependContents;


                foreach (ShaderType type in System.Enum.GetValues(typeof(ShaderType)))
                {

                    ShaderType shaderType = ShaderType.VertexShader;
                    int startOfTag = GetClosestShaderTypeTagPosition(source, 0, ref shaderType);

                    if (startOfTag != -1)
                    {
                        var tagLength = shaderType.ToString().Length + 2;
                        ShaderType _st = ShaderType.VertexShader;
                        int endOfShaderPart = GetClosestShaderTypeTagPosition(source, startOfTag + tagLength, ref _st);
                        if (endOfShaderPart == -1) endOfShaderPart = source.Length;

                        var startOfShaderPart = startOfTag + tagLength;
                        string shaderPart = source.Substring(
                            startOfShaderPart,
                            endOfShaderPart - startOfShaderPart
                        );


                        AttachShader(prependContents + "\n#line " + currentStartingLine + "\n" + shaderPart, shaderType, filePath);

                        currentStartingLine += shaderPart.Split('\n').Length;

                        source = source.Substring(endOfShaderPart);
                    }
                }
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
                            shaderType = type;
                        startOfTag = thisStartOfTag;
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
}

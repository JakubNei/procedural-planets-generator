using MyEngine.Components;
using Neitri;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyEngine
{
	public partial class ObjLoader
	{
		[Dependency]
		IDependencyManager dependency;

		public Mesh Load(Asset resource, Entity appendToEntity = null)
		{
			var loadJob = dependency.Create<LoadJob>();
			var mesh = loadJob.Parse(resource, appendToEntity);
			mesh.asset = resource;
			return mesh;
		}

		static char[] splitCharacters = new char[] { ' ' };

		class LoadJob
		{
			[Dependency]
			AssetSystem assetSystem;

			[Dependency]
			Debug debug;

			[Dependency]
			Factory factory;

			List<Vector3> verticesObj = new List<Vector3>();
			List<Vector3> normalsObj = new List<Vector3>();
			List<Vector2> uvsObj = new List<Vector2>();

			List<Vector3> verticesMesh = new List<Vector3>();
			List<Vector3> normalsMesh = new List<Vector3>();
			List<Vector2> uvsMesh = new List<Vector2>();
			List<int> triangleIndiciesMesh = new List<int>();

			bool gotUvs = false;
			bool gotNormal = false;

			Dictionary<string, int> objFaceToMeshIndicie = new Dictionary<string, int>();

			MaterialLibrary materialLibrary;
			MaterialPBR lastMaterial;

			int failedParse = 0;

			void Parse(ref string str, ref float t)
			{
				if (!float.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out t))
					failedParse++;
			}

			public Mesh Parse(Asset asset, Entity appendToEntity)
			{
				using (StreamReader textReader = new StreamReader(asset.GetDataStream()))
				{
					int i1, i2, i3, i4;

					string line;
					while ((line = textReader.ReadLine()) != null)
					{
						line = line.Trim();
						if (line.StartsWith("#")) continue;
						line = line.Replace("  ", " ");

						string[] parameters = line.Split(splitCharacters);

						switch (parameters[0])
						{
							case "p": // Point
								break;

							case "v": // Vertex
								var v = Vector3.Zero;
								Parse(ref parameters[1], ref v.X);
								Parse(ref parameters[2], ref v.Y);
								Parse(ref parameters[3], ref v.Z);
								verticesObj.Add(v);
								break;

							case "vt": // TexCoord
								gotUvs = true;
								var vt = Vector2.Zero;
								Parse(ref parameters[1], ref vt.X);
								Parse(ref parameters[2], ref vt.Y);
								uvsObj.Add(vt);
								break;

							case "vn": // Normal
								gotNormal = true;
								var vn = Vector3.Zero;
								Parse(ref parameters[1], ref vn.X);
								Parse(ref parameters[2], ref vn.Y);
								Parse(ref parameters[3], ref vn.Z);
								normalsObj.Add(vn);
								break;

							case "f":
								switch (parameters.Length)
								{
									case 4:
										i1 = ParseFaceParameter(parameters[1]);
										i2 = ParseFaceParameter(parameters[2]);
										i3 = ParseFaceParameter(parameters[3]);
										triangleIndiciesMesh.Add(i1);
										triangleIndiciesMesh.Add(i2);
										triangleIndiciesMesh.Add(i3);
										break;

									case 5:
										i1 = ParseFaceParameter(parameters[1]);
										i2 = ParseFaceParameter(parameters[2]);
										i3 = ParseFaceParameter(parameters[3]);
										i4 = ParseFaceParameter(parameters[4]);
										triangleIndiciesMesh.Add(i1);
										triangleIndiciesMesh.Add(i2);
										triangleIndiciesMesh.Add(i3);
										triangleIndiciesMesh.Add(i1);
										triangleIndiciesMesh.Add(i3);
										triangleIndiciesMesh.Add(i4);
										break;
								}
								break;

							case "mtllib":
								if (assetSystem.AssetExists(parameters[1], asset.AssetFolder))
								{
									materialLibrary = new MaterialLibrary(assetSystem.FindAsset(parameters[1], asset.AssetFolder), assetSystem, factory);
								}
								break;

							case "usemtl":
								if (parameters[1] == "(null)") continue;
								if (materialLibrary != null) lastMaterial = materialLibrary.GetMat(parameters[1]);
								break;
						}
					}

					textReader.Close();
				}

				if (appendToEntity != null) return EndObjPart(appendToEntity);

				debug.Info("Loaded " + asset + " vertices:" + verticesMesh.Count + " faces:" + triangleIndiciesMesh.Count / 3);

				return EndMesh();
			}

			Mesh EndMesh()
			{
				var mesh = new Mesh();

				mesh.Vertices.SetData(verticesMesh);
				mesh.UVs.SetData(uvsMesh);
				mesh.TriangleIndicies.SetData(triangleIndiciesMesh);

				if (failedParse > 0) debug.Warning("Failed to parse data " + failedParse + " times");
				failedParse = 0;

				if (gotNormal) mesh.Normals.SetData(normalsMesh);
				else mesh.RecalculateNormals();

				return mesh;
			}

			Mesh EndObjPart(Entity appendToEntity)
			{
				var renderer = appendToEntity.AddComponent<MeshRenderer>();
				renderer.Material = lastMaterial;
				renderer.Mesh = EndMesh();
				return renderer.Mesh;
			}

			static char[] faceParamaterSplitter = new char[] { '/' };

			int ParseFaceParameter(string faceParameter)
			{
				Vector3 vertex = new Vector3();
				Vector2 texCoord = new Vector2();
				Vector3 normal = new Vector3();

				string[] parameters = faceParameter.Split(faceParamaterSplitter);

				int vertexIndex = int.Parse(parameters[0]);
				if (vertexIndex < 0) vertexIndex = verticesObj.Count + vertexIndex;
				else vertexIndex = vertexIndex - 1;
				vertex = verticesObj[vertexIndex];

				if (parameters.Length > 1)
				{
					int texCoordIndex = 0;
					if (int.TryParse(parameters[1], out texCoordIndex))
					{
						if (texCoordIndex < 0) texCoordIndex = uvsObj.Count + texCoordIndex;
						else texCoordIndex = texCoordIndex - 1;
						texCoord = uvsObj[texCoordIndex];
					}
				}

				if (parameters.Length > 2)
				{
					int normalIndex = 0;
					if (int.TryParse(parameters[2], out normalIndex))
					{
						if (normalIndex < 0) normalIndex = normalsObj.Count + normalIndex;
						else normalIndex = normalIndex - 1;
						normal = normalsObj[normalIndex];
					}
				}

				return FindOrAddObjVertex(ref faceParameter, ref vertex, ref texCoord, ref normal);
			}

			int FindOrAddObjVertex(ref string faceParamater, ref Vector3 vertex, ref Vector2 texCoord, ref Vector3 normal)
			{
				int index;
				if (objFaceToMeshIndicie.TryGetValue(faceParamater, out index))
				{
					return index;
				}
				else
				{
					verticesMesh.Add(vertex);
					uvsMesh.Add(texCoord);
					normalsMesh.Add(normal);

					index = verticesMesh.Count - 1;
					objFaceToMeshIndicie[faceParamater] = index;

					return index;
				}
			}
		}
	}
}
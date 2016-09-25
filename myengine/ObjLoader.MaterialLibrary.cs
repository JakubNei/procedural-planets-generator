using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyEngine
{
	public partial class ObjLoader
	{
		class MaterialLibrary
		{
			Dictionary<string, MaterialPBR> materials = new Dictionary<string, MaterialPBR>();

			int failedParse = 0;

			void Parse(ref string str, ref float t)
			{
				if (!float.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out t))
					failedParse++;
			}

			public MaterialLibrary(Asset asset, AssetSystem assetSystem, Factory factory)
			{
				using (var s = asset.GetDataStream())
				using (StreamReader textReader = new StreamReader(s))
				{
					MaterialPBR lastMat = new MaterialPBR(factory);
					string line;
					while ((line = textReader.ReadLine()) != null)
					{
						line = line.Trim();
						line = line.Replace("  ", " ");

						string[] parameters = line.Split(splitCharacters);

						/*
							Ka 1.000 1.000 1.000
						   Kd 1.000 1.000 1.000
						   Ks 0.000 0.000 0.000
						   d 1.0
						   illum 2
						   map_Ka lenna.tga           # the ambient texture map
						   map_Kd lenna.tga           # the diffuse texture map (most of the time, it will
													  # be the same as the ambient texture map)
						   map_Ks lenna.tga           # specular color texture map
						   map_Ns lenna_spec.tga      # specular highlight component
						   map_d lenna_alpha.tga      # the alpha texture map
						   map_bump lenna_bump.tga    # some implementations use 'map_bump' instead of 'bump' below
						 * */

						switch (parameters[0])
						{
							case "newmtl":
								lastMat = new MaterialPBR(factory);
								materials[parameters[1]] = lastMat;
								break;

							case "Kd": // diffuse
								if (parameters.Length > 2)
								{
									Parse(ref parameters[1], ref lastMat.albedo.X);
									Parse(ref parameters[2], ref lastMat.albedo.Y);
									Parse(ref parameters[3], ref lastMat.albedo.Z);
								}
								else
								{
									float r = 1;
									Parse(ref parameters[1], ref r);
									lastMat.albedo.X = lastMat.albedo.Y = lastMat.albedo.Z = r;
								}
								break;

							case "map_Kd":
								lastMat.albedoTexture = new Texture2D(assetSystem.FindAsset(parameters[1], asset.AssetFolder));
								break;
						}
					}

					textReader.Close();
				}
			}

			public MaterialPBR GetMat(string matName)
			{
				MaterialPBR ret = null;
				materials.TryGetValue(matName, out ret);
				return ret;
			}
		}
	}
}
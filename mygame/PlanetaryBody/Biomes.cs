using MyEngine;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MyGame.PlanetaryBody
{

	public class Biome
	{
		public int atlasId;
		public Texture2D diffuse;
		public Texture2D normal;
		public Color color;
		public Vector3 VectorColor => color.ToVector4().Xyz;

	}

	public class BiomesAtlas : SingletonsPropertyAccesor
	{
		int nextId = 0;



		public Dictionary<Color, Biome> biomes = new Dictionary<Color, Biome>();
		public void AddBiome(Color color, string textureVirtualPath)
		{
			Texture2D diffuse;
			Texture2D normal;

			var diffuseFile = FileSystem.FindOptionalFile(textureVirtualPath + "_d.*");
			if (diffuseFile.Exists) diffuse = Factory.GetTexture2D(diffuseFile.VirtualPath);
			else diffuse = Factory.TestTexture;

			var normalFile = FileSystem.FindOptionalFile(textureVirtualPath + "_n.*");
			if (normalFile.Exists) normal = Factory.GetTexture2D(normalFile.VirtualPath);
			else normal = Factory.DefaultNormalMap;

			AddBiome(color, diffuse, normal);

		}
		public void AddBiome(Color color, Texture2D diffuse, Texture2D normal)
		{
			var b = new Biome()
			{
				diffuse = diffuse,
				normal = normal,
				atlasId = nextId,
				color = color,
			};
			nextId++;
			biomes[color] = b;
		}

		public Biome GetBiome(Color color)
		{
			return biomes[color];
		}
	}


}

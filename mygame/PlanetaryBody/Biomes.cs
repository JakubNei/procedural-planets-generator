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
		public Texture2D diffuse;
		public Texture2D normal;
	}

	public class BiomesAtlas
	{
		Dictionary<Color, Biome> biomes = new Dictionary<Color, Biome>();
		public void AddBiome(Color color, Texture2D diffuse, Texture2D normal)
		{
			var b = new Biome()
			{
				diffuse = diffuse,
				normal = normal,
			};
			biomes[color] = b;
		}

		public Biome GetBiome(Color color)
		{
			return biomes[color];
		}
	}


}

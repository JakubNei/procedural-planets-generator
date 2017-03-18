using MyEngine;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{
	public class Config
	{
		public double radiusMin;
		public double noiseMultiplier;
		public Texture2D biomesSplatMap;

		public Texture2D baseHeightMap;
		public double baseHeightMapMultiplier;


		public class BiomeData
		{
			public Vector3 biomesMapColor;
			public Texture2D diffuse;
			public Texture2D normal;
		}

		private List<BiomeData> biomes = new List<BiomeData>();

		public void AddBiome(Vector3 biomesMapColor, Texture2D diffuse, Texture2D normal)
		{
			biomes.Add(new BiomeData()
			{
				biomesMapColor = biomesMapColor,
				diffuse = diffuse,
				normal = normal,
			});
		}

		public void SetTo(UniformsData uniforms)
		{
			uniforms.Set("param_radiusMin", (double)radiusMin);
			uniforms.Set("param_noiseMultiplier", (double)noiseMultiplier);
			uniforms.Set("param_biomesSplatMap", biomesSplatMap);

			uniforms.Set("param_baseHeightMap", baseHeightMap);
			uniforms.Set("param_baseHeightMapMultiplier", (double)baseHeightMapMultiplier);

			for (int i = 0; i < biomes.Count; i++)
			{
				var biome = biomes[i];
				uniforms.Set("param_biome" + i + "_biomesSplatMapColor", biome.biomesMapColor);
				uniforms.Set("param_biome" + i + "_diffuseMap", biome.diffuse);
				uniforms.Set("param_biome" + i + "_normalMap", biome.normal);
			}
		}
	}
}

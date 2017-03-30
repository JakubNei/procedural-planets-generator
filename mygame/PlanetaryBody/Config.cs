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

		public int chunkNumberOfVerticesOnEdge = 50;
		public float sizeOnScreenNeededToSubdivide = 0.3f;
		public int stopSegmentRecursionAtWorldSize = 100;

		public double radiusMin;
		public double noiseMultiplier;

		public Texture2D baseHeightMap;
		public double baseHeightMapMultiplier;


		public class BiomeData
		{
			public Texture2D diffuse;
			public Texture2D normal;
		}

		private Dictionary<int, Texture2D> splatMaps = new Dictionary<int, Texture2D>();
		/// <summary>
		/// Every splat map has 4 channels, every channel controls biome intensity.
		/// splatMapTexture = splatMaps[floor(biome.id / 4)];
		/// splatMapChannel = splatMaps[biome.id % 4];
		/// </summary>
		/// <param name="id"></param>
		/// <param name="splatMap"></param>
		public void AddControlSplatMap(int id, Texture2D splatMap)
		{
			splatMaps.Add(id, splatMap);
		}

		private Dictionary<int, BiomeData> biomes = new Dictionary<int, BiomeData>();

		public void AddBiome(int id, Texture2D diffuse, Texture2D normal)
		{
			biomes.Add(id, new BiomeData()
			{
				diffuse = diffuse,
				normal = normal,
			});
		}

		public void SetTo(UniformsData uniforms)
		{
			uniforms.Set("param_radiusMin", (double)radiusMin);
			uniforms.Set("param_noiseMultiplier", (double)noiseMultiplier);

			uniforms.Set("param_baseHeightMap", baseHeightMap);
			uniforms.Set("param_baseHeightMapMultiplier", (double)baseHeightMapMultiplier);

			foreach(var splatMap in splatMaps)
			{
				uniforms.Set("param_biomesSplatMap" + splatMap.Key, splatMap.Value);
			}

			foreach(var biome in biomes)
			{
				var splatMapTextureId = (biome.Key / 4.0).FloorToInt();
				var splatMapTextureChannel = biome.Key % 4;
				uniforms.Set("param_biome" + splatMapTextureId + "" + splatMapTextureChannel + "_diffuseMap", biome.Value.diffuse);
				uniforms.Set("param_biome" + splatMapTextureId + "" + splatMapTextureChannel + "_normalMap", biome.Value.normal);
			}
		}
	}
}

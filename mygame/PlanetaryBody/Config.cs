using MyEngine;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BiomesMetadataFile = PreparePlanetData.BiomesMetadataFile;

namespace MyGame.PlanetaryBody
{
	public class Config
	{

		public int chunkNumberOfVerticesOnEdge = 50;
		public float weightNeededToSubdivide = 0.3f;
		public int stopSegmentRecursionAtWorldSize = 100;

		public double radiusMin;
		public double noiseMultiplier;

		public Texture2D baseHeightMap;
		public double baseHeightMapMultiplier;

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

		private Dictionary<int, Biome> biomes = new Dictionary<int, Biome>();

		public void LoadConfig(MyFile path, BiomesAtlas atlas)
		{
			var data = BiomesMetadataFile.Load(path.RealPath);
			foreach (var i in data.data)
			{
				var b = atlas.GetBiome(i.Color);
				AddBiome(i.BiomeId, b);
			}
		}

		public void AddBiome(int id, Biome biome)
		{
			biomes.Add(id, biome);
		}

		public void SetTo(UniformsData uniforms)
		{
			uniforms.Set("param_radiusMin", (double)radiusMin);
			uniforms.Set("param_noiseMultiplier", (double)noiseMultiplier);

			uniforms.Set("param_baseHeightMap", baseHeightMap);
			uniforms.Set("param_baseHeightMapMultiplier", (double)baseHeightMapMultiplier);

			foreach (var splatMap in splatMaps)
			{
				uniforms.Set("param_biomesSplatMap" + splatMap.Key, splatMap.Value);
			}

			foreach (var biome in biomes)
			{
				var splatMapTextureId = (biome.Key / 4.0).FloorToInt();
				var splatMapTextureChannel = biome.Key % 4;
				uniforms.Set("param_biome" + splatMapTextureId + "" + splatMapTextureChannel + "_diffuseMap", biome.Value.diffuse);
				uniforms.Set("param_biome" + splatMapTextureId + "" + splatMapTextureChannel + "_normalMap", biome.Value.normal);
			}
		}
	}
}

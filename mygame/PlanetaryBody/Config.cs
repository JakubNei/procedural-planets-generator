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

		public Texture2D biomesControlMap;



		private Dictionary<int, Biome> biomes = new Dictionary<int, Biome>();

		public void AddBiomes(BiomesAtlas atlas)
		{
			foreach (var b in atlas.biomes.Values)
				AddBiome(b.atlasId, b);
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

			uniforms.Set("param_biomesControlMap", biomesControlMap);

			foreach (var biome in biomes)
			{
				var splatMapTextureId = (biome.Key / 4.0).FloorToInt();
				var splatMapTextureChannel = biome.Key % 4;

				var channel = new string[] { "r", "g", "b", "a" }[splatMapTextureChannel];
				splatMapTextureId += 1;
				uniforms.Set("param_biome" + splatMapTextureId + "" + channel + "_diffuseMap", biome.Value.diffuse);
				uniforms.Set("param_biome" + splatMapTextureId + "" + channel + "_normalMap", biome.Value.normal);
				uniforms.Set("param_biome" + splatMapTextureId + "" + channel + "_color", biome.Value.VectorColor);
			}
		}
	}
}

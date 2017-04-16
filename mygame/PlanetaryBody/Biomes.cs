using MyEngine;
using OpenTK;
using System;
using System.Collections.Generic;
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
		Dictionary<Vector3, Biome> biomes = new Dictionary<Vector3, Biome>();
		public void AddBiome(Vector3 color, Texture2D diffuse, Texture2D normal)
		{
			var b = new Biome()
			{
				diffuse = diffuse,
				normal = normal,
			};
			biomes[color] = b;
		}

		public Biome GetBiome(Vector3 color)
		{
			return biomes[color];
		}
	}


	public class BiomesMetadataFile
	{
		[Serializable()]
		public class BiomeMetadata
		{
			[System.Xml.Serialization.XmlElement()]
			public float ColorRed { get; set; }
			[System.Xml.Serialization.XmlElement()]
			public float ColorGreen { get; set; }
			[System.Xml.Serialization.XmlElement()]
			public float ColorBlue { get; set; }
			[System.Xml.Serialization.XmlElement()]
			public string SplatMapName { get; set; }
			[System.Xml.Serialization.XmlElement()]
			public int SplatMapChannel { get; set; }
			[System.Xml.Serialization.XmlElement()]
			public int BiomeId { get; set; }
		}

		public List<BiomeMetadata> data = new List<BiomeMetadata>();

		public void Save(string filePath)
		{
			var xmlser = new XmlSerializer(typeof(List<BiomeMetadata>));
			var srdr = new StreamWriter(filePath);
			xmlser.Serialize(srdr, data);
			srdr.Close();
		}


		public static BiomesMetadataFile Load(string filePath)
		{
			var xmlser = new XmlSerializer(typeof(List<BiomeMetadata>));
			var srdr = new StreamReader(filePath);
			var data = (List<BiomeMetadata>)xmlser.Deserialize(srdr);
			srdr.Close();
			return new BiomesMetadataFile()
			{
				data = data,
			};
		}


	}

}

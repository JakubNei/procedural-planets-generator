using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PreparePlanetData
{
	public class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			var p = new Program();

			Console.WriteLine("enter planet path (eg.:'../../../resources/planet/data/earth/') :");
			p.targetFolder = Console.ReadLine();

			Console.WriteLine("enter max pixel distance to sea (default 20):");
			p.maxDistanceToSea = float.Parse(Console.ReadLine());

			Console.WriteLine("enter maximum biomes splat map wiedth (default 1024):");
			p.maximumBiomesSplatMapwWidth = int.Parse(Console.ReadLine());

			p.Run();
		}

		public string targetFolder = "../../../resources/planet/data/earth/";
		public float maxDistanceToSea = 20;
		public float distanceToSieaIncreasePerPixel = 1;

		public int maximumBiomesSplatMapwWidth = 1024;

		public void Run()
		{

			// https://visibleearth.nasa.gov/view.php?id=73934
			var surfaceElevation = Texture.LoadTexture(targetFolder + "surface_elevation_map.jpg");
			var oceanElevation = Texture.LoadTexture(targetFolder + "ocean_elevation_map.jpg");


			const float defaultAmount = 0.5f;



			var finalHeightTexture = new Texture(oceanElevation.Width, oceanElevation.Height);

			Console.WriteLine("combining " + nameof(oceanElevation) + " and " + nameof(surfaceElevation) + " into " + nameof(finalHeightTexture));
			Parallel.For(0, finalHeightTexture.Width, (int x) =>
			{
				for (int y = 0; y < finalHeightTexture.Height; y++)
				{
					float resultRedAmount = defaultAmount;


					var surface = surfaceElevation.GetTexel(x, y).x;
					var ocean = oceanElevation.GetTexel(x, y).x;

					if (surface > 0)
					{
						resultRedAmount = 0.5f + surface * 0.5f;
						oceanElevation.SetAt(x, y, Color.White);
					}
					else if (ocean < 1)
					{
						resultRedAmount = ocean * 0.5f;
						surfaceElevation.SetAt(x, y, Color.Black);
					}

					finalHeightTexture.SetAt(x, y, Vector3.One * resultRedAmount);
				}
			});
			Console.WriteLine("combined " + nameof(oceanElevation) + " and " + nameof(surfaceElevation) + " into " + nameof(finalHeightTexture));



			if (finalHeightTexture.Width > maximumBiomesSplatMapwWidth)
				finalHeightTexture.SaveAsJPG(targetFolder + "height_map.jpg", 70);
			else
				finalHeightTexture.Save(targetFolder + "height_map.png");

			Console.WriteLine(nameof(finalHeightTexture) + " too big for subsequent generation, gotta downsize");
			if (finalHeightTexture.Width > maximumBiomesSplatMapwWidth)
			{
				surfaceElevation.Resize(maximumBiomesSplatMapwWidth, maximumBiomesSplatMapwWidth);
				oceanElevation.Resize(maximumBiomesSplatMapwWidth, maximumBiomesSplatMapwWidth);
				finalHeightTexture.Resize(maximumBiomesSplatMapwWidth, maximumBiomesSplatMapwWidth);
			}


			{
				var biomesSplatMapColored = new Texture(oceanElevation.Width, oceanElevation.Height);
				var biomesSplatMapChanneledTextures = new List<Texture>();


				var infoFile = new BiomesMetadataFile();
				var biomesControlMapBiomesColors = new HashSet<Color>();
				var biomesColorToTextureAndChannel = new Dictionary<Color, Tuple<ushort, Texture>>();
				var biomesControlMap = Texture.LoadTexture(targetFolder + "biomes_control_map.png");
				const int numberOfChannelsToUsePerChanneledTexture = 4;

				Console.WriteLine("learning possible biomes");
				for (int x = 0; x < biomesControlMap.Width; x++)
				{
					for (int y = 0; y < biomesControlMap.Height; y++)
					{
						var c = biomesControlMap.GetPixel(x, y);
						if (!biomesControlMapBiomesColors.Contains(c))
						{
							// new biome (color)
							var nextChannel = (ushort)(biomesColorToTextureAndChannel.Count % numberOfChannelsToUsePerChanneledTexture);
							var targetTexture = biomesSplatMapChanneledTextures.LastOrDefault();
							var shouldMakeNewChannelsTexture = biomesColorToTextureAndChannel.Count % numberOfChannelsToUsePerChanneledTexture == 0;
							if (shouldMakeNewChannelsTexture)
							{
								targetTexture = new Texture(biomesSplatMapColored.Width, biomesSplatMapColored.Height);
								biomesSplatMapChanneledTextures.Add(targetTexture);
							}
							var n = "biomes_splat_map_" + biomesSplatMapChanneledTextures.IndexOf(targetTexture) + ".png";
							infoFile.data.Add(new BiomesMetadataFile.BiomeMetadata()
							{
								SplatMapName = n,
								SplatMapChannel = nextChannel,
								Color = c,
								BiomeId = biomesControlMapBiomesColors.Count,
							});
							biomesColorToTextureAndChannel[c] = new Tuple<ushort, Texture>(nextChannel, targetTexture);
							biomesControlMapBiomesColors.Add(c);
						}
					}
				}
				Console.WriteLine("learned possible biomes");

				infoFile.Save(targetFolder + "biomes_splat_maps_metadata.xml");

				var temperatureMap = new Texture(oceanElevation.Width, oceanElevation.Height);
				var humidityMap = new Texture(oceanElevation.Width, oceanElevation.Height);


				Console.WriteLine("generating biomes");
				Parallel.For(0, biomesSplatMapColored.Width, (int x) =>
				{
					for (int y = 0; y < biomesSplatMapColored.Height; y++)
					{
						var altidute = (float)Math.Max(0, finalHeightTexture.GetTexel(x, y).x * 2 - 1.0f); // 0 sea level.. 1 max
						var distanceToSea = DistanceToValueUnder(oceanElevation, x, y, 1) / (float)maxDistanceToSea; // 0..1
																													 //if (distanceToSea > 1) distanceToSea = 1;
																													 //if (distanceToSea < 0) distanceToSea = 0;
						var distanceFromPoles = 1.0f - (float)Math.Abs(y / (float)biomesSplatMapColored.Height - 0.5f) * 2; // 1 at meridian.. 0 at poles

						var temperature = (1 - altidute) * distanceFromPoles;
						temperatureMap.SetAt(x, y, Vector3.One * temperature);
						var humidity = 1 - distanceToSea;
						humidityMap.SetAt(x, y, Vector3.One * humidity);

						var biome = biomesControlMap.GetPixel(temperature, humidity);

						biomesSplatMapColored.SetAt(x, y, biome);

						var splatMap = biomesColorToTextureAndChannel[biome];
						splatMap.Item2.SetAt(x, y, splatMap.Item1, 1);
					}
				});
				Console.WriteLine("generated biomes");

				biomesSplatMapColored.Save(targetFolder + "biomes_splat_map_colored.png");
				for (int i = 0; i < biomesSplatMapChanneledTextures.Count; i++)
				{
					var n = targetFolder + "biomes_splat_map_" + i + ".png";
					biomesSplatMapChanneledTextures[i].Save(n);
				}
				temperatureMap.Save(targetFolder + "temperature_map.png");
				humidityMap.Save(targetFolder + "humidity_map.png");
			}
		}

		float DistanceToValueUnder(Texture tex, int myX, int myY, float theta)
		{
			{
				var c = tex.GetTexel(myX, myY);
				if (c.x < theta) return 0;
			}

			float size = distanceToSieaIncreasePerPixel;
			while (size < maxDistanceToSea)
			{
				var max = (size * 4.0f).CeilToInt();
				for (int i = 0; i <= max; i++)
				{
					var f = i / (float)max * Math.PI * 2;
					var x = (Math.Cos(f) * size).RoundToInt();
					var y = (Math.Sin(f) * size).RoundToInt();
					var c = tex.GetTexel(myX + x, myY + y); // bottom square edge
					if (c.x < theta) return size;
				}

				size += distanceToSieaIncreasePerPixel;
			}

			return maxDistanceToSea;
		}

	}
}

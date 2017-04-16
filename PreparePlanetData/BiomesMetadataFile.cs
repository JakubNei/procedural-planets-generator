using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PreparePlanetData
{

	public class BiomesMetadataFile
	{
		[Serializable()]
		public class BiomeMetadata
		{
			[XmlElement(Type = typeof(XmlColor))]
			public Color Color { get; set; }
			[System.Xml.Serialization.XmlElement()]
			public string SplatMapName { get; set; }
			[System.Xml.Serialization.XmlElement()]
			public int SplatMapChannel { get; set; }
			[System.Xml.Serialization.XmlElement()]
			public int BiomeId { get; set; }

			// based on: http://stackoverflow.com/a/4322461/782022
			public class XmlColor
			{
				private Color color_ = Color.Black;

				public XmlColor() { }
				public XmlColor(Color c) { color_ = c; }


				public Color ToColor()
				{
					return color_;
				}

				public void FromColor(Color c)
				{
					color_ = c;
				}

				public static implicit operator Color(XmlColor x)
				{
					return x.ToColor();
				}

				public static implicit operator XmlColor(Color c)
				{
					return new XmlColor(c);
				}


				[XmlAttribute]
				public byte Red
				{
					get { return color_.R; }
					set
					{
						if (value != color_.G)
							color_ = Color.FromArgb(color_.A, value, color_.G, color_.B);
					}
				}

				[XmlAttribute]
				public byte Green
				{
					get { return color_.G; }
					set
					{
						if (value != color_.G)
							color_ = Color.FromArgb(color_.A, color_.R, value, color_.B);
					}
				}

				[XmlAttribute]
				public byte Blue
				{
					get { return color_.B; }
					set
					{
						if (value != color_.B)
							color_ = Color.FromArgb(color_.A, color_.R, color_.G, value);
					}
				}

				public bool ShouldSerializeAlpha() { return Alpha < 0xFF; }
				[XmlAttribute]
				public byte Alpha
				{
					get { return color_.A; }
					set
					{
						if (value != color_.A)
							color_ = Color.FromArgb(value, color_.R, color_.G, color_.B);
					}
				}

			}
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

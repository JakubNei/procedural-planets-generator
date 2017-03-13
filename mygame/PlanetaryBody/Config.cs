using MyEngine;
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

		public Texture2D baseHeightMap;
		public double baseHeightMapMultiplier;

		public double noiseMultiplier;


		public void SetTo(UniformsData uniforms)
		{
			uniforms.Set("param_radiusMin", (double)radiusMin);
			uniforms.Set("param_baseHeightMap", baseHeightMap);
			uniforms.Set("param_baseHeightMapMultiplier", (double)baseHeightMapMultiplier);
			uniforms.Set("param_noiseMultiplier", (double)noiseMultiplier);
		}
	}
}

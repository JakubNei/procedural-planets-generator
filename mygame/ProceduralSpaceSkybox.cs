using MyEngine;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyGame
{
	public class ProceduralSpaceSkybox
	{
		public class Result
		{
			public Cubemap[] cubemaps;
		}

		int w = 1024;
		int h = 1024;

		SceneSystem scene;
		Factory Factory => scene.Factory;
		Debug Debug => scene.Debug;

		public Cubemap cubemap { get; private set; }

		public ProceduralSpaceSkybox(SceneSystem scene)
		{
			this.scene = scene;

			cubemap = new Cubemap(w, h);

			var r = new Random();
			for (int i = 0; i < 1000; i++)
			{
				AddStar(
					new Vector3(
						(float)r.NextDouble() * 2 - 1,
						(float)r.NextDouble() * 2 - 1,
						(float)r.NextDouble() * 2 - 1
					),
					Color.White,
					0.5f + (float)r.NextDouble()
				);
			}

			var t = new Thread(Render);
			//t.Start();
			Render();
		}

		public void AddStar(Vector3 direction, Color color, float size)
		{
			stars.Add(new Star()
			{
				dir = direction.Normalized(),
				color = color,
				size = size,
				checkDistanceRadius = size / (float)w / (float)h,
			});
		}

		struct Star
		{
			public Vector3 dir;
			public Color color;
			public float size;
			public float checkDistanceRadius;
		}

		System.Collections.Generic.List<Star> stars = new System.Collections.Generic.List<Star>();

		void Render()
		{
			//var projection = Matrix4.CreatePerspectiveFieldOfView(60, 1, 0, 100);
			var time = new System.Diagnostics.Stopwatch();
			time.Start();
			Debug.Info("start");

			var faces = new[] {
				new {
					face = Cubemap.Face.PositiveX,
					getPos = (Func<float, float, Vector3>)((float x, float y) => { return new Vector3(+1, x, y); }),
				},
				new {
					face = Cubemap.Face.NegativeX,
					getPos = (Func<float, float, Vector3>)((float x, float y) => { return new Vector3(-1, x, y); }),
				},
				new {
					face = Cubemap.Face.PositiveY,
					getPos = (Func<float, float, Vector3>)((float x, float y) => { return new Vector3(x, +1, y); }),
				},
				new {
					face = Cubemap.Face.NegativeY,
					getPos = (Func<float, float, Vector3>)((float x, float y) => { return new Vector3(x, -1, y); }),
				},
				 new {
					face = Cubemap.Face.PositiveZ,
					getPos = (Func<float, float, Vector3>)((float x, float y) => { return new Vector3(x, y, +1); }),
				},
				new {
					face = Cubemap.Face.NegativeZ,
					getPos = (Func<float, float, Vector3>)((float x, float y) => { return new Vector3(x, y, -1); }),
				},
			};

			var r = new Random();

			for (int x = 0; x < w; x++)
			{
				for (int y = 0; y < h; y++)
				{
					var fx = x / (float)w;
					var fy = y / (float)h;

					for (int f = 0; f < 6; f++)
					{
						var pos = faces[f].getPos(fx, fy);
						pos.Normalize();

						var face = faces[f].face;

						for (int i = 0; i < stars.Count; i++)
						{
							var d = stars[i].dir.DistanceSqr(pos);
							if (d > stars[i].checkDistanceRadius) continue;

							d = 1.0f - MyMath.Clamp(d * w * h / stars[i].size, 0.0f, 1.0f);

							//var c = cubemap.GetPixel(face, x, y);
							var s = stars[i].color;
							s = Color.FromArgb((int)(d * 255), s.R, s.G, s.B);
							//c = c.Blend(s);

							cubemap.SetPixel(face, x, y, s);
							//cubemap.SetPixel(face, x, y, Color.Red);
						}

						//cubemap.SetPixel(face, x, y, Color.Red);
					}
				}
			}

			Debug.Info("done in " + time.Elapsed.TotalSeconds + " seconds");
		}
	}
}
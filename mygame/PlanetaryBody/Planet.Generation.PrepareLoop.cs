using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;


namespace MyGame.PlanetaryBody
{
	public partial class Planet
	{

		void GatherWeights(WeightedSegmentsList toGenerate, Segment segment, int recursionDepth)
		{
			var weight = segment.GetGenerationWeight(Camera);

			if (segment.GenerationBegan == false)
			{
				toGenerate.Add(segment, weight);
			}

			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				if (weight > WeightNeededToSubdivide)
				{
					segment.EnsureChildrenAreCreated();
					segment.ShouldChildrenBeVisible(true);
					foreach (var child in segment.Children)
					{
						GatherWeights(toGenerate, child, recursionDepth + 1);
					}
				}
				else
				{
					segment.ShouldChildrenBeVisible(false);
				}
			}
			else
			{
				//Log.Warn("recursion depth is over: " + SubdivisionMaxRecurisonDepth);
			}
		}


		// return true if all childs are visible
		// we can hide parent only once all 4 childs are generated
		// we have to show all 4 childs at once
		void UpdateVisibility(Segment segment, WeightedSegmentsList toGenerate, int recursionDepth)
		{
			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				var canAllChildrenBeVisible = segment.Children.Count > 0 && segment.Children.All(c => c.ShouldBeVisible && c.IsGenerationDone);

				// hide only if all our childs are visible, they might still be generating or they might want to be hidden
				if (canAllChildrenBeVisible)
				{

					foreach (var child in segment.Children)
					{
						UpdateVisibility(child, toGenerate, recursionDepth + 1);
					}
					segment.SetVisible(false);

					return;
				}
			}

			segment.SetVisible(true);

			if (Generateheights.Version != segment.meshGeneratedWithShaderVersion)
				toGenerate.Add(segment, float.MaxValue);
		}


		// new SortedList<double, Chunk>(ReverseComparer<double>.Default)
		class WeightedSegmentsList : Dictionary<Segment, double>
		{
			public Camera cam;

			public new void Add(Segment chunk, double weight)
			{
				PrivateAdd1(chunk, weight);

				// we have to generate all our parents first
				while (chunk.parent != null && chunk.parent.GenerationBegan == false)
				{
					chunk = chunk.parent;
					var w = chunk.GetGenerationWeight(cam);
					PrivateAdd1(chunk, Math.Max(w, weight));
				}
			}
			private void PrivateAdd1(Segment segment, double weight)
			{
				PrivateAdd2(segment, weight);

				// if we want to show this chunk, our neighbours have the same weight, because we cant be shown without our neighbours
				if (segment.parent != null)
				{
					foreach (var neighbour in segment.parent.Children)
					{
						if (neighbour.GenerationBegan == false)
						{
							var w = neighbour.GetGenerationWeight(cam);
							PrivateAdd2(neighbour, Math.Max(w, weight));
						}
					}
				}
			}
			private void PrivateAdd2(Segment segment, double weight)
			{
				double w;
				if (this.TryGetValue(segment, out w))
				{
					if (w > weight) return; // the weight already present is bigger, dont change it
				}

				this[segment] = weight;
			}
			public IEnumerable<Segment> GetWeighted()
			{
				return this.OrderByDescending(i => i.Value).Take(100).Select(i => i.Key);
			}
			public Segment GetMostImportantChunk()
			{
				return this.OrderByDescending(i => i.Value).FirstOrDefault().Key;
			}
		}

		Queue<Segment> toGenerateOrdered;
		WeightedSegmentsList toGenerate;
		public void TrySubdivideOver(WorldPos pos)
		{
			if (toGenerate == null)
				toGenerate = new WeightedSegmentsList() { cam = Camera };
			toGenerate.Clear();

			foreach (var rootSegment in rootSegments)
			{
				if (rootSegment.GenerationBegan == false)
				{
					// first generate rootCunks
					toGenerate.Add(rootSegment, float.MaxValue);
				}
				else
				{
					// then their children
					GatherWeights(toGenerate, rootSegment, 0);
				}
			}

			foreach (var rootSegment in this.rootSegments)
			{
				UpdateVisibility(rootSegment, toGenerate, 0);
			}


			Debug.AddValue("generation / segments to generate", toGenerate.Count);
			toGenerateOrdered = new Queue<Segment>(toGenerate.GetWeighted());
		}


	}
}

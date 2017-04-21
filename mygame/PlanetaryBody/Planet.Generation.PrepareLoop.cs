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
					segment.SetWantsToBeVisible(false);

					segment.EnsureChildrenAreCreated();

					foreach (var child in segment.Children)
					{
						GatherWeights(toGenerate, child, recursionDepth + 1);
					}
					return;
				}
			}

			segment.SetWantsToBeVisible(true);
		}


		static bool AnyDescendantWantsToBeVisible(Segment segment)
		{
			if (segment.WantsToBeVisible) return true;
			if (segment.Children.Count > 0 && segment.Children.Any(c => AnyDescendantWantsToBeVisible(c))) return true;
			return false;
		}

		// return true if all childs are visible
		// we can hide parent only once all 4 childs are generated
		// we have to show all 4 childs at once
		void UpdateVisibility(Segment segment, WeightedSegmentsList toGenerate, int recursionDepth)
		{
			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				var canAllChildrenBeVisible = segment.Children.Count > 0 && segment.Children.All(c => c.IsGenerationDone);
				// hide only if all our childs are visible, they might still be generating or they might want to be hidden
				if (canAllChildrenBeVisible)
				{
					var anyDescendantWantsToBeVisible = AnyDescendantWantsToBeVisible(segment);
					if (anyDescendantWantsToBeVisible)
					{
						foreach (var child in segment.Children)
						{
							UpdateVisibility(child, toGenerate, recursionDepth + 1);
						}

						segment.SetVisible(false);
						return;
					}
				}
			}

			segment.SetVisible(true);
		}


		// new SortedList<double, Chunk>(ReverseComparer<double>.Default)
		class WeightedSegmentsList : Dictionary<Segment, double>
		{
			public Camera cam;

			public new void Add(Segment segment, double weight)
			{
				PrivateAdd1(segment, weight);

				// we have to generate all our parents first
				while (segment.parent != null && segment.parent.GenerationBegan == false)
				{
					segment = segment.parent;
					var w = segment.GetGenerationWeight(cam);
					PrivateAdd1(segment, Math.Max(w, weight));
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
				if (segment.GenerationBegan) return;

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

			ShadersReloadedCheck();

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

		public class VersionWatcher
		{

			public event Action OnAnyVersionChanged;

			Dictionary<IHasVersion, ulong> watch = new Dictionary<IHasVersion, ulong>();
			public void Watch(IHasVersion v)
			{
				this.watch[v] = v.Version;
			}

			public void Tick()
			{
				bool anyVersionChanged = false;
				foreach(var kvp in watch)
				{
					var version = kvp.Value;
					var hasVersion = kvp.Key;

					if(version != hasVersion.Version)
					{
						anyVersionChanged = true;
						break;
					}
				}

				if(anyVersionChanged)
				{
					foreach (var hasVersion in watch.Keys.ToArray())
					{
						watch[hasVersion] = hasVersion.Version;
					}
					OnAnyVersionChanged.Raise();
				}

			}
		}


		VersionWatcher versionWatcher = new VersionWatcher();

		void InitializePrepareLoop()
		{
			versionWatcher.Watch(GenerateSurface);
			versionWatcher.Watch(GenerateBiomes);
			versionWatcher.Watch(GenerateSea);
			versionWatcher.Watch(config.biomesControlMap);

			versionWatcher.OnAnyVersionChanged += () =>
			{
				foreach (var s in this.rootSegments)
					s.MarkForRegeneration();
			};
		}

		void ShadersReloadedCheck()
		{
			versionWatcher.Tick();
		}


	}
}

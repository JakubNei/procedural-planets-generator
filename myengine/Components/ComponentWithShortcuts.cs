using Neitri;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEngine.Components
{
	public abstract class ComponentWithShortcuts : Component
	{
		public InputSystem Input => Entity.Input;
		public Transform Transform => Entity.Transform;

		public SceneSystem Scene => Entity.Scene;

		public Factory Factory => Entity.Factory;
		public Debug Debug => Entity.Debug;

		public IDependencyManager Dependency => Entity.Dependency;

		public ComponentWithShortcuts(Entity entity) : base(entity)
		{
		}
	}
}
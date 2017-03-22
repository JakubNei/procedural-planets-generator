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
		public MyDebug Debug => Entity.Debug;

        public FileSystem FileSystem => Entity.FileSystem;

		public Events.EventSystem EventSystem => Entity.EventSystem;

		public IDependencyManager Dependency => Entity.Dependency;

		public ComponentWithShortcuts(Entity entity) : base(entity)
		{
		}
	}
}